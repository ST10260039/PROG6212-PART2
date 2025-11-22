using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonthlyClaimSystem.Data;
using MonthlyClaimSystem.Models;

namespace MonthlyClaimSystem.Controllers
{
    [Authorize(Policy = "CoordinatorPolicy")]
    public class CoordinatorController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public CoordinatorController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // Show all pending claims for coordinator verification
        [HttpGet]
        public async Task<IActionResult> PendingClaims()
        {
            var claims = await _db.Claims
                .Include(c => c.Documents)
                .Include(c => c.Employee)
                .Where(c => c.VerifyStatus == "Pending")
                .OrderBy(c => c.DateSubmitted)
                .ToListAsync();

            return View(claims);
        }

        // Verify claim (mark as Verified if valid, else Rejected)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyClaim(int id)
        {
            var claim = await _db.Claims.FindAsync(id);
            if (claim == null) return NotFound();

            bool valid = claim.HoursWorked > 0;

            if (valid)
            {
                claim.VerifyStatus = "Verified";
                claim.ApproveStatus = "Pending";   // Lecturer sees “Verified (Awaiting Approval)”
            }
            else
            {
                claim.VerifyStatus = "Rejected";
                claim.ApproveStatus = "Rejected";  // Lecturer sees “Rejected”
            }
            claim.VerifiedByUserId = _userManager.GetUserId(User);
            claim.VerifiedOn = DateTime.UtcNow;

            _db.Claims.Update(claim);
            await _db.SaveChangesAsync();

            // Redirect to VerifiedClaims so coordinator can immediately see verified claims
            return RedirectToAction(nameof(VerifiedClaims));
        }

        // Explicit reject action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectClaim(int id, string? reason)
        {
            var claim = await _db.Claims.FindAsync(id);
            if (claim == null) return NotFound();

            claim.VerifyStatus = "Rejected";
            claim.ApproveStatus = "Rejected";   // keep statuses aligned
            claim.VerifiedByUserId = _userManager.GetUserId(User);
            claim.VerifiedOn = DateTime.UtcNow;

            // Append rejection reason to Notes
            claim.Notes = string.IsNullOrWhiteSpace(claim.Notes)
                ? $"Coordinator rejected: {reason}"
                : $"{claim.Notes}\nCoordinator rejected: {reason}";

            _db.Claims.Update(claim);
            await _db.SaveChangesAsync();

            // Redirect to RejectedClaims so coordinator can immediately see rejected claims
            return RedirectToAction(nameof(RejectedClaims));
        }

        // View verified claims by this coordinator
        [HttpGet]
        public async Task<IActionResult> VerifiedClaims()
        {
            var userId = _userManager.GetUserId(User);

            var claims = await _db.Claims
                .Include(c => c.Employee)
                .Where(c => c.VerifyStatus == "Verified" &&
                            c.VerifiedByUserId == userId)
                .OrderByDescending(c => c.VerifiedOn)
                .ToListAsync();

            return View(claims);
        }

        // View rejected claims
        [HttpGet]
        public async Task<IActionResult> RejectedClaims()
        {
            var userId = _userManager.GetUserId(User);

            var claims = await _db.Claims
                .Include(c => c.Employee)
                // Option A: only show claims rejected by this coordinator
                .Where(c => c.VerifyStatus == "Rejected" &&
                            c.VerifiedByUserId == userId)
                // Option B: show all rejected claims (remove user filter)
                //.Where(c => c.VerifyStatus == "Rejected")
                .OrderByDescending(c => c.VerifiedOn)
                .ToListAsync();

            return View(claims);
        }
    }
}