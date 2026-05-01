using HrPlatform.Data;
using HrPlatform.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HrPlatform.Services;

public class InvitationService : IInvitationService
{
    private readonly ApplicationDbContext _db;

    public InvitationService(ApplicationDbContext db)
    {
        _db = db;
    }
    
    public async Task<List<Invitation>> GetRecentInvitationsAsync(int count = 20)
    {
        return await _db.Invitations
            .OrderByDescending(i => i.CreatedAt)
            .Take(count)
            .ToListAsync();
    }


    public async Task<Invitation> CreateAsync(Invitation invitation)
    {
        _db.Invitations.Add(invitation);
        await _db.SaveChangesAsync();
        return invitation;
    }

    public async Task<bool> InvitationExistsAsync(string email, string phone)
    {
        return await _db.Invitations.AnyAsync(i =>
            (i.Email == email || i.Phone == phone) &&
            !i.IsUsed &&
            i.ExpiresAt > DateTime.UtcNow);
    }


    public Task<Invitation?> GetValidAsync(string token) =>
        _db.Invitations.FirstOrDefaultAsync(i =>
            i.Token == token &&
            !i.IsUsed &&
            i.ExpiresAt > DateTime.UtcNow);

    public async Task MarkUsedAsync(string token)
    {
        var inv = await _db.Invitations.FirstOrDefaultAsync(i => i.Token == token);
        if (inv is not null)
        {
            inv.IsUsed = true;
            await _db.SaveChangesAsync();
        }
    }

    public Task<bool> PendingExistsAsync(string email) =>
        _db.Invitations.AnyAsync(i =>
            i.Email == email.Trim().ToLowerInvariant() &&
            !i.IsUsed &&
            i.ExpiresAt > DateTime.UtcNow);
}