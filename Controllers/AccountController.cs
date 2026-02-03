using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CoWorkManager.Models;
using CoWorkManager.Models.ViewModels;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq;

namespace CoWorkManager.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager,
                                 SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email!,
                Email = model.Email!,
                FullName = model.FullName
            };

            var result = await _userManager.CreateAsync(user, model.Password!);

            if (result.Succeeded)
            {
                // Assign default role (User)
                var roleResult = await _userManager.AddToRoleAsync(user, "User");
                if (!roleResult.Succeeded)
                {
                    foreach (var err in roleResult.Errors)
                        ModelState.AddModelError(string.Empty, err.Description);
                    return View(model);
                }

                var claimsToAdd = new[]
                {
                    new Claim("MembershipLevel", "1")
                }.ToList();

                if (!string.IsNullOrWhiteSpace(user.FullName))
                {
                    if (!claimsToAdd.Any(c => c.Type == "FullName"))
                        claimsToAdd.Add(new Claim("FullName", user.FullName));
                }

                
                var existingClaims = await _userManager.GetClaimsAsync(user);
                foreach (var claim in claimsToAdd)
                {
                    if (!existingClaims.Any(c => c.Type == claim.Type && c.Value == claim.Value))
                    {
                        var addClaimResult = await _userManager.AddClaimAsync(user, claim);
                        if (!addClaimResult.Succeeded)
                        {
                            foreach (var err in addClaimResult.Errors)
                                ModelState.AddModelError(string.Empty, err.Description);
                            return View(model);
                        }
                    }
                }

         
                await _signInManager.SignInAsync(user, isPersistent: false);

                return RedirectToAction("Dashboard", "User");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email!, model.Password!, model.RememberMe, lockoutOnFailure: false
            );

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email!);
                if (user != null)
                {
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                        return RedirectToAction("Dashboard", "Admin");

                    return RedirectToAction("Dashboard", "User");
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public IActionResult Profile()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
