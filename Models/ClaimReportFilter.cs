using System;
using MonthlyClaimSystem.Models;

namespace MonthlyClaimSystem.ViewModels
{
    public class ClaimReportFilter
    {
        public string? VerifyStatus { get; set; }   // was ClaimVerifyStatus?
        public string? ApproveStatus { get; set; }
        public int? LecturerEmployeeId { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public decimal? MinRate { get; set; }
        public decimal? MaxRate { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public string ExportFormat { get; set; } // "Excel" or "PDF"
    }
}