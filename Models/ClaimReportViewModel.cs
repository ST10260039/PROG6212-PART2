namespace MonthlyClaimSystem.Models
{
    public class ClaimReportViewModel
    {
        public int ClaimId { get; set; }
        public string LecturerName { get; set; }
        public decimal TotalPayment { get; set; }
        public DateTime ClaimDate { get; set; }
    }
}
