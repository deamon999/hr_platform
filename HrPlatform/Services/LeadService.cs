using HrPlatform.Data;
using HrPlatform.Data.Entities;
using HrPlatform.Data.Enums;
using HrPlatform.Data.Models;
using HrPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace HrPlatform.Services;

public class LeadService : ILeadService
{
    private readonly ApplicationDbContext _db;

    public LeadService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PaginationResult<Lead>> GetLeadsPagedAsync(int pageNumber, int pageSize, int? companyId = null, string? searchTerm = null, LeadStatus? status = null, string? addedByUserId = null, bool actionableOnly = false)
    {
        var query = _db.Leads.Include(l => l.AddedByUser).Include(l => l.Notes).AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(l => l.CompanyId == companyId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearch = searchTerm.ToLowerInvariant();
            query = query.Where(l => 
                l.FirstName.ToLower().Contains(lowerSearch) || 
                l.LastName.ToLower().Contains(lowerSearch) || 
                (l.Email != null && l.Email.ToLower().Contains(lowerSearch)) || 
                (l.Phone != null && l.Phone.ToLower().Contains(lowerSearch))
            );
        }

        if (status.HasValue)
        {
            query = query.Where(l => l.Status == status.Value);
        }

        if (!string.IsNullOrEmpty(addedByUserId))
        {
            query = query.Where(l => l.AddedByUserId == addedByUserId);
        }

        if (actionableOnly)
        {
            query = query.Where(l => l.Status != LeadStatus.Hired && l.Status != LeadStatus.NotInterested && l.Status != LeadStatus.Rejected);
        }

        var results = await query
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

        return results.Paginate(pageNumber, pageSize);
    }

    public async Task<Lead?> GetByIdAsync(int id)
    {
        return await _db.Leads
            .Include(l => l.Company)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<Lead> CreateAsync(Lead lead)
    {
        _db.Leads.Add(lead);
        await _db.SaveChangesAsync();
        return lead;
    }

    public async Task UpdateAsync(Lead lead)
    {
        _db.Leads.Update(lead);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var lead = await _db.Leads.FindAsync(id);
        if (lead != null)
        {
            _db.Leads.Remove(lead);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<List<Lead>> GetActionableRemindersAsync(int? companyId)
    {
        var now = DateTime.UtcNow;
        var query = _db.Leads.AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(l => l.CompanyId == companyId.Value);
        }

        return await query
            .Where(l => l.ReminderDate != null && l.ReminderDate <= now && l.Status != LeadStatus.Hired && l.Status != LeadStatus.NotInterested)
            .OrderBy(l => l.ReminderDate)
            .ToListAsync();
    }

    public async Task<List<ApplicationUser>> GetUsersWithLeadsAsync(int? companyId)
    {
        var query = _db.Leads
            .Include(l => l.AddedByUser)
            .Where(l => l.AddedByUser != null);

        if (companyId.HasValue)
        {
            query = query.Where(l => l.CompanyId == companyId.Value);
        }

        return await query
            .Select(l => l.AddedByUser!)
            .Distinct()
            .OrderBy(u => u.Email)
            .ToListAsync();
    }

    public async Task<HashSet<string>> GetRegisteredEmailsAsync(IEnumerable<string> emails)
    {
        var emailList = emails.Where(e => !string.IsNullOrWhiteSpace(e)).Select(e => e.Trim()).ToList();
        if (!emailList.Any()) return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var registered = await _db.Users
            .Where(u => u.Email != null && emailList.Contains(u.Email))
            .Select(u => u.Email)
            .ToListAsync();

        var driverEmails = await _db.DriverProfiles
            .Where(d => emailList.Contains(d.Email))
            .Select(d => d.Email)
            .ToListAsync();

        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in registered) if (e != null) set.Add(e);
        foreach (var e in driverEmails) if (e != null) set.Add(e);

        return set;
    }
}
