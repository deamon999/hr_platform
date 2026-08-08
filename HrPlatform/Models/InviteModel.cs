using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HrPlatform.Models;

public class InviteModel : IValidatableObject
{
    public ContactMethod ContactMethod { get; set; } = ContactMethod.Email;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    [Required(ErrorMessage = "Please select a role.")]
    public string Role { get; set; } = string.Empty;

    // Required only when Role == "Manager" — validated manually in HandleSubmit
    public int? CompanyId { get; set; }

    public int? JobId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ContactMethod == ContactMethod.Email)
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                yield return new ValidationResult("Email address is required.", new[] { nameof(Email) });
            }
            else if (!new EmailAddressAttribute().IsValid(Email))
            {
                yield return new ValidationResult("Enter a valid email address.", new[] { nameof(Email) });
            }
        }
        else if (ContactMethod == ContactMethod.Phone)
        {
            if (string.IsNullOrWhiteSpace(Phone))
            {
                yield return new ValidationResult("Phone number is required.", new[] { nameof(Phone) });
            }
            else if (!new PhoneAttribute().IsValid(Phone))
            {
                yield return new ValidationResult("Enter a valid phone number.", new[] { nameof(Phone) });
            }
        }
    }
}