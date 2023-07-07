using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Chat.BL.Helper
{
    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public TokenValidationMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
       
            if (token != null)
            {
                await ValidateToken(context, token);
                // Remove the existing Authorization header
                context.Request.Headers.Remove("Authorization");

                // Add the updated Authorization header with the token
                context.Request.Headers.Add("Authorization", token);
            }

            await _next(context);
        }

        private async Task ValidateToken(HttpContext context, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtSecret = _configuration["JWT:Key"];
                var key = Encoding.ASCII.GetBytes(jwtSecret);

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false, // Set to true if you want to validate the issuer
                    ValidateAudience = false, // Set to true if you want to validate the audience
                    ClockSkew = TimeSpan.Zero // Set the clock skew tolerance
                };

                var claimsPrincipal = tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
                context.User = claimsPrincipal;
            }
            catch (Exception)
            {
                // Token validation failed
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Login failed due to incorrect credentials");
            }
        }

    }

}
