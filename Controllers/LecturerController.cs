using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using MonthlyClaimSystem.Models;
using MonthlyClaimSystem.Services;

namespace MonthlyClaimSystem.Controllers
{
    public class LecturerController : Controller
    {
        public IActionResult SubmitClaim()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SubmitClaim(Claim claim, List<IFormFile> files)
        {
            claim.ClaimId = new Random().Next(1000, 9999);
            claim.DateSubmitted = DateTime.Now;
            claim.Status = "Pending";
            claim.Documents = new List<Document>();

            foreach (var file in files)
            {
                if (file.Length > 0 && IsValidFileType(file.FileName))
                {
                    var path = Path.Combine("wwwroot/uploads", file.FileName);
                    using var stream = new FileStream(path, FileMode.Create);
                    file.CopyTo(stream);

                    claim.Documents.Add(new Document
                    {
                        FileName = file.FileName,
                        FilePath = path,
                        FileType = Path.GetExtension(file.FileName),
                        EncryptedData = EncryptFile(path)
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

        private byte[] EncryptFile(string path)
        {
            byte[] data = System.IO.File.ReadAllBytes(path);
            for (int i = 0; i < data.Length; i++) data[i] ^= 0x5A; // Simple XOR encryption
            return data;
        }
    }
}