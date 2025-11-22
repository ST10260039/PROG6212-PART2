using MonthlyClaimSystem.Models;
using System.Collections.Generic;
using System.Linq;

namespace MonthlyClaimSystem.Services
{
    public static class EmployeeService
    {
        private static List<Employee> employees = new();

        public static List<Employee> GetAll() => employees;

        public static Employee? GetById(int id) =>
            employees.FirstOrDefault(e => e.EmployeeID == id);

        public static void Add(Employee emp)
        {
            if (emp.EmployeeID == 0)
            {
                emp.EmployeeID = employees.Count > 0
                    ? employees.Max(e => e.EmployeeID) + 1
                    : 1;
            }
            employees.Add(emp);
        }

        public static void Update(Employee emp)
        {
            var existing = GetById(emp.EmployeeID);
            if (existing != null)
            {
                existing.EmployeeName = emp.EmployeeName;
                existing.Department = emp.Department;
                existing.ContactInfo = emp.ContactInfo;
                existing.ClaimRate = emp.ClaimRate;
            }
        }

        public static void Delete(int id)
        {
            var emp = GetById(id);
            if (emp != null)
            {
                employees.Remove(emp);
            }
        }
    }
}