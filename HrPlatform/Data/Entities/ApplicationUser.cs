using Microsoft.AspNetCore.Identity;

namespace HrPlatform.Data.Models;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public bool TermsAccepted { get; set; }
    public DateTime? TermsAcceptedDate { get; set; }

    // reference to Company entity (for Manager and Company users)
    public int? CompanyId { get; set; }

    public Company? Company { get; set; }

    // reference to DriverProfile entity (for Driver users)
    public int? driverProfileId { get; set; }
    public DriverProfile? DriverProfile { get; set; }

    public ICollection<JobApplication> Applications { get; set; } = [];
}