using BusinessLayer.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.Model;
using BusinessLayer.Helper;
using BusinessLayer.Service;
using RepositoryLayer.Entity;

namespace AddressBook.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly IUserBL _userBL;
        private readonly JwtTokenService _jwtTokennService;
        private readonly IEmailService _emailService;
        private readonly RedisCacheService _redisCacheService;
        private readonly RabbitMQService _rabbitMQService;


        public UserController(IUserBL userService, JwtTokenService jwtTokennService, IEmailService emailService, RedisCacheService redisCacheService, RabbitMQService rabbitMQService)
        {
            _userBL = userService;
            _jwtTokennService = jwtTokennService;
            _emailService = emailService;
            _redisCacheService = redisCacheService;
            _rabbitMQService = rabbitMQService;
        }

        /* [HttpPost("register")]
         public IActionResult Register([FromBody] UserDTO userDTO)
         {
             try
             {
                 var user = _userBL.RegisterUser(userDTO);
                 return Ok(new { message = "User registered successfully!", user });
             }
             catch (Exception ex)
             {
                 return BadRequest(new { error = ex.Message });
             }
         }*/


       // after implement redis
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDTO userDTO)
        {
            try
            {
                // Pehle Redis Cache me check karenge
                var existingUser = await _redisCacheService.GetDataAsync<UserEntity>($"user_{userDTO.Email}");
                if (existingUser != null)
                {
                    return BadRequest(new { message = "User already exists in cache!" });
                }

                var user = _userBL.RegisterUser(userDTO);

                //  Redis Cache me store 
                await _redisCacheService.SetDataAsync($"user_{userDTO.Email}", user, TimeSpan.FromMinutes(30));

                _rabbitMQService.PublishMessage("UserRegisteredQueue", $"New user registered: {userDTO.Email}");

                return Ok(new { message = "User registered successfully!", user });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }






        /*[HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTO loginDTO)
        {
            try
            {
                var user = _userBL.LoginUser(loginDTO.Email, loginDTO.Password);
                var token = _jwtTokennService.GenerateToken(user.Email);
                return Ok(new { message = "Login successful!", user , token});
            }
            catch (Exception ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }
*/
        // after implement redis
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            try
            {
                var cacheKey = $"user_{loginDTO.Email}";
                var cachedUser = await _redisCacheService.GetDataAsync<UserEntity>(cacheKey);

                if (cachedUser != null)
                {
                    var cachedToken = _jwtTokennService.GenerateToken(cachedUser.Email);
                    return Ok(new { message = "Login successful! (from cache)", user = cachedUser, token = cachedToken });
                }

                var user = _userBL.LoginUser(loginDTO.Email, loginDTO.Password);
                var token = _jwtTokennService.GenerateToken(user.Email);

                //  Redis Cache me store 
                await _redisCacheService.SetDataAsync(cacheKey, user, TimeSpan.FromMinutes(30));

                return Ok(new { message = "Login successful!", user, token });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }


        //GetUserByEmail() → User Data Ko Cache Se Retrieve Karne Ke Liye

        [HttpGet("get-user")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            var cacheKey = $"user_{email}";
            var cachedUser = await _redisCacheService.GetDataAsync<UserEntity>(cacheKey);

            if (cachedUser != null)
            {
                cachedUser.Source = "Fetching from redis"; //  Redis se data aa raha hai
                return Ok(cachedUser);
            }

            var user = _userBL.GetUserByEmail(email);
            if (user == null)
            {
                return NotFound(new { message = "User not found!" });
            }

            user.Source = "Fetching from database"; //  Database se data aa raha hai
            //  Redis Cache me store karo
            await _redisCacheService.SetDataAsync(cacheKey, user, TimeSpan.FromMinutes(30));
            return Ok(user);
        }


        //UpdateUser() → Cache Invalidate Karne Ke Liye
        [HttpPut("update-user")]
        public async Task<IActionResult> UpdateUser(UserEntity user)
        {
            _userBL.UpdateUser(user);

            // ✅ Cache Invalidate karo
            await _redisCacheService.RemoveDataAsync($"user_{user.Email}");

            return Ok(new { message = "User updated successfully!" });
        }


        [Authorize]
        [HttpGet("secure-data")]
        public IActionResult GetSecureData()
        {
            var email = User.Identity.Name;
            return Ok("This api is secure");
        }


        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword([FromBody] ForgetPasswordDTO forgotPasswordDTO)
        {
            var user = _userBL.GetUserByEmail(forgotPasswordDTO.Email);
            if (user == null)
            {
                return NotFound(new { message = "User not found!" });
            }

            //  Reset Token Generate Karo
            string resetToken = JwtTokenService.GenerateResetToken();
            user.ResetToken = resetToken;
            user.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(15); // Token valid for 15 min

            _userBL.UpdateUser(user);

            //  Email Send 
            string resetLink = $"https://yourfrontend.com/reset-password?token={resetToken}";
            _emailService.SendEmail(user.Email, "Password Reset", $"Click here to reset your password: {resetLink}");

            return Ok(new { message = "Password reset link sent to your email!" });
        }

        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPasswordDTO resetPasswordDTO)
        {
            var user = _userBL.GetUserByResetToken(resetPasswordDTO.Token);
            if (user == null || user.ResetTokenExpiry < DateTime.UtcNow)
            {
                return BadRequest(new { message = "Invalid or expired reset token!" });
            }

            // ✅ Hash new password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(resetPasswordDTO.NewPassword);
            user.ResetToken = null;  // Token ko invalidate kar do
            user.ResetTokenExpiry = null;

            _userBL.UpdateUser(user);

            return Ok(new { message = "Password reset successfully!" });
        }
    }
}
