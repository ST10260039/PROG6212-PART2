using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MonthlyClaimSystem.Models;

namespace MonthlyClaimSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Claim> Claims { get; set; }
        public DbSet<Document> Documents { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Relationship configuration
            builder.Entity<Claim>()
                .HasOne(c => c.Employee)
                .WithMany()
                .HasForeignKey(c => c.EmployeeID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Document>()
                .HasOne(d => d.Claim)
                .WithMany(c => c.Documents)
                .HasForeignKey(d => d.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);

            // Decimal precision configuration
            builder.Entity<Claim>(entity =>
            {
                entity.Property(c => c.HourlyRate).HasPrecision(18, 4);
                entity.Property(c => c.HoursWorked).HasPrecision(18, 2);
                entity.Property(c => c.TotalPayment).HasPrecision(18, 4);
            });

            builder.Entity<Employee>(entity =>
            {
                entity.Property(e => e.ClaimRate).HasPrecision(18, 4);
            });
        }
    }
}