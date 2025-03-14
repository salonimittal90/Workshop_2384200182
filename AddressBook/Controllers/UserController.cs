using BusinessLayer.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.Model;
using BusinessLayer.Helper;

namespace AddressBook.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserBL _userBL;
        private readonly JwtTokenService _jwtTokennService;
        private readonly IEmailService _emailService;


        public UserController(IUserBL userService, JwtTokenService jwtTokennService, IEmailService emailService)
        {
            _userBL = userService;
            _jwtTokennService = jwtTokennService;
            _emailService = emailService;
        }

        [HttpPost("register")]
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
        }


        [HttpPost("login")]
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
