using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Webserver.Helpers;
using Webserver.Models;
using Webserver.Repository;

namespace Webserver.Controllers
{
    [ApiController]
    [Route("api/v1/auth/[controller]/")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly AppSettings _appSettings;
        
        public UserController(IUserRepository userRepository, IMapper mapper, IOptions<AppSettings> appSettings)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _appSettings = appSettings.Value;
        }

        [AllowAnonymous]
        [HttpPost("verification/process")]
        public IActionResult Authenticate([FromBody] AuthenticateModel model)
        {
            var user = _userRepository.Authenticate(model.Email, model.Password);
            
            if (user == null)
                return BadRequest(new {message = "Username or password is incorrect"});
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new []
                {
                    new Claim(ClaimTypes.Name, user.Id)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new
            {
                Token = tokenString
            });
        }

        [HttpPost("imageUpload", Name = "imageUpload")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ImageUpload(IFormFile file, string id)
        {
            var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
            if (extension == ".jpg" || extension == ".png" || extension == "jpeg" || extension == ".gif")
            {
                await _userRepository.UploadProfileImage(file, id);
            }
            else
            {
                return BadRequest(new { message = "Invalid file extension" });
            }
            
            return Ok( new { message = "Updated Success!"});
        }

        [HttpGet("users/display/all/details")]
        public IActionResult GetUser()
        {
            var users = _userRepository.GetUsers();
            var model = _mapper.Map<IList<UserModels>>(users);
            return Ok(model);
        }
        
        [HttpGet("users/find/particular/finding")]
        public IActionResult GetUserByUserName(string username)
        {
            var user = _userRepository.GetUserByUserName(username);

            if (user == null)
                return BadRequest(new {message = "No user was found"});
            
            var model = _mapper.Map<UserModels>(user);
            return Ok(model);
        }

        // [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Mod)]
        [HttpGet("{id:length(24)}", Name = "GetUser")]
        public ActionResult<User> GetUser(string id)
        {
            
            var user = _userRepository.GetUserById(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }
        
        [HttpPost("creation/process/add")]
        public IActionResult Add([FromBody]RegisterModel model)
        {
            var user = _mapper.Map<User>(model);
            
            try
            {
                _userRepository.AddUser(user, model.Password);
                return CreatedAtRoute("GetUser", new {id = user.Id}, user);
            }
            catch (AppException e)
            {
                return BadRequest(new {message = e.Message });
            }
        }
        [Authorize]
        [HttpPut("{id:length(24)}")]
        public IActionResult Edit(string id, [FromBody]UpdateModel model)
        {
            var user = _mapper.Map<User>(model);
            user.Id = id;

            try
            {
                _userRepository.EditUser(id, user, model.Password);
                return Ok();
            }
            catch (AppException e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id)
        {
            var user = _userRepository.GetUserById(id);

            if (user == null)
            {
                return NotFound();
            }
            _userRepository.DeleteUser(user.Id);

            return NoContent();
        }
    }
}