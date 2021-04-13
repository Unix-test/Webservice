using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Webserver.Models;

namespace Webserver.Repository
{
    public interface IUserRepository
    {
        IEnumerable<User> GetUsers();

        User Authenticate(string email, string password);

        User GetUserById(string id);
        
        User GetUserByUserName(string finding);
        
        User AddUser(User user, string password);

        Task<bool> UploadProfileImage(IFormFile file, string id);

        void EditUser(string id, User updateUser, string password = null);
        
        void DeleteUser(string id);
        
    }
}