using HrPlatform.Data;
using HrPlatform.Data.Models;
using HrPlatform.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HrPlatform.Services;

public class AdminUserService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    // Inject both the DbContext and UserManager
    public AdminUserService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<List<UserViewModel>> GetAllUsersWithRolesAsync()
    {
        var users = await _context.Users.ToListAsync();
        var userList = new List<UserViewModel>();

        foreach (var user in users)
        {
            // Use UserManager to retrieve the roles assigned to this specific user
            var userRoles = await _userManager.GetRolesAsync(user);

            userList.Add(new UserViewModel
            {
                UserId = user.Id,
                Username = user.UserName,
                Email = user.Email,
                Roles = string.Join(", ", userRoles) // Combine roles into a single string for display
            });
        }

        return userList;
    }

    public async Task<UserViewModel> GetUserByIdAsync(string id)
    {
        var user = await _context.Users.FindAsync(id);
        // Use UserManager to retrieve the roles assigned to this specific user
        var userRoles = await _userManager.GetRolesAsync(user);

        return new UserViewModel
        {
            UserId = user.Id,
            Username = user.UserName,
            Email = user.Email,
            Roles = string.Join(", ", userRoles) // Combine roles into a single string for display
        };
    }

    public async Task DeleteAsync(string id)
    {
        var c = await _context.Users.FindAsync(id);
        if (c is null) return;
        _context.Users.Remove(c);
        await _context.SaveChangesAsync();
    }
}