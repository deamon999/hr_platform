using System.ComponentModel.DataAnnotations;

namespace HrPlatform.Models;

public class InviteModel
{
    public ContactMethod ContactMethod { get; set; } = ContactMethod.Email;

    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Enter a valid phone number.")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Please select a role.")]
    public string Role { get; set; } = string.Empty;

    // Required only when Role == "Manager" — validated manually in HandleSubmit
    public int? CompanyId { get; set; }

    public int? JobId { get; set; }
}