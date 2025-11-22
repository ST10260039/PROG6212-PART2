namespace MonthlyClaimSystem.Models
{
    public class Employee
    {
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string Department { get; set; }
        public string ContactInfo { get; set; }
        public decimal ClaimRate { get; set; } // HR sets this
    }
}