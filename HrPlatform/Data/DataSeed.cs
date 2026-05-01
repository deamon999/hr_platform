using HrPlatform.Data.Models;
using HrPlatform.Services;
using Microsoft.AspNetCore.Identity;

namespace HrPlatform.Data;

public static class DataSeed
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // 1. Create required roles
        string[] roles = { RoleConstants.Admin, RoleConstants.Manager, RoleConstants.Driver };

        foreach (var role in roles)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        // 2. Create admin user
        const string adminEmail = "admin@example.com";
        const string adminPassword = "Admin123!";

        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                PhoneNumber = "3149707320",
                FirstName = "Admin",
                LastName = "Admin",
            };

            var result = await userManager.CreateAsync(admin, adminPassword);

            if (result.Succeeded) await userManager.AddToRoleAsync(admin, RoleConstants.Admin);
        }

        // 2. Create driver user
        const string driverEmail = "driver@example.com";
        const string driverPassword = "Driver123!";

        var driver = await userManager.FindByEmailAsync(driverEmail);

        if (driver == null)
        {
            driver = new ApplicationUser
            {
                UserName = driverEmail,
                Email = driverEmail,
                EmailConfirmed = true,
                PhoneNumber = "3149707320",
                FirstName = "Driver",
                LastName = "Driver",
            };

            var result = await userManager.CreateAsync(driver, driverPassword);

            if (result.Succeeded) await userManager.AddToRoleAsync(driver, RoleConstants.Driver);
        }

        // 3. Create a sample company and manager user assigned to it
        var companySvc = services.GetRequiredService<ICompanyService>();
        var defaultCompany = new Company
        {
            Name = "LVT",
            RegistrationNumber = "LVT-0001",
            DateRegistered = DateOnly.FromDateTime(DateTime.Today),
            City = "Anytown",
            State = "CA",
            Country = "USA",
            ContactEmail = "info@lvt.example.com"
        };

        // try to avoid duplicates when seeding repeatedly
        var allCompanies = await companySvc.GetAllAsync();
        var existing = allCompanies.FirstOrDefault(c => c.Name == defaultCompany.Name);
        if (existing is null)
            defaultCompany = await companySvc.CreateAsync(defaultCompany);
        else
            defaultCompany = existing;

        const string managerEmail = "manager@example.com";
        const string managerPassword = "Manager123!";

        var manager = await userManager.FindByEmailAsync(managerEmail);

        if (manager == null)
        {
            manager = new ApplicationUser
            {
                UserName = managerEmail,
                Email = managerEmail,
                EmailConfirmed = true,
                CompanyId = defaultCompany.Id,
                PhoneNumber = "3149707320",
                FirstName = "Manager",
                LastName = "Manager",
            };

            var result = await userManager.CreateAsync(manager, managerPassword);

            if (result.Succeeded) await userManager.AddToRoleAsync(manager, RoleConstants.Manager);
        }
    }
}