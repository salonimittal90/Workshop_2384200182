using RepositoryLayer.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelLayer.Model;

namespace BusinessLayer.Interface
{
    public interface IUserBL
    {
        UserEntity RegisterUser(UserDTO userDTO);
        UserEntity LoginUser(string email, string password);

        UserEntity GetUserByEmail(string email);
        void UpdateUser(UserEntity user);

        UserEntity GetUserByResetToken(string token);
    }
}
