using Microsoft.AspNetCore.Identity;

namespace RailwayMenagmentSystem.Data;

public static class RoleSeeder
{
    public static async Task SeedRolesAndUsersAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

        string[] roleNames = { "Admin", "Employee", "User" };

        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        var adminEmail = "admin@railway.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            var newAdmin = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(newAdmin, "AdminPass123!");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(newAdmin, "Admin");
            }
        }

        var employeeEmail = "employee@railway.com";
        var employeeUser = await userManager.FindByEmailAsync(employeeEmail);

        if (employeeUser == null)
        {
            var newEmployee = new IdentityUser
            {
                UserName = employeeEmail,
                Email = employeeEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(newEmployee, "EmployeePass123!");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(newEmployee, "Employee");
            }
        }

        var userEmail = "user@railway.com";
        var user = await userManager.FindByEmailAsync(userEmail);

        if (user == null)
        {
            var newUser = new IdentityUser
            {
                UserName = userEmail,
                Email = userEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(newUser, "UserPass123!");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(newUser, "User");
            }
        }
    }
}