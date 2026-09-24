using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Woodlands_Prototype_Insy7315.Models;

namespace Woodlands_Prototype_Insy7315.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            foreach (var role in IdentitySeederRoles.All)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole(role));
                    EnsureSuccess(result, $"create role {role}");
                }
            }

            await CreatePrototypeUser(userManager, "Admin User", "admin@woodlandsdb.co.za", "admin123", IdentitySeederRoles.Admin, null);
            await CreatePrototypeUser(userManager, "Soweto Manager", "soweto@woodlandsdb.co.za", "manager123", IdentitySeederRoles.SowetoManager, "Soweto");
            await CreatePrototypeUser(userManager, "Roodepoort Manager", "roodepoort@woodlandsdb.co.za", "manager123", IdentitySeederRoles.RoodepoortManager, "Roodepoort");
            await CreatePrototypeUser(userManager, "Randfontein Manager", "randfontein@woodlandsdb.co.za", "manager123", IdentitySeederRoles.RandfonteinManager, "Randfontein");
            await CreatePrototypeUser(userManager, "Prototype Customer", "customer@example.com", "customer123", IdentitySeederRoles.Customer, null);
        }

        private static async Task CreatePrototypeUser(
            UserManager<ApplicationUser> userManager,
            string fullName,
            string email,
            string password,
            string role,
            string? branch)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = fullName,
                    Branch = branch
                };

                var createResult = await userManager.CreateAsync(user, password);
                EnsureSuccess(createResult, $"create user {email}");
            }
            else
            {
                user.FullName = fullName;
                user.Branch = branch;
                user.EmailConfirmed = true;
                await userManager.UpdateAsync(user);
            }

            if (!await userManager.IsInRoleAsync(user, role))
            {
                var roleResult = await userManager.AddToRoleAsync(user, role);
                EnsureSuccess(roleResult, $"assign role {role} to {email}");
            }
        }

        private static void EnsureSuccess(IdentityResult result, string action)
        {
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not {action}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }
}
