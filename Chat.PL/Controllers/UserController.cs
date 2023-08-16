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
        
        public UserController(IUserService userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;            
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
        [HttpPost("logs")] 
        public async Task<IActionResult> logs([FromBody] ReqLog reqLog)
        {
            try
            {
                if (reqLog.StartTime == null)
                {
                    DateTime dateTime = DateTime.Now.AddMinutes(5);
                    DateTimeOffset dateTimeOffset = new DateTimeOffset(dateTime);
                    reqLog.StartTime = dateTimeOffset.ToUnixTimeSeconds();
                }
                else if (reqLog.EndTime == null)
                {
                    DateTime dateTime = DateTime.Now;
                    DateTimeOffset dateTimeOffset = new DateTimeOffset(dateTime);
                    reqLog.EndTime = dateTimeOffset.ToUnixTimeSeconds();
                }
                else if (reqLog.StartTime >= reqLog.EndTime)
                {
                    return BadRequest("startDateTime must be smaller than endDateTime");
                }

                // Retrieve the values from the HttpContext
                var value = HttpContext.Items["logMessages"];

                object v = HttpContext.Items["lstLogResponses"];
                List<LogResponse> lstLogResponse1 = (List<LogResponse>)v;

                if (lstLogResponse1 == null || lstLogResponse1.Count == 0)
                {
                    return Ok(null); 
                }

                List<LogResponse> logRes = lstLogResponse1.Where(log => log.TimeOfCall >= reqLog.StartTime && log.TimeOfCall <= reqLog.EndTime).ToList();

                if (logRes.Count == 0)
                {
                    return Ok(null);
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
