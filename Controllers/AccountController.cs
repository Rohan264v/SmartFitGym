using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartFitGym.Models;
using System.Security.Claims;

namespace SmartFitGym.Controllers
{
	public class AccountController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly RoleManager<IdentityRole> _roleManager;

		public AccountController(UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager,
			RoleManager<IdentityRole> roleManager)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_roleManager = roleManager;
		}

		// GET: Register
		public IActionResult Register() => View();

		// POST: Register
		[HttpPost]
		public async Task<IActionResult> Register(string fullName, string email,
			string password, string confirmPassword)
		{
			if (password != confirmPassword)
			{
				ViewBag.Error = "Passwords do not match!";
				return View();
			}

			// Create roles if they don't exist
			if (!await _roleManager.RoleExistsAsync("Admin"))
				await _roleManager.CreateAsync(new IdentityRole("Admin"));
			if (!await _roleManager.RoleExistsAsync("Member"))
				await _roleManager.CreateAsync(new IdentityRole("Member"));
			if (!await _roleManager.RoleExistsAsync("Trainer"))
				await _roleManager.CreateAsync(new IdentityRole("Trainer"));

			var user = new ApplicationUser
			{
				FullName = fullName,
				UserName = email,
				Email = email
			};

			var result = await _userManager.CreateAsync(user, password);

			if (result.Succeeded)
			{
				// First user becomes Admin
				var userCount = _userManager.Users.Count();
				if (userCount == 1)
					await _userManager.AddToRoleAsync(user, "Admin");
				else
					await _userManager.AddToRoleAsync(user, "Member");

				await _signInManager.SignInAsync(user, isPersistent: false);
				return RedirectToAction("Index", "Home");
			}

			ViewBag.Error = string.Join(", ", result.Errors.Select(e => e.Description));
			return View();
		}

		// GET: Login
		public IActionResult Login() => View();

		// POST: Login
		[HttpPost]
		public async Task<IActionResult> Login(string email, string password, bool rememberMe)
		{
			var result = await _signInManager.PasswordSignInAsync(email, password, rememberMe, false);

			if (result.Succeeded)
			{
				var user = await _userManager.FindByEmailAsync(email);
				var roles = await _userManager.GetRolesAsync(user!);

				if (roles.Contains("Admin"))
					return RedirectToAction("Index", "Admin");
				else
					return RedirectToAction("Dashboard", "Member");
			}

			ViewBag.Error = "Invalid email or password!";
			return View();
		}

		// Logout
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			return RedirectToAction("Index", "Home");
		}

        // POST: ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        // GET: ExternalLoginCallback
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ViewBag.Error = $"Error from external provider: {remoteError}";
                return View("Login");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                return RedirectToLocal(returnUrl);
            }

            // If user doesn't have an account, create one
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var name = info.Principal.FindFirstValue(ClaimTypes.Name);

            if (email != null)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new ApplicationUser { UserName = email, Email = email, FullName = name ?? "User" };
                    var createResult = await _userManager.CreateAsync(user);
                    if (createResult.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "Member");
                        await _userManager.AddLoginAsync(user, info);
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                else
                {
                    await _userManager.AddLoginAsync(user, info);
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToLocal(returnUrl);
                }
            }

            ViewBag.Error = "Error loading external login information.";
            return View("Login");
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
	}
}
