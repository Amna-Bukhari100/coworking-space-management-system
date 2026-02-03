using CoWorkManager.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace CoWorkManager.Data
{
    public static class DataSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

     
            string[] roles = { "Admin", "User" };
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                    await roleManager.CreateAsync(new IdentityRole(roleName));
            }

            var adminRole = await roleManager.FindByNameAsync("Admin");
            if (adminRole != null)
            {
                var adminRoleClaims = await roleManager.GetClaimsAsync(adminRole);
                if (!adminRoleClaims.Any(c => c.Type == "Permission" && c.Value == "ManageBookings"))
                    await roleManager.AddClaimAsync(adminRole, new Claim("Permission", "ManageBookings"));

                if (!adminRoleClaims.Any(c => c.Type == "Permission" && c.Value == "ViewVisitors"))
                    await roleManager.AddClaimAsync(adminRole, new Claim("Permission", "ViewVisitors"));
            }

            // User role claims
            var userRole = await roleManager.FindByNameAsync("User");
            if (userRole != null)
            {
                var userRoleClaims = await roleManager.GetClaimsAsync(userRole);
                if (!userRoleClaims.Any(c => c.Type == "Permission" && c.Value == "ManageBookings"))
                    await roleManager.AddClaimAsync(userRole, new Claim("Permission", "ManageBookings"));

                if (!userRoleClaims.Any(c => c.Type == "Permission" && c.Value == "ViewVisitors"))
                    await roleManager.AddClaimAsync(userRole, new Claim("Permission", "ViewVisitors"));
            }

        
      
            var adminEmail = "admin@cowork.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Default Admin",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");

                    var existingClaims = await userManager.GetClaimsAsync(adminUser);
                    if (!existingClaims.Any(c => c.Type == "MembershipLevel"))
                        await userManager.AddClaimAsync(adminUser, new Claim("MembershipLevel", "5"));

                    if (!existingClaims.Any(c => c.Type == "FullName"))
                        await userManager.AddClaimAsync(adminUser, new Claim("FullName", adminUser.FullName ?? ""));
                }
            }

            var userEmail = "user@cowork.com";
            var normalUser = await userManager.FindByEmailAsync(userEmail);
            if (normalUser == null)
            {
                normalUser = new ApplicationUser
                {
                    UserName = userEmail,
                    Email = userEmail,
                    FullName = "Default User",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(normalUser, "User@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(normalUser, "User");

                    var existingClaims = await userManager.GetClaimsAsync(normalUser);
                    if (!existingClaims.Any(c => c.Type == "MembershipLevel"))
                        await userManager.AddClaimAsync(normalUser, new Claim("MembershipLevel", "1"));

                    if (!existingClaims.Any(c => c.Type == "FullName"))
                        await userManager.AddClaimAsync(normalUser, new Claim("FullName", normalUser.FullName ?? ""));
                }
            }
        }
    }
}
