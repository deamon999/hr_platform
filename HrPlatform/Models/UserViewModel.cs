namespace HrPlatform.Models;

public class UserViewModel
{
    public string UserId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Roles { get; set; }
    public string? Password { get; set; }
    public bool IsConfirmed { get; set; }
    public int? CompanyId { get; set; }
    public string? CompanyName { get; set; }
}