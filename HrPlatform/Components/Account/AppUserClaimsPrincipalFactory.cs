using System.Security.Claims;
using HrPlatform.Data;
using HrPlatform.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HrPlatform.Components.Account;

public class AppUserClaimsPrincipalFactory
    : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    private readonly ApplicationDbContext _db;

    public AppUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor,
        ApplicationDbContext db)
        : base(userManager, roleManager, optionsAccessor)
    {
        _db = db;
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        // Project only the fields we need to keep the query small
        var userData = await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == user.Id)
            .Select(u => new
            {
                u.CompanyId,
                CompanyName = u.Company != null ? u.Company.Name : string.Empty,
                // Cast to int? to safely handle users who don't have a profile yet
                DriverProfileId = u.DriverProfile != null ? (int?)u.DriverProfile.Id : null
            })
            .SingleOrDefaultAsync();

        if (userData != null)
        {
            // Add Company claims if applicable (for Managers/Recruiters)
            if (userData.CompanyId.HasValue)
            {
                identity.AddClaim(new Claim("companyId", userData.CompanyId.Value.ToString()));
                identity.AddClaim(new Claim("companyName", userData.CompanyName ?? string.Empty));
            }

            // Add Driver Profile claim if applicable (for Drivers)
            if (userData.DriverProfileId.HasValue)
            {
                identity.AddClaim(new Claim("driverProfileId", userData.DriverProfileId.Value.ToString()));
            }
        }

        return identity;
    }
}