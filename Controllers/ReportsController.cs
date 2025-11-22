using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MonthlyClaimSystem.Data;
using MonthlyClaimSystem.Models;
using MonthlyClaimSystem.Models.MonthlyClaimSystem.Models;
using MonthlyClaimSystem.ViewModels;

[Authorize(Policy = "RequireHR")]
public class ReportsController : Controller
{
    private readonly ApplicationDbContext _db;

    public ReportsController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Claims([FromQuery] ClaimReportFilter filter)
    {
        var query = _db.Claims.Include(c => c.Employee).AsQueryable();

        if (!string.IsNullOrEmpty(filter.VerifyStatus))
            query = query.Where(c => c.VerifyStatus == filter.VerifyStatus);
        if (!string.IsNullOrEmpty(filter.ApproveStatus))
            query = query.Where(c => c.ApproveStatus == filter.ApproveStatus);
        if (filter.LecturerEmployeeId.HasValue)
            query = query.Where(c => c.EmployeeID == filter.LecturerEmployeeId.Value);
        if (filter.MinAmount.HasValue)
            query = query.Where(c => c.TotalPayment >= filter.MinAmount.Value);
        if (filter.MaxAmount.HasValue)
            query = query.Where(c => c.TotalPayment <= filter.MaxAmount.Value);
        if (filter.MinRate.HasValue)
            query = query.Where(c => c.HourlyRate >= filter.MinRate.Value);
        if (filter.MaxRate.HasValue)
            query = query.Where(c => c.HourlyRate <= filter.MaxRate.Value);
        if (filter.FromDate.HasValue)
            query = query.Where(c => c.ClaimDate >= filter.FromDate.Value);
        if (filter.ToDate.HasValue)
            query = query.Where(c => c.ClaimDate <= filter.ToDate.Value);

        var results = await query
            .OrderByDescending(c => c.ClaimDate)
            .Select(c => new
            {
                c.ClaimId,
                Lecturer = c.Employee.EmployeeName,
                c.EmployeeID,
                c.HoursWorked,
                c.HourlyRate,
                c.TotalPayment,
                c.VerifyStatus,
                c.ApproveStatus,
                c.ClaimDate,
                c.DateSubmitted,
                c.VerifiedOn,
                c.ApprovedOn,
                c.Notes
            })
            .ToListAsync();

        if (string.Equals(filter.ExportFormat, "Excel", StringComparison.OrdinalIgnoreCase))
        {
            var bytes = ExportExcel(results);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"ClaimsReport_{DateTime.Now:yyyyMMddHHmm}.xlsx");
        }

        if (string.Equals(filter.ExportFormat, "PDF", StringComparison.OrdinalIgnoreCase))
        {
            var bytes = ExportPdf(results);
            return File(bytes, "application/pdf", $"ClaimsReport_{DateTime.Now:yyyyMMddHHmm}.pdf");
        }

        return View(results);
    }

    private static byte[] ExportExcel(System.Collections.IEnumerable rows)
    {
        using var ms = new MemoryStream();
        using var sw = new StreamWriter(ms);
        sw.WriteLine("Use ClosedXML for real Excel export.");
        sw.Flush();
        return ms.ToArray();
    }

    private static byte[] ExportPdf(System.Collections.IEnumerable rows)
    {
        using var ms = new MemoryStream();
        using var sw = new StreamWriter(ms);
        sw.WriteLine("Use QuestPDF/iText for real PDF export.");
        sw.Flush();
        return ms.ToArray();
    }

    [Authorize(Policy = "RequireHR")]
    public async Task<IActionResult> Dashboard()
    {
        var pending = await _db.Claims.CountAsync(c => c.VerifyStatus == ClaimVerifyStatus.Pending
                                                    || c.ApproveStatus == ClaimApproveStatus.Pending);
        var verified = await _db.Claims.CountAsync(c => c.VerifyStatus == ClaimVerifyStatus.Verified);
        var approved = await _db.Claims.CountAsync(c => c.ApproveStatus == ClaimApproveStatus.Approved);
        var rejected = await _db.Claims.CountAsync(c => c.VerifyStatus == ClaimVerifyStatus.Rejected
                                                     || c.ApproveStatus == ClaimApproveStatus.Rejected);

        var model = new
        {
            PendingCount = pending,
            VerifiedCount = verified,
            ApprovedCount = approved,
            RejectedCount = rejected
        };

        return View(model);
    }
}