using HrPlatform.Data;
using HrPlatform.Data.Models;
using HrPlatform.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HrPlatform.Services;

public class AdminUserService : IAdminUserService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminUserService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<List<UserViewModel>> GetAllUsersWithRolesAsync()
    {
        var users = await _context.Users
            .Include(applicationUser => applicationUser.Company)
            .ToListAsync();

        return await BuildUserViewModelsAsync(users);
    }

    public async Task<List<UserViewModel>> GetUsersByCompanyWithRolesAsync(int companyId)
    {
        var users = await _context.Users
            .Include(applicationUser => applicationUser.Company)
            .Where(u => u.CompanyId == companyId)
            .ToListAsync();

        return await BuildUserViewModelsAsync(users);
    }

    public async Task<UserViewModel> GetUserByIdAsync(string id)
    {
        var user = await _context.Users
            .Include(applicationUser => applicationUser.Company)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
        {
            throw new KeyNotFoundException($"User {id} not found.");
        }

        var userRoles = await _userManager.GetRolesAsync(user);

        return new UserViewModel
        {
            UserId = user.Id,
            Username = user.UserName,
            Email = user.Email,
            Phone = user.PhoneNumber,
            CompanyId = user.CompanyId,
            CompanyName = user.Company?.Name,
            IsConfirmed = user.EmailConfirmed,
            Roles = string.Join(", ", userRoles)
        };
    }

    public async Task DeleteAsync(string id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user is null)
        {
            return;
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }

    private async Task<List<UserViewModel>> BuildUserViewModelsAsync(List<ApplicationUser> users)
    {
        var userList = new List<UserViewModel>();

        foreach (var user in users)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            userList.Add(new UserViewModel
            {
                UserId = user.Id,
                Username = user.UserName,
                Email = user.Email,
                Phone = user.PhoneNumber,
                CompanyId = user.CompanyId,
                CompanyName = user.Company?.Name,
                IsConfirmed = user.EmailConfirmed,
                Roles = string.Join(", ", userRoles)
            });
        }

        return userList;
    }
}