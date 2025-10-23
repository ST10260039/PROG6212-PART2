using System.Reflection.Metadata;
namespace MonthlyClaimSystem.Models
{

    public class Claim
    {
        public int ClaimId { get; set; }
        public string LecturerName { get; set; }
        public double HoursWorked { get; set; }
        public double HourlyRate { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Verified, Approved, Rejected
        public bool IsVerified { get; set; } = false;
        public DateTime DateSubmitted { get; set; }
        public List<Document> Documents { get; set; } = new();
    }
}