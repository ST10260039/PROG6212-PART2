using System;
using System.Collections.Generic;

namespace MonthlyClaimSystem.Models
{
    public class Claim
    {
        public int ClaimId { get; set; }

        // Lecturer details
        public string LecturerName { get; set; }
        public int EmployeeID { get; set; } // Foreign key to Employee

        // Claim details
        public decimal HoursWorked { get; set; }
        public decimal HourlyRate { get; set; } // Must match HR’s preset ClaimRate
        public decimal TotalPayment { get; set; } // Auto-calculated: HoursWorked * HourlyRate
        public string Notes { get; set; }

        // Workflow statuses
        public string Status { get; set; } = "Pending"; // Pending, Verified, Approved, Rejected
        public string VerifyStatus { get; set; } = "Pending"; // Verified/Rejected
        public string ApproveStatus { get; set; } = "Pending"; // Approved/Rejected
        public bool IsVerified { get; set; } = false;

        // Metadata
        public DateTime ClaimDate { get; set; } = DateTime.Now;
        public DateTime DateSubmitted { get; set; } = DateTime.Now;

        // Supporting documents
        public List<Document> Documents { get; set; } = new();
    }
}