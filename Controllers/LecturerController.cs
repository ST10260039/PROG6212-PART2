using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MonthlyClaimSystem.Models;
using MonthlyClaimSystem.Services;
using AppDocument = MonthlyClaimSystem.Models.Document;

namespace MonthlyClaimSystem.Controllers
{
    public class LecturerController : Controller
    {
        public IActionResult SubmitClaim()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitClaim(Claim claim, List<IFormFile> files)
        {
            claim.ClaimId = new Random().Next(1000, 9999);
            claim.DateSubmitted = DateTime.Now;
            claim.Status = "Pending";
            claim.Documents = new List<AppDocument>();

            // Ensure upload folder exists
            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            foreach (var file in files)
            {
                if (file.Length > 0 && IsValidFileType(file.FileName))
                {
                    if (file.Length > 5 * 1024 * 1024) // 5MB limit
                    {
                        ModelState.AddModelError("", $"File '{file.FileName}' is too large. Max size is 5MB.");
                        return View();
                    }

                    var filePath = Path.Combine(uploadFolder, file.FileName);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await file.CopyToAsync(stream);

                    var encrypted = await EncryptFileAsync(file);

                    claim.Documents.Add(new AppDocument
                    {
                        FileName = file.FileName,
                        FilePath = filePath,
                        FileType = Path.GetExtension(file.FileName),
                        EncryptedData = encrypted
                    });
                }
            }

            ClaimService.SaveClaim(claim);
            TempData["Message"] = $"Claim submitted successfully! Your Claim ID is {claim.ClaimId}";
            return RedirectToAction("SubmitClaim");
        }

        private bool IsValidFileType(string fileName)
        {
            var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx" };
            return allowedExtensions.Contains(Path.GetExtension(fileName).ToLower());
        }

        private async Task<byte[]> EncryptFileAsync(IFormFile file)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            byte[] data = ms.ToArray();
            for (int i = 0; i < data.Length; i++) data[i] ^= 0x5A; // Simple XOR encryption
            return data;
        }

        public IActionResult MyClaims(string lecturerName)
        {
            var claims = ClaimService.GetAll()
                .Where(c => c.LecturerName.Equals(lecturerName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return View(claims);
        }
    }
}