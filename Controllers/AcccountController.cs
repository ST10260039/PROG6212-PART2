using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MonthlyClaimSystem.Models;

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

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string role)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, password, false, false);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrWhiteSpace(role) && await _userManager.IsInRoleAsync(user, role))
                    {
                        return role switch
                        {
                            "Lecturer" => RedirectToAction("SubmitClaim", "Lecturer"),
                            "Coordinator" => RedirectToAction("PendingClaims", "Coordinator"),
                            "Manager" => RedirectToAction("VerifiedClaims", "Manager"),
                            "HR" => RedirectToAction("ManageLecturers", "HR"),
                            _ => RedirectToAction("Login")
                        };
                    }
                    TempData["Error"] = "User is not assigned to the selected role.";
                    return RedirectToAction("Login");
                }
            }
            TempData["Error"] = "Invalid login attempt.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
        //[HttpPost]
        //[Authorize(Roles = "HR")] // only HR can reset passwords
        //public async Task<IActionResult> ResetLecturerPassword()
        //{
        //    var user = await _userManager.FindByEmailAsync("lecturer@cms.com");
        //    if (user == null)
        //    {
        //        return NotFound("User not found.");
        //    }

        //    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        //    var result = await _userManager.ResetPasswordAsync(user, token, "NewPassword123!");

        //    if (result.Succeeded)
        //    {
        //        return Ok("Password reset successfully.");
        //    }
        //    else
        //    {
        //        return BadRequest(result.Errors);
        //    }
        //}
    }
}