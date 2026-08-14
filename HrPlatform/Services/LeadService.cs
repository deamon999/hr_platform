using HrPlatform.Data;
using HrPlatform.Data.Entities;
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

    public async Task<PaginationResult<Lead>> GetLeadsPagedAsync(int pageNumber, int pageSize, int? companyId = null, string? searchTerm = null)
    {
        var query = _db.Leads.AsQueryable();

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
                (l.Phone != null && l.Phone.ToLower().Contains(lowerSearch)) ||
                l.Source.ToLower().Contains(lowerSearch)
            );
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
}
