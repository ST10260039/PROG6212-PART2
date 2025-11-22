using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MonthlyClaimSystem.Data;
using MonthlyClaimSystem.Models;

namespace MonthlyClaimSystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ? Step 4: Configure EF Core with SQL Server
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // ? Step 4: Configure Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // ? Other services
            builder.Services.AddControllersWithViews();
            builder.Services.AddSession();
            builder.Services.AddAuthentication();
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("HRPolicy", policy => policy.RequireRole("HR"));
            });

            var app = builder.Build();

            // ? Step 6: Seed roles and HR user
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                await DataSeeder.SeedRolesAndHRUser(services);
            }

            // ? Middleware pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}