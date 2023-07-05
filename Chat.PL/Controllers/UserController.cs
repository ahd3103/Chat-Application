using Chat.BL.DTOs;
using Chat.BL.Servies;
using Chat.DL.Models;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Newtonsoft.Json;
//using static Chat.BL.Helper.RequestLoggingMiddlewareExtensions;

namespace Chat.PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogService _logService;

        public UserController(IUserService userRepository, IConfiguration configuration, ILogService logService)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _logService = logService;
        }
        [Authorize]
        [HttpGet]
        [Route("/api/users")]
        public async Task<IActionResult> GetAll()
        {

            var users = await _userRepository.GetAll();
            var response = users.Select(user => new UserResponse
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email
            });

            await _logService.AddLogs(new Log
            {
                IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
                RequestPath = "GetAll User",
                RequestBody = JsonConvert.SerializeObject(response),
                Message = "User GetAll",
                Timestamp = DateTime.UtcNow,
                Level = "info",
                Username = User.Identity.Name
            });

            return Ok(response);
        }

        [HttpPost]
        [Route("/api/register")]
        public async Task<IActionResult> Insert([FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                var errorResponse = new ErrorResponse
                {
                    Error = "Invalid user data."
                };
                return BadRequest(errorResponse);
            }



            var existingUser = await _userRepository.GetByEmail(user.Email);
            if (existingUser != null)
            {
                var errorResponse = new ErrorResponse
                {
                    Error = "Email is already registered."
                };
                return Conflict(errorResponse);
            }
            else
            {
                user.Id = Guid.NewGuid();
            }

            await _userRepository.Insert(user);

            var response = new UserResponse
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                Message = "Registration successful"
            };

            return Ok(response);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _userRepository.CheckUser(email, password);

            if (user == null)
            {
                return Unauthorized(); // Login failed due to incorrect credentials
            }
            else
            {
                var token = GenerateJwtToken(user.Id);

                var userProfile = new UserResponse
                {
                    UserId = user.Id,
                    Name = user.Name,
                    Email = user.Email
                };

                return Ok(new { Token = token, Profile = userProfile, Massage = "Login successful" });

            }
        }




        private string GenerateJwtToken(Guid userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtSecret = _configuration["JWT:Key"];
            var key = Encoding.ASCII.GetBytes(jwtSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, userId.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(12), // Set the token expiration time
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return tokenString;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("GoogleLogin")]
        public IActionResult GoogleLogin()
        {
            string redirectUrl = Url.Action("GoogleResponse", "Account");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

    }
}
