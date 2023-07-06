using Chat.BL.DTOs;
using Chat.BL.Servies;
using Chat.DL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Chat.PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userRepository;
        private readonly IConfiguration _configuration;
        //private readonly ILogService _logService;

        public UserController(IUserService userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            //_logService = logService;
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

        [Authorize]
        [HttpGet]
        [Route("GetLogs")]
        public IActionResult logs([FromQuery] long? startDateTime = null, long? endDateTime = null)
        {
            try
            {
                if (startDateTime == null)
                {
                    DateTime dateTime = DateTime.Now.AddMinutes(5);
                    DateTimeOffset dateTimeOffset = new DateTimeOffset(dateTime);
                    startDateTime = dateTimeOffset.ToUnixTimeSeconds();
                }
                else if (endDateTime == null)
                {
                    DateTime dateTime = DateTime.Now;
                    DateTimeOffset dateTimeOffset = new DateTimeOffset(dateTime);
                    endDateTime = dateTimeOffset.ToUnixTimeSeconds();
                }
                else if (startDateTime >= endDateTime)
                {
                    return BadRequest("startDateTime must be smaller than endDateTime");
                }

                // Retrieve the values from the HttpContext
                var value = HttpContext.Items["logMessages"];

                object v = HttpContext.Items["lstLogResponses"];
                List<LogResponse> lstLogResponse1 = (List<LogResponse>)v;

                if (lstLogResponse1 == null || lstLogResponse1.Count == 0)
                {
                    return NotFound();
                }

                List<LogResponse> logRes = lstLogResponse1.Where(log => log.TimeOfCall >= startDateTime && log.TimeOfCall <= endDateTime).ToList();

                if (logRes.Count == 0)
                {
                    return NotFound();
                }

                return Ok(logRes);
            }
            catch (Exception ex)
            {
                return BadRequest();
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
    }
}
