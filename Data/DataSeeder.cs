using Microsoft.AspNetCore.Identity;
using MonthlyClaimSystem.Models;

namespace MonthlyClaimSystem.Data
{
    public static class DataSeeder
    {
        public static async Task SeedRolesAndUsers(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles = { "Lecturer", "Coordinator", "Manager", "HR" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var roleResult = await roleManager.CreateAsync(new IdentityRole(role));
                    if (roleResult.Succeeded)
                        Console.WriteLine($"Role '{role}' created.");
                    else
                        foreach (var error in roleResult.Errors)
                            Console.WriteLine($"Error creating role '{role}': {error.Description}");
                }
            }

            // Seed HR (must exist)
            await EnsureUser(userManager, "hr@cms.com", "HR", "hr@cms.com");

            // Demo users (optional)
            await EnsureUser(userManager, "lecturer@cms.com", "Lecturer", "lecturer@cms.com");
            await EnsureUser(userManager, "coordinator@cms.com", "Coordinator", "coordinator@cms.com");
            await EnsureUser(userManager, "manager@cms.com", "Manager", "manager@cms.com");
        }

        private static async Task EnsureUser(UserManager<ApplicationUser> userManager, string email, string role, string password)
        {
            var existing = await userManager.FindByEmailAsync(email);
            if (existing != null)
            {
                if (!await userManager.IsInRoleAsync(existing, role))
                {
                    var roleResult = await userManager.AddToRoleAsync(existing, role);
                    if (roleResult.Succeeded)
                        Console.WriteLine($"User '{email}' added to role '{role}'.");
                    else
                        foreach (var error in roleResult.Errors)
                            Console.WriteLine($"Error assigning role '{role}' to '{email}': {error.Description}");
                }
                return;
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (createResult.Succeeded)
            {
                var roleResult = await userManager.AddToRoleAsync(user, role);
                if (roleResult.Succeeded)
                    Console.WriteLine($"User '{email}' created and assigned to role '{role}'.");
                else
                    foreach (var error in roleResult.Errors)
                        Console.WriteLine($"Error assigning role '{role}' to '{email}': {error.Description}");
            }
            else
            {
                foreach (var error in createResult.Errors)
                    Console.WriteLine($"Error creating user '{email}': {error.Description}");
            }
        }
    }
}