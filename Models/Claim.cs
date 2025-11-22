using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MonthlyClaimSystem.Models.MonthlyClaimSystem.Models;

namespace MonthlyClaimSystem.Models
{
    public class Claim
    {
        [Key]
        public int ClaimId { get; set; }

        [Required]
        public string LecturerName { get; set; } = string.Empty;

        [ForeignKey(nameof(Employee))]
        public int EmployeeID { get; set; }
        public Employee? Employee { get; set; }

        [Range(0, double.MaxValue)]
        public decimal HoursWorked { get; set; }

        [Range(0, double.MaxValue)]
        public decimal HourlyRate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalPayment { get; set; }

        public string? Notes { get; set; }

        // Status values stored as strings
        [Required]
        public string VerifyStatus { get; set; } = ClaimVerifyStatus.Pending;

        [Required]
        public string ApproveStatus { get; set; } = ClaimApproveStatus.Pending;

        //Derived property: successful only if verified + approved
        [NotMapped]
        public bool IsSuccessful =>
            VerifyStatus == ClaimVerifyStatus.Verified &&
            ApproveStatus == ClaimApproveStatus.Approved;

        public DateTime ClaimDate { get; set; } = DateTime.Now;
        public DateTime DateSubmitted { get; set; } = DateTime.Now;

        //  Audit fields
        public string? VerifiedByUserId { get; set; }
        public DateTime? VerifiedOn { get; set; }
        public string? ApprovedByUserId { get; set; }
        public DateTime? ApprovedOn { get; set; }

        public List<Document> Documents { get; set; } = new();
    }
}