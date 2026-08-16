using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HrPlatform.Data;
using HrPlatform.Data.Entities;
using HrPlatform.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HrPlatform.Tests.Services
{
    public class LeadNoteServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _db;
        private readonly LeadNoteService _service;

        public LeadNoteServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);
            _service = new LeadNoteService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task GetNotesForLeadAsync_ReturnsNotesOrderedByTimestampDescending()
        {
            // Arrange
            int leadId = 1;
            _db.LeadNotes.AddRange(
                new LeadNote { Id = 1, LeadId = leadId, Timestamp = DateTime.UtcNow.AddMinutes(-10), Content = "Old" },
                new LeadNote { Id = 2, LeadId = leadId, Timestamp = DateTime.UtcNow, Content = "New" },
                new LeadNote { Id = 3, LeadId = 2, Timestamp = DateTime.UtcNow, Content = "Other Lead" }
            );
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetNotesForLeadAsync(leadId);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(2, result[0].Id);
            Assert.Equal(1, result[1].Id);
        }

        [Fact]
        public async Task CreateAsync_AddsNoteAndReturnsIt()
        {
            // Arrange
            var note = new LeadNote { LeadId = 1, Content = "Test" };

            // Act
            var result = await _service.CreateAsync(note);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(0, result.Id);
            Assert.Equal(1, await _db.LeadNotes.CountAsync());
        }

        [Fact]
        public async Task UpdateAsync_UpdatesNoteAndSetsIsEdited()
        {
            // Arrange
            var note = new LeadNote { Id = 1, LeadId = 1, Content = "Original", IsEdited = false };
            _db.LeadNotes.Add(note);
            await _db.SaveChangesAsync();

            // Act
            note.Content = "Updated";
            await _service.UpdateAsync(note);

            // Assert
            var updatedNote = await _db.LeadNotes.FindAsync(1);
            Assert.NotNull(updatedNote);
            Assert.Equal("Updated", updatedNote.Content);
            Assert.True(updatedNote.IsEdited);
        }

        [Fact]
        public async Task DeleteAsync_DeletesExistingNote()
        {
            // Arrange
            var note = new LeadNote { Id = 1, LeadId = 1, Content = "Test" };
            _db.LeadNotes.Add(note);
            await _db.SaveChangesAsync();

            // Act
            await _service.DeleteAsync(1);

            // Assert
            Assert.Equal(0, await _db.LeadNotes.CountAsync());
        }

        [Fact]
        public async Task DeleteAsync_DoesNothingIfNoteNotFound()
        {
            // Arrange
            var note = new LeadNote { Id = 1, LeadId = 1, Content = "Test" };
            _db.LeadNotes.Add(note);
            await _db.SaveChangesAsync();

            // Act
            await _service.DeleteAsync(999);

            // Assert
            Assert.Equal(1, await _db.LeadNotes.CountAsync());
        }
    }
}
