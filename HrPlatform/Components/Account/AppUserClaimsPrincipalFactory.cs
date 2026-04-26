using System.Security.Claims;
using HrPlatform.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace HrPlatform.Components.Account;

public class AppUserClaimsPrincipalFactory
    : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    public AppUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        if (user.CompanyId.HasValue)
        {
            identity.AddClaim(new Claim("companyId", user.CompanyId.Value.ToString()));
            identity.AddClaim(new Claim("companyName", user?.Company?.Name ?? ""));
        }

        return identity;
    }
}