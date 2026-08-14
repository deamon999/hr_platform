using HrPlatform.Data;
using HrPlatform.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HrPlatform.Services;

public class LeadNoteService : ILeadNoteService
{
    private readonly ApplicationDbContext _db;

    public LeadNoteService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<LeadNote>> GetNotesForLeadAsync(int leadId)
    {
        return await _db.LeadNotes
            .Include(n => n.AuthorUser)
            .Where(n => n.LeadId == leadId)
            .OrderByDescending(n => n.Timestamp)
            .ToListAsync();
    }

    public async Task<LeadNote> CreateAsync(LeadNote note)
    {
        _db.LeadNotes.Add(note);
        await _db.SaveChangesAsync();
        return note;
    }

    public async Task UpdateAsync(LeadNote note)
    {
        note.IsEdited = true;
        _db.LeadNotes.Update(note);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var note = await _db.LeadNotes.FindAsync(id);
        if (note != null)
        {
            _db.LeadNotes.Remove(note);
            await _db.SaveChangesAsync();
        }
    }
}
