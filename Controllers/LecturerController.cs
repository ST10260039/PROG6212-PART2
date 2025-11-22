using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonthlyClaimSystem.Data;
using MonthlyClaimSystem.Models;
using MonthlyClaimSystem.Models.MonthlyClaimSystem.Models;

namespace MonthlyClaimSystem.Controllers
{
    [Authorize(Policy = "LecturerPolicy")]
    public class LecturerController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public LecturerController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        [HttpGet]
        public IActionResult SubmitClaim() => View(new Claim());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitClaim(Claim claim, List<IFormFile> files)
        {
            var employee = await _db.Employees.FirstOrDefaultAsync(e => e.EmployeeID == claim.EmployeeID);
            if (employee == null)
            {
                ModelState.AddModelError("", "Select a valid lecturer (Employee).");
                return View(claim);
            }

            // Populate claim details
            claim.LecturerName = employee.EmployeeName;
            claim.HourlyRate = employee.ClaimRate;
            claim.TotalPayment = claim.HoursWorked * claim.HourlyRate;

            // Use string constants instead of enums
            claim.VerifyStatus = ClaimVerifyStatus.Pending;
            claim.ApproveStatus = ClaimApproveStatus.Pending;
            claim.DateSubmitted = DateTime.UtcNow;

            _db.Claims.Add(claim);
            await _db.SaveChangesAsync();

            //Handle file uploads
            var uploadFolder = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadFolder);

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var ext = Path.GetExtension(file.FileName).ToLower();
                    var allowed = new[] { ".pdf", ".docx", ".xlsx", ".jpg", ".png" };
                    if (!allowed.Contains(ext) || file.Length > 5 * 1024 * 1024) continue;

                    var uniqueName = $"{claim.ClaimId}_{Path.GetFileName(file.FileName)}";
                    var filePath = Path.Combine(uploadFolder, uniqueName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    var data = ms.ToArray();

                    // Simple XOR "encryption"
                    for (int i = 0; i < data.Length; i++) data[i] ^= 0x5A;

                    _db.Documents.Add(new Document
                    {
                        ClaimId = claim.ClaimId,
                        FileName = file.FileName,
                        FileType = ext,
                        FileSize = file.Length,
                        UploadedOn = DateTime.UtcNow,
                        EncryptedData = data
                    });
                }
            }

            await _db.SaveChangesAsync();
            TempData["Message"] = $"Claim submitted successfully! Claim ID: {claim.ClaimId}";
            return RedirectToAction("MyClaims", new { lecturerName = employee.EmployeeName });
        }

        [HttpGet]
        public async Task<IActionResult> MyClaims(string lecturerName)
        {
            var claims = await _db.Claims
                .Include(c => c.Documents)
                .Where(c => c.LecturerName == lecturerName)
                .OrderByDescending(c => c.DateSubmitted)
                .ToListAsync();

            return View(claims);
        }
    }
}