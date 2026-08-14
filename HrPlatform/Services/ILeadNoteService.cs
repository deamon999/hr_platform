using HrPlatform.Data.Entities;

namespace HrPlatform.Services;

public interface ILeadNoteService
{
    Task<List<LeadNote>> GetNotesForLeadAsync(int leadId);
    Task<LeadNote> CreateAsync(LeadNote note);
    Task UpdateAsync(LeadNote note);
    Task DeleteAsync(int id);
}
