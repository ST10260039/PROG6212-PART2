using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonthlyClaimSystem.Models;
using MonthlyClaimSystem.Services;
using System;
using System.Linq;

namespace MonthlyClaimSystem.Controllers
{
    [Authorize(Roles = "HR")] // Only HR can access
    public class HRController : Controller
    {
        // Filtered claims report
        public IActionResult FilteredReport(string status, string lecturer, decimal? minAmount, decimal? maxAmount, DateTime? fromDate, DateTime? toDate)
        {
            var claims = ClaimService.GetAll().AsEnumerable(); // ✅ IEnumerable

            if (!string.IsNullOrEmpty(status))
                claims = claims.Where(c => c.Status == status);

            if (!string.IsNullOrEmpty(lecturer))
                claims = claims.Where(c => c.LecturerName.Contains(lecturer));

            if (minAmount.HasValue)
                claims = claims.Where(c => c.TotalPayment >= minAmount.Value);

            if (maxAmount.HasValue)
                claims = claims.Where(c => c.TotalPayment <= maxAmount.Value);

            if (fromDate.HasValue)
                claims = claims.Where(c => c.ClaimDate >= fromDate.Value);

            if (toDate.HasValue)
                claims = claims.Where(c => c.ClaimDate <= toDate.Value);

            return View("FilteredReport", claims.ToList()); // ✅ convert once at the end
        }

        // Manage lecturers
        public IActionResult ManageLecturers()
        {
            var lecturers = EmployeeService.GetAll();
            return View("ManageLecturers", lecturers);
        }

        [HttpGet]
        public IActionResult CreateLecturer()
        {
            return View(new Employee());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateLecturer(Employee lecturer)
        {
            if (!ModelState.IsValid) return View(lecturer);

            EmployeeService.Add(lecturer);
            TempData["Message"] = "Lecturer created successfully.";
            return RedirectToAction("ManageLecturers");
        }

        [HttpGet]
        public IActionResult EditLecturer(int id)
        {
            var lecturer = EmployeeService.GetById(id);
            if (lecturer == null) return NotFound();

            return View("EditLecturer", lecturer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditLecturer(Employee updated)
        {
            if (!ModelState.IsValid) return View(updated);

            EmployeeService.Update(updated);
            TempData["Message"] = "Lecturer updated successfully.";
            return RedirectToAction("ManageLecturers");
        }

        // HR sets lecturer claim rate
        [HttpPost]
        public IActionResult SetClaimRate(int id, decimal rate)
        {
            var lecturer = EmployeeService.GetById(id);
            if (lecturer == null) return NotFound();

            lecturer.ClaimRate = rate;
            EmployeeService.Update(lecturer);

            TempData["Message"] = $"Claim rate set to {rate} for {lecturer.EmployeeName}.";
            return RedirectToAction("ManageLecturers");
        }
    }
}