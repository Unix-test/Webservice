using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using Webserver.Helpers;
using Webserver.Models;

namespace Webserver.Repository
{
 
    public class UserService: IUserRepository
    {
        private readonly IMongoCollection<User> _users;

        public UserService(ISocialiteDatabaseSettings setting)
        {
            var client = new MongoClient(setting.ConnectionString);
            var database = client.GetDatabase(setting.DatabaseName);
            _users = database.GetCollection<User>(setting.CollectionName);
        }
        
        public IEnumerable<User> GetUsers() => _users.Find(user => true).ToEnumerable();
        
        public User Authenticate(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return null;
            }

            var users = _users.Find(user => user.Email == email).FirstOrDefault();

            if (users == null)
            {
                return null;
            }
            
            //check password section
            return !VerifyHashedPassword(password, users.PasswordHash, users.PasswordSalt) ? null : users;
        }
        
        
        #region FindingUser
        public User GetUserById(string id) => _users.Find(user => user.Id == id).FirstOrDefault();

        public User GetUserByUserName(string finding)
        {
            
            if (string.IsNullOrEmpty(finding))
                return null;
            
            var username = _users.Find(user => user.Username.Contains(finding)).FirstOrDefault();

            return username;
        }
        #endregion

        public User AddUser(User user, string password)
        {
            var timestamp = DateTime.UtcNow.ToLocalTime();
            const string role = UserRoles.User;

            if(string.IsNullOrWhiteSpace(password))
                throw new AppException("Password is required");
            
            if(_users.Find(x => x.Username == user.Username).FirstOrDefault() != null)
                throw new AppException("Username \"" + user.Username + "\" is already taken");
            
            CreatePasswordHash(password, out var passwordHash, out var passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.Role = role;
            user.CreateAt = timestamp;

            _users.InsertOne(user);
            return user;
        }

        #region Uploadfunction
        public async Task<Boolean> UploadProfileImage(IFormFile file, string id)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Id, id);
            var options = new UpdateOptions {IsUpsert = true};

            try
            {
                // var userId = GetId(id);
                var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
                var fileName = DateTime.Now.Ticks + extension;
                
                var pathBuilt = Path.Combine(Directory.GetCurrentDirectory(), "Upload/Profiles");

                if (!Directory.Exists(pathBuilt))
                {
                    Directory.CreateDirectory(pathBuilt);
                }

                if (file.Length > 0)
                {
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "Upload/Profiles", fileName);
                    var stream = new FileStream(path, FileMode.Create);
                    await file.CopyToAsync(stream);
                    var update = Builders<User>.Update.Push(x => x.ImageProfile, path);
                    await _users.UpdateOneAsync(filter, update, options);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return true;
        }
        #endregion
        
        public void DeleteUser(User deleteUser) => _users.DeleteOneAsync(user => user.Id == deleteUser.Id);

        public void EditUser(string id, User updateUser, string password = null)
        {
            var user = _users.Find(users => users.Id == id).SingleOrDefault();
            
            if(user == null)
                throw new AppException("User's not found");

            if (!string.IsNullOrWhiteSpace(updateUser.Username) && updateUser.Username != user.Username)
            {
                if(_users.Find(x => x.Username == updateUser.Username).SingleOrDefault() != null)
                    throw new AppException("Username " + updateUser.Username + " is already taken");
                
                user.Username = updateUser.Username;
            }
            
            //update user properties if provide
            if (!string.IsNullOrWhiteSpace(updateUser.Firstname))
                user.Firstname = updateUser.Lastname;

            if (!string.IsNullOrWhiteSpace(updateUser.Lastname))
                user.Lastname = updateUser.Lastname;

            if (!string.IsNullOrWhiteSpace(updateUser.Email))
                user.Email = updateUser.Email;

            if (!string.IsNullOrWhiteSpace(password))
            {
                CreatePasswordHash(password, out var passwordHash, out var passwordSalt);
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }
            
            _users.ReplaceOneAsync(users => users.Id == id, user);
        }
        
        public void DeleteUser(string id) => _users.DeleteOneAsync(user => user.Id == id);

        #region decodeToken
        private string GetId(string tokenString)
        {
            var jwtEncodedString = tokenString;
            var token = new JwtSecurityToken(jwtEncodedString);
            return token.Claims.First(c => c.Type == "_id").Value;
        }
        #endregion
        
        #region Encrypt
        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.",nameof(password));

            using var hmac = new System.Security.Cryptography.HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }
        #endregion
        
        #region Decrypt
        private static bool VerifyHashedPassword(string password, byte[] storedHash, byte[] storedSalt)
        {
            if(password == null) throw new  ArgumentNullException(nameof(password));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", nameof(password));
            if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).");
            if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).");

            using var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt);
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

            return !computedHash.Where((t, i) => t != storedHash[i]).Any();
        }
        #endregion
    }
}