using AutoMapper;
using BusinessLayer.Interface;
using ModelLayer.Model;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;
using BusinessLayer.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Service
{
    public class UserBL : IUserBL
    {
        private readonly IUserRL _userRL;
        private readonly IMapper _mapper;

        public UserBL(IUserRL userRL, IMapper mapper)
        {
            _userRL = userRL;
            _mapper = mapper;
        }

        public UserEntity RegisterUser(UserDTO userDTO)
        {
            // Check if user already exists
            var existingUser = _userRL.GetUserByEmail(userDTO.Email);
            if (existingUser != null)
            {
                throw new Exception("User already exists!");
            }

            // Hash Password
            string hashedPassword = PasswordHasher.HashPassword(userDTO.Password);

            // Map DTO to Entity
            var userEntity = _mapper.Map<UserEntity>(userDTO);
            userEntity.PasswordHash = hashedPassword; // Save hashed password

            // Save user to database
            return _userRL.AddUser(userEntity);
        }

        public UserEntity LoginUser(string email, string password)
        {
            var user = _userRL.GetUserByEmail(email);
            if (user == null || !PasswordHasher.VerifyPassword(password, user.PasswordHash))
            {
                throw new Exception("Invalid email or password!");
            }

            return user;
        }

        public UserEntity GetUserByEmail(string email)
        {
            return _userRL.GetUserByEmail(email);
        }

        public void UpdateUser(UserEntity user)
        {
            _userRL.UpdateUser(user);
        }

        public UserEntity GetUserByResetToken(string token)
        {
            return _userRL.GetUserByResetToken(token);
        }

    }
}
