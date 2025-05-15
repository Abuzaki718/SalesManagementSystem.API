using Microsoft.AspNetCore.Identity;
using SalesManagementSystem.Shared.Constants;

namespace SalesManagementSystem.API.Serivces
{
    public static class DataSeeder
    {
        public static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync(RolesNames.User))
            {
                await roleManager.CreateAsync(new IdentityRole(RolesNames.User));
            }

        }
    }
}
