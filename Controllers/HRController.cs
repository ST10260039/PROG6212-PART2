using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonthlyClaimSystem.Data;
using MonthlyClaimSystem.Models;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonthlyClaimSystem.Controllers
{
    [Authorize(Policy = "HRPolicy")]
    public class HRController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HRController(ApplicationDbContext db) => _db = db;

        // Lecturer management
        [HttpGet]
        public async Task<IActionResult> ManageLecturers()
        {
            var lecturers = await _db.Employees.OrderBy(e => e.EmployeeName).ToListAsync();
            return View(lecturers);
        }

        [HttpGet]
        public IActionResult CreateLecturer() => View(new Employee());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLecturer(Employee lecturer)
        {
            if (!ModelState.IsValid) return View(lecturer);
            _db.Employees.Add(lecturer);
            await _db.SaveChangesAsync();
            TempData["Message"] = "Lecturer created successfully.";
            return RedirectToAction("ManageLecturers");
        }

        [HttpGet]
        public async Task<IActionResult> EditLecturer(int id)
        {
            var lecturer = await _db.Employees.FindAsync(id);
            if (lecturer == null) return NotFound();
            return View(lecturer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLecturer(Employee updated)
        {
            if (!ModelState.IsValid) return View(updated);
            _db.Employees.Update(updated);
            await _db.SaveChangesAsync();
            TempData["Message"] = "Lecturer updated successfully.";
            return RedirectToAction("ManageLecturers");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetClaimRate(int id, decimal rate)
        {
            var lecturer = await _db.Employees.FindAsync(id);
            if (lecturer == null) return NotFound();

            lecturer.ClaimRate = rate;
            _db.Employees.Update(lecturer);
            await _db.SaveChangesAsync();

            TempData["Message"] = $"Claim rate set to {rate} for {lecturer.EmployeeName}.";
            return RedirectToAction("ManageLecturers");
        }

        //HR Claims Dashboard with filters
        [HttpGet]
        public async Task<IActionResult> Claims(
            string? verifyStatus,
            string? approveStatus,
            string? lecturer,
            decimal? minAmount,
            decimal? maxAmount,
            decimal? minRate,
            decimal? maxRate,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = _db.Claims.Include(c => c.Employee).AsQueryable();

            if (!string.IsNullOrEmpty(verifyStatus))
                query = query.Where(c => c.VerifyStatus == verifyStatus);

            if (!string.IsNullOrEmpty(approveStatus))
                query = query.Where(c => c.ApproveStatus == approveStatus);

            if (!string.IsNullOrEmpty(lecturer))
                query = query.Where(c => c.Employee.EmployeeName.Contains(lecturer));

            if (minAmount.HasValue) query = query.Where(c => c.TotalPayment >= minAmount.Value);
            if (maxAmount.HasValue) query = query.Where(c => c.TotalPayment <= maxAmount.Value);
            if (minRate.HasValue) query = query.Where(c => c.HourlyRate >= minRate.Value);
            if (maxRate.HasValue) query = query.Where(c => c.HourlyRate <= maxRate.Value);
            if (fromDate.HasValue) query = query.Where(c => c.ClaimDate >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(c => c.ClaimDate <= toDate.Value);

            var claims = await query.OrderByDescending(c => c.DateSubmitted).ToListAsync();
            return View(claims); // Views/HR/Claims.cshtml
        }

        //Export CSV
        [HttpGet]
        public async Task<IActionResult> ExportCsv(string? verifyStatus, string? approveStatus, string? lecturer)
        {
            var query = _db.Claims.AsQueryable();

            if (!string.IsNullOrEmpty(verifyStatus))
                query = query.Where(c => c.VerifyStatus == verifyStatus);

            if (!string.IsNullOrEmpty(approveStatus))
                query = query.Where(c => c.ApproveStatus == approveStatus);

            if (!string.IsNullOrEmpty(lecturer))
                query = query.Where(c => c.LecturerName.Contains(lecturer));

            var rows = await query.OrderByDescending(c => c.DateSubmitted).ToListAsync();
            var sb = new StringBuilder();
            sb.AppendLine("ClaimId,LecturerName,HoursWorked,HourlyRate,TotalPayment,VerifyStatus,ApproveStatus,DateSubmitted");
            foreach (var c in rows)
                sb.AppendLine($"{c.ClaimId},{c.LecturerName},{c.HoursWorked},{c.HourlyRate},{c.TotalPayment},{c.VerifyStatus},{c.ApproveStatus},{c.DateSubmitted:yyyy-MM-dd}");

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "claims.csv");
        }

        // Export Excel (tab-delimited)
        [HttpGet]
        public async Task<IActionResult> ExportExcel(string? verifyStatus, string? approveStatus, string? lecturer)
        {
            var query = _db.Claims.AsQueryable();

            if (!string.IsNullOrEmpty(verifyStatus))
                query = query.Where(c => c.VerifyStatus == verifyStatus);

            if (!string.IsNullOrEmpty(approveStatus))
                query = query.Where(c => c.ApproveStatus == approveStatus);

            if (!string.IsNullOrEmpty(lecturer))
                query = query.Where(c => c.LecturerName.Contains(lecturer));

            var rows = await query.OrderByDescending(c => c.DateSubmitted).ToListAsync();
            var sb = new StringBuilder();
            sb.AppendLine("ClaimId\tLecturerName\tHoursWorked\tHourlyRate\tTotalPayment\tVerifyStatus\tApproveStatus\tDateSubmitted");
            foreach (var c in rows)
                sb.AppendLine($"{c.ClaimId}\t{c.LecturerName}\t{c.HoursWorked}\t{c.HourlyRate}\t{c.TotalPayment}\t{c.VerifyStatus}\t{c.ApproveStatus}\t{c.DateSubmitted:yyyy-MM-dd}");

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "application/vnd.ms-excel", "claims.xls");
        }
    }
}