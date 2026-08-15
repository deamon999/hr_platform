namespace HrPlatform.Models;

public class UserViewModel
{
    public string UserId { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Roles { get; set; } = null!;
    public string? Password { get; set; }
    public bool IsConfirmed { get; set; }
    public int? CompanyId { get; set; }
    public string? CompanyName { get; set; }
}