using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonthlyClaimSystem.Data;
using MonthlyClaimSystem.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MonthlyClaimSystem.Controllers
{
    [Authorize(Policy = "ManagerPolicy")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ManagerController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        //Show all claims that have been verified by coordinator but not yet approved
        [HttpGet]
        public async Task<IActionResult> VerifiedClaims()
        {
            var claims = await _db.Claims
                .Include(c => c.Documents)
                .Include(c => c.Employee)
                .Where(c => c.VerifyStatus == "Verified" &&
                            c.ApproveStatus == "Pending")
                .OrderBy(c => c.DateSubmitted)
                .ToListAsync();

            return View(claims);
        }

        // Approve claim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveClaim(int id)
        {
            var claim = await _db.Claims.FindAsync(id);
            if (claim == null) return NotFound();

            if (claim.VerifyStatus != "Verified")
            {
                // Manager should only approve verified claims
                return BadRequest("Claim must be verified before approval.");
            }

            claim.ApproveStatus = "Approved";
            claim.ApprovedByUserId = _userManager.GetUserId(User);
            claim.ApprovedOn = DateTime.UtcNow;

            _db.Claims.Update(claim);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(VerifiedClaims));
        }

        //Reject claim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectClaim(int id, string? reason)
        {
            var claim = await _db.Claims.FindAsync(id);
            if (claim == null) return NotFound();

            claim.ApproveStatus = "Rejected";
            claim.ApprovedByUserId = _userManager.GetUserId(User);
            claim.ApprovedOn = DateTime.UtcNow;

            // Append rejection reason to Notes
            claim.Notes = string.IsNullOrWhiteSpace(claim.Notes)
                ? $"Manager rejected: {reason}"
                : $"{claim.Notes}\nManager rejected: {reason}";

            _db.Claims.Update(claim);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(VerifiedClaims));
        }

        // View approved claims by this manager
        [HttpGet]
        public async Task<IActionResult> ApprovedClaims()
        {
            var userId = _userManager.GetUserId(User);
            var claims = await _db.Claims
                .Include(c => c.Employee)
                .Where(c => c.ApproveStatus == "Approved" &&
                            c.ApprovedByUserId == userId)
                .OrderByDescending(c => c.ApprovedOn)
                .ToListAsync();

            return View(claims);
        }

        //View rejected claims by this manager
        [HttpGet]
        public async Task<IActionResult> RejectedClaims()
        {
            var userId = _userManager.GetUserId(User);
            var claims = await _db.Claims
                .Include(c => c.Employee)
                .Where(c => c.ApproveStatus == "Rejected" &&
                            c.ApprovedByUserId == userId)
                .OrderByDescending(c => c.ApprovedOn)
                .ToListAsync();

            return View(claims);
        }
    }
}