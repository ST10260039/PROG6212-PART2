using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MonthlyClaimSystem.Models;
using System.Collections.Generic;

namespace MonthlyClaimSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Claim> Claims { get; set; }
    }
}