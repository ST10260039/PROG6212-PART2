using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MonthlyClaimSystem.Data;
using MonthlyClaimSystem.Models;
using MonthlyClaimSystem.Services;

namespace MonthlyClaimSystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure EF Core with SQL Server
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Configure Identity with relaxed password rules
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // Add MVC and session support
            builder.Services.AddControllersWithViews();
            builder.Services.AddSession();

            // Register ClaimService
            builder.Services.AddScoped<IClaimService, ClaimService>();

            // Role-based authorization policies
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("HRPolicy", policy => policy.RequireRole("HR"));
                options.AddPolicy("LecturerPolicy", policy => policy.RequireRole("Lecturer"));
                options.AddPolicy("CoordinatorPolicy", policy => policy.RequireRole("Coordinator"));
                options.AddPolicy("ManagerPolicy", policy => policy.RequireRole("Manager"));
            });

            var app = builder.Build();

            // Seed roles and users
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                await DataSeeder.SeedRolesAndUsers(services);
            }

            // Middleware pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseSession();        // Session before auth
            app.UseAuthentication(); // Identity
            app.UseAuthorization();

            // Default route to login
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            app.Run();
        }
    }
}