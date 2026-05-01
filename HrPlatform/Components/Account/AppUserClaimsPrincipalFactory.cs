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
        var userWithCompany = await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == user.Id)
            .Select(u => new
            {
                u.CompanyId,
                CompanyName = u.Company != null ? u.Company.Name : string.Empty
            })
            .SingleOrDefaultAsync();

        if (userWithCompany?.CompanyId != null)
        {
            identity.AddClaim(new Claim("companyId", userWithCompany.CompanyId.Value.ToString()));
            identity.AddClaim(new Claim("companyName", userWithCompany.CompanyName ?? string.Empty));
        }

        return identity;
    }
}