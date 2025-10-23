using Microsoft.AspNetCore.Mvc;
using System.Linq;
using MonthlyClaimSystem.Models;
using MonthlyClaimSystem.Services;

namespace MonthlyClaimSystem.Controllers
{
    public class CoordinatorController : Controller
    {
        public IActionResult PendingClaims()
        {
            var claims = ClaimService.GetAll().Where(c => c.Status == "Pending").ToList();
            return View(claims);
        }

        [HttpPost]
        public IActionResult VerifyClaim(int id)
        {
            var claim = ClaimService.GetById(id);
            if (claim != null)
            {
                claim.Status = "Verified";
                claim.IsVerified = true;
                ClaimService.Update(claim);
            }
            return RedirectToAction("PendingClaims");
        }

        [HttpPost]
        public IActionResult RejectClaim(int id)
        {
            var claim = ClaimService.GetById(id);
            if (claim != null)
            {
                claim.Status = "Rejected";
                ClaimService.Update(claim);
            }
            return RedirectToAction("PendingClaims");
        }
    }
}