using Microsoft.AspNetCore.Mvc;
using MonthlyClaimSystem.Models;
using MonthlyClaimSystem.Services;

namespace MonthlyClaimSystem.Controllers
{
    public class EmployeeController : Controller
    {
        public IActionResult Index()
        {
            var employees = EmployeeService.GetAll();
            return View(employees);
        }

        // … rest of your controller actions …
    }
}