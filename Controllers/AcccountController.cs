using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using MonthlyClaimSystem.Models;
using System.Threading.Tasks;

namespace MonthlyClaimSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string role)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, password, false, false);
                if (result.Succeeded)
                {
                    // Redirect based on role
                    if (await _userManager.IsInRoleAsync(user, "Lecturer"))
                        return RedirectToAction("SubmitClaim", "Lecturer");

                    if (await _userManager.IsInRoleAsync(user, "Coordinator"))
                        return RedirectToAction("PendingClaims", "Coordinator");

                    if (await _userManager.IsInRoleAsync(user, "Manager"))
                        return RedirectToAction("VerifiedClaims", "Manager");

                    if (await _userManager.IsInRoleAsync(user, "HR"))
                        return RedirectToAction("ManageLecturers", "HR");
                }
            }

            TempData["Error"] = "Invalid login attempt.";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
    }
}