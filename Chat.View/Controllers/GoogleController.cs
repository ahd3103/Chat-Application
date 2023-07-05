
using Chat.View.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Chat.PL.Controllers
{
    public class GoogleController : Controller
    {
        private UserManager<AppUser> userManager;
        private SignInManager<AppUser> signInManager;

        public GoogleController(UserManager<AppUser> userMgr, SignInManager<AppUser> signinMgr)
        {
            userManager = userMgr;
            signInManager = signinMgr;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]        
        public IActionResult GoogleLogin()
        {
            string redirectUrl = Url.Action("GoogleResponse", "Account");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpPost] // Specify the HTTP method explicitly
        [AllowAnonymous]
        [Route("GoogleResponse")]
        public async Task<IActionResult> GoogleResponse()
        {
            ExternalLoginInfo info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                // Handle the case when external login info is null
                // You can redirect the user to an appropriate page or return an error response
                return BadRequest("Failed to retrieve external login information.");
            }

            var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
            string[] userInfo = { info.Principal.FindFirst(ClaimTypes.Name).Value, info.Principal.FindFirst(ClaimTypes.Email).Value };

            if (result.Succeeded)
            {
                return Ok(userInfo);
            }
            else
            {
                AppUser user = new AppUser
                {
                    Email = info.Principal.FindFirst(ClaimTypes.Email).Value,
                    UserName = info.Principal.FindFirst(ClaimTypes.Email).Value
                };

                IdentityResult identResult = await userManager.CreateAsync(user);

                if (identResult.Succeeded)
                {
                    identResult = await userManager.AddLoginAsync(user, info);

                    if (identResult.Succeeded)
                    {
                        await signInManager.SignInAsync(user, false);
                        return Ok(userInfo);
                    }
                }

                // Handle the case when user creation or login fails
                // You can redirect the user to an appropriate page or return an error response
                return BadRequest("Failed to create user or sign in.");
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("AccessDenied")]
        public IActionResult AccessDenied()
        {
            // Handle access denied scenario
            // You can redirect the user to an appropriate page or return an error response
            return BadRequest("Access denied.");
        }
    }
}
