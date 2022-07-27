using AuthFirst.Models;
using AuthFirst.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AuthFirst.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserAccess _useraccess;

        public HomeController(ILogger<HomeController> logger, UserAccess useraccess)
        {
            _logger = logger;
            _useraccess = useraccess;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Secured()
        {
            //var token = await HttpContext.GetTokenAsync("id_token");
            return View();
        }

        [HttpGet("login")]
        public IActionResult Login(string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpGet("login/{provider}")]
        public IActionResult LoginWithProvider([FromRoute] string provider, [FromQuery] string returnUrl)
        {
            if(User != null && User.Identities.Any(identity => identity.IsAuthenticated))
            {
                RedirectToAction("", "Home");
            }
            returnUrl = string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl;

            var authenticationProperties = new AuthenticationProperties { RedirectUri = returnUrl };
            // authenticationProperties.SetParameter("prompt", "select-account");
            // await HttpContext.ChallengeAsync(provider, authenticationProperties).ConfigureAwait(false);

            return new ChallengeResult(provider, authenticationProperties);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Validate(string username, string password, string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if(_useraccess.TryValidateUser(username, password, out List<Claim> claims))
            {
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                IDictionary<string, string> items = new Dictionary<string, string>();
                items.Add(".AuthScheme", CookieAuthenticationDefaults.AuthenticationScheme);
                var properties = new AuthenticationProperties(items);
                await HttpContext.SignInAsync(claimsPrincipal, properties);
                return Redirect(returnUrl);
            }
            else
            {
                TempData["error"] = "Error. Invalid Username or Password";
                return View("login");
            }
        }

        [HttpGet("denied")]
        public IActionResult Denied()
        {
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            var scheme = User.Claims.FirstOrDefault(t => t.Type == ".AuthScheme").Value;
            if(scheme == "google")
            {
                await HttpContext.SignOutAsync();
                return Redirect("https://www.google.com/accounts/logout?continue=https://appengine.google.com/_ah/logout?continue=https://localhost:5001");
            }else if(scheme == "Cookies")
            {
                await HttpContext.SignOutAsync();
                return Redirect("/");
            }
            else
            {
                return new SignOutResult(new[] { CookieAuthenticationDefaults.AuthenticationScheme, scheme });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
