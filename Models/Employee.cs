using System.ComponentModel.DataAnnotations;

namespace MonthlyClaimSystem.Models
{
    public class Employee
    {
        public int EmployeeID { get; set; }

        [Required] public string EmployeeName { get; set; } = string.Empty;
        [Required] public string Department { get; set; } = string.Empty;
        [Required] public string ContactInfo { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal ClaimRate { get; set; }
    }
}