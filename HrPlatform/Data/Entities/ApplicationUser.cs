using Microsoft.AspNetCore.Identity;

namespace HrPlatform.Data.Models;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    // reference to Company entity (for Manager and Company users)
    public int? CompanyId { get; set; }
    public Company? Company { get; set; }
}