using HrPlatform.Data;
using HrPlatform.Data.Models;
using HrPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace HrPlatform.Services;

public class CompanyService(ApplicationDbContext db) : ICompanyService
{
    public async Task<List<Company>> GetAllAsync()
    {
        return await db.Companies.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<PaginationResult<Company>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, string? searchName = null, string sortBy = "name")
    {
        var query = db.Companies.AsQueryable();
        if (!string.IsNullOrWhiteSpace(searchName))
        {
            var lowerSearch = searchName.ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(lowerSearch));
        }
        
        if (sortBy == "date")
        {
            query = query.OrderByDescending(c => c.DateRegistered);
        }
        else
        {
            query = query.OrderBy(c => c.Name);
        }
        
        var companies = await query.ToListAsync();
        return companies.Paginate(pageNumber, pageSize);
    }

    public async Task<Company?> GetByIdAsync(int id)
    {
        return await db.Companies.FindAsync(id);
    }

    public async Task<Company> CreateAsync(Company company)
    {
        db.Companies.Add(company);
        await db.SaveChangesAsync();
        return company;
    }

    public async Task UpdateAsync(Company company)
    {
        company.UpdatedAt = DateTime.UtcNow;
        db.Companies.Update(company);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var c = await db.Companies.FindAsync(id);
        if (c is null) return;
        db.Companies.Remove(c);
        await db.SaveChangesAsync();
    }
}