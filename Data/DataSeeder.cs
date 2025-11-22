using Microsoft.AspNetCore.Identity;
using MonthlyClaimSystem.Models;

namespace MonthlyClaimSystem.Data
{
    public static class DataSeeder
    {
        public static async Task SeedRolesAndHRUser(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles = { "Lecturer", "Coordinator", "Manager", "HR" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            var hrUser = new ApplicationUser { UserName = "hr@cms.com", Email = "hr@cms.com", EmailConfirmed = true };
            var user = await userManager.FindByEmailAsync(hrUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(hrUser, "SecurePassword123!");
                await userManager.AddToRoleAsync(hrUser, "HR");
            }
        }
    }
}