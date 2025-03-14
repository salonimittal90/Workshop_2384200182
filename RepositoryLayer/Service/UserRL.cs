using RepositoryLayer.Context;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Service
{
    public class UserRL : IUserRL
    {
        private readonly AppDbContext _context;
        public UserRL(AppDbContext context)
        {
             _context = context;
        }

        public UserEntity AddUser(UserEntity user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
             return user;
        }

        public UserEntity GetUserByEmail(string email)
        {
                return _context.Users.FirstOrDefault(u => u.Email == email);
        }

        public void UpdateUser(UserEntity user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
        }

        public UserEntity GetUserByResetToken(string token)
        {
            return _context.Users.FirstOrDefault(u => u.ResetToken == token);
        }

    }
}
