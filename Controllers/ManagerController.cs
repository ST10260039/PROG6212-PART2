using Microsoft.AspNetCore.Mvc;
using System.Linq;
using MonthlyClaimSystem.Models;
using MonthlyClaimSystem.Services;

namespace MonthlyClaimSystem.Controllers
{
    public class ManagerController : Controller
    {
        public IActionResult VerifiedClaims()
        {
            var claims = ClaimService.GetAll().Where(c => c.Status == "Verified").ToList();
            return View(claims);
        }

        //[HttpPost]
        //public IActionResult ApproveClaim(int id)
        //{
        //    var claim = ClaimService.GetById(id);
        //    if (claim != null)
        //    {
        //        claim.Status = "Approved";
        //        ClaimService.Update(claim);
        //    }
        //    return RedirectToAction("VerifiedClaims");
        //}

        [HttpPost]
        public IActionResult RejectClaim(int id)
        {
            var claim = ClaimService.GetById(id);
            if (claim != null)
            {
                claim.Status = "Rejected";
                ClaimService.Update(claim);
            }
            return RedirectToAction("VerifiedClaims");
        }

        [HttpPost]
        public IActionResult ApproveClaim(int id)
        {
            var claim = ClaimService.GetById(id);
            if (claim.VerifyStatus == "Verified")
            {
                claim.ApproveStatus = "Approved";
            }
            else
            {
                claim.ApproveStatus = "Rejected";
            }
            ClaimService.Update(claim);
            return RedirectToAction("VerifiedClaims");
        }
    }
}