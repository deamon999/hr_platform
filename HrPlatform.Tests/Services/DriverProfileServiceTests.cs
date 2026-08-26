using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HrPlatform.Data;
using HrPlatform.Data.Enums;
using HrPlatform.Data.Models;
using HrPlatform.Models;
using HrPlatform.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using HrPlatform.Data.Entities;
using Xunit;

namespace HrPlatform.Tests.Services;

public class DriverProfileServiceTests
{
    private ApplicationDbContext GetDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetByUserIdAsync_And_GetByIdAsync_ReturnProfile()
    {
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetDbContext(dbName))
        {
            context.DriverProfiles.Add(new DriverProfile 
            { 
                Id = 1, UserId = "user1", FirstName = "John", LastName = "Doe", Email = "j@d.com", PhoneNumber = "123",
                License = new DriverLicense { Id = 1, Class = CdlClass.A, IssuingState="TX", LicenseNumber="1" }
            });
            await context.SaveChangesAsync();
        }

        using (var context = GetDbContext(dbName))
        {
            var service = new DriverProfileService(context, new Mock<IDocumentStorageService>().Object);
            
            var byUser = await service.GetByUserIdAsync("user1");
            Assert.NotNull(byUser);
            Assert.Equal("John", byUser.FirstName);
            Assert.NotNull(byUser.License);
            
            var byId = await service.GetByIdAsync(1);
            Assert.NotNull(byId);
            Assert.Equal("John", byId.FirstName);
            
            Assert.Null(await service.GetByIdAsync(99));
            Assert.Null(await service.GetByUserIdAsync("notfound"));
        }
    }

    [Fact]
    public async Task GetAllPagedAsync_FiltersAndSortsCorrectly()
    {
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetDbContext(dbName))
        {
            context.DriverProfiles.AddRange(
                new DriverProfile { Id = 1, UserId = "u1", Email = "a@a.com", PhoneNumber="1", FirstName = "Alice", LastName = "Smith", YearsOfExperience = 5, License = new DriverLicense { Class = CdlClass.A, IssuingState="TX", LicenseNumber="1" } },
                new DriverProfile { Id = 2, UserId = "u2", Email = "b@a.com", PhoneNumber="2", FirstName = "Bob", LastName = "Jones", YearsOfExperience = 2, License = new DriverLicense { Class = CdlClass.B, IssuingState="TX", LicenseNumber="2" } },
                new DriverProfile { Id = 3, UserId = "u3", Email = "c@a.com", PhoneNumber="3", FirstName = "Charlie", LastName = "Brown", YearsOfExperience = 10, License = new DriverLicense { Class = CdlClass.A, IssuingState="TX", LicenseNumber="3", Endorsements = new List<DriverLicenseEndorsement> { new DriverLicenseEndorsement { Endorsement = CdlEndorsement.Hazmat } } } }
            );
            await context.SaveChangesAsync();
        }

        using (var context = GetDbContext(dbName))
        {
            var service = new DriverProfileService(context, new Mock<IDocumentStorageService>().Object);

            var all = await service.GetAllPagedAsync(new ProfileSearch());
            Assert.Equal(3, all.TotalCount);
            
            var searchName = await service.GetAllPagedAsync(new ProfileSearch { Name = "Alice" });
            Assert.Equal(1, searchName.TotalCount);

            var cdlA = await service.GetAllPagedAsync(new ProfileSearch { CdlClass = CdlClass.A });
            Assert.Equal(2, cdlA.TotalCount);

            var minExp = await service.GetAllPagedAsync(new ProfileSearch { MinYears = 4 });
            Assert.Equal(2, minExp.TotalCount); // Alice, Charlie

            var hazmat = await service.GetAllPagedAsync(new ProfileSearch { RequiredEndorsement = CdlEndorsement.Hazmat });
            Assert.Equal(1, hazmat.TotalCount);
            Assert.Equal(3, hazmat.Items[0].Id);
        }
    }

    [Fact]
    public async Task GetByCompanyPagedAsync_ReturnsOnlyDriversWhoAppliedToCompanyJobs()
    {
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetDbContext(dbName))
        {
            var user1 = new ApplicationUser { Id = "user1" };
            var user2 = new ApplicationUser { Id = "user2" };
            var jobCompany1 = new Job { Id = 1, CompanyId = 1, Title = "Job" };
            
            context.Users.AddRange(user1, user2);
            context.Jobs.Add(jobCompany1);

            context.DriverProfiles.AddRange(
                new DriverProfile { Id = 1, UserId = "user1", FirstName = "Applied", LastName = "One", Email = "1@a.com", PhoneNumber = "1" },
                new DriverProfile { Id = 2, UserId = "user2", FirstName = "DidNot", LastName = "Apply", Email = "2@a.com", PhoneNumber = "2" }
            );

            context.JobApplications.Add(new JobApplication { UserId = "user1", JobId = 1 });

            await context.SaveChangesAsync();
        }

        using (var context = GetDbContext(dbName))
        {
            var service = new DriverProfileService(context, new Mock<IDocumentStorageService>().Object);

            var result = await service.GetByCompanyPagedAsync(new ProfileSearch(), companyId: 1);
            Assert.Single(result.Items);
            Assert.Equal("Applied", result.Items[0].FirstName);
        }
    }

    [Fact]
    public async Task CreateAsync_AddsProfile()
    {
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetDbContext(dbName))
        {
            var service = new DriverProfileService(context, new Mock<IDocumentStorageService>().Object);
            var result = await service.CreateAsync(new DriverProfile { FirstName = "New", LastName = "Driver", UserId = "user", Email="e@e.com", PhoneNumber="1" });
            Assert.NotEqual(0, result.Id);
        }
        
        using (var context = GetDbContext(dbName))
        {
            Assert.Equal(1, await context.DriverProfiles.CountAsync());
        }
    }

    [Fact]
    public async Task UpdateAsync_RejectsUnauthorizedUser()
    {
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetDbContext(dbName))
        {
            context.DriverProfiles.Add(new DriverProfile { Id = 1, UserId = "owner", FirstName = "F", LastName = "L", Email = "e@e.com", PhoneNumber = "1" });
            await context.SaveChangesAsync();
        }

        using (var context = GetDbContext(dbName))
        {
            var service = new DriverProfileService(context, new Mock<IDocumentStorageService>().Object);
            var profile = new DriverProfile { Id = 1, UserId = "hacker", FirstName = "F", LastName = "L", Email = "e@e.com", PhoneNumber = "1" };
            
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.UpdateAsync(profile, "hacker"));
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(new DriverProfile { Id = 99, UserId = "owner", FirstName = "F", LastName = "L", Email = "e@e.com", PhoneNumber = "1" }, "owner"));
        }
    }

    [Fact]
    public async Task UpdateAsync_UpdatesProfileAndPrunesOrphans()
    {
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetDbContext(dbName))
        {
            context.DriverProfiles.Add(new DriverProfile 
            { 
                Id = 1, UserId = "owner", FirstName = "F", LastName = "L", Email = "e@e.com", PhoneNumber = "1",
                EmploymentHistory = new List<DriverEmployment> 
                {
                    new DriverEmployment { Id = 1, CompanyName = "Old Job", JobTitle = "Driver" }
                }
            });
            await context.SaveChangesAsync();
        }

        using (var context = GetDbContext(dbName))
        {
            var service = new DriverProfileService(context, new Mock<IDocumentStorageService>().Object);
            
            var updateProfile = new DriverProfile 
            { 
                Id = 1, UserId = "owner",
                FirstName = "Updated", LastName = "L", Email = "e@e.com", PhoneNumber = "1",
                EmploymentHistory = new List<DriverEmployment>
                {
                    // Adding a new job, old job should be deleted
                    new DriverEmployment { Id = 0, CompanyName = "New Job", JobTitle = "Driver" },
                    new DriverEmployment { Id = 0, CompanyName = "", JobTitle = "" } // Should be pruned
                }
            };

            await service.UpdateAsync(updateProfile, "owner");
        }

        using (var context = GetDbContext(dbName))
        {
            var updated = await context.DriverProfiles.Include(p => p.EmploymentHistory).FirstAsync(p => p.Id == 1);
            Assert.Equal("Updated", updated.FirstName);
            Assert.Single(updated.EmploymentHistory);
            Assert.Equal("New Job", updated.EmploymentHistory.First().CompanyName);
        }
    }

    [Fact]
    public async Task DeleteAsync_RejectsUnauthorizedUser()
    {
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetDbContext(dbName))
        {
            context.DriverProfiles.Add(new DriverProfile { Id = 1, UserId = "owner", FirstName="F", LastName="L", Email="e", PhoneNumber="1" });
            await context.SaveChangesAsync();
        }

        using (var context = GetDbContext(dbName))
        {
            var service = new DriverProfileService(context, new Mock<IDocumentStorageService>().Object);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.DeleteAsync(1, "hacker", false));
        }
    }

    [Fact]
    public async Task DeleteAsync_DeletesProfile_ForOwnerOrAdmin()
    {
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetDbContext(dbName))
        {
            context.DriverProfiles.Add(new DriverProfile { Id = 1, UserId = "owner", FirstName="F", LastName="L", Email="e", PhoneNumber="1", Documents = [new DocumentFile { Id = "doc1", FilePath = "path1", ContentType = "pdf", FileName="f.pdf" }] });
            context.DriverProfiles.Add(new DriverProfile { Id = 2, UserId = "owner", FirstName="F", LastName="L", Email="e", PhoneNumber="1" });
            await context.SaveChangesAsync();
        }

        var mockStorage = new Mock<IDocumentStorageService>();

        using (var context = GetDbContext(dbName))
        {
            var service = new DriverProfileService(context, mockStorage.Object);
            
            // Delete as owner
            await service.DeleteAsync(1, "owner", false);
            
            // Delete as admin
            await service.DeleteAsync(2, "admin", true);
        }

        using (var context = GetDbContext(dbName))
        {
            Assert.Null(await context.DriverProfiles.FindAsync(1));
            Assert.Null(await context.DriverProfiles.FindAsync(2));
            Assert.Empty(context.DocumentFiles);
        }

        mockStorage.Verify(x => x.DeleteAsync("doc1"), Times.Once);
    }

    // ============================================================================
    // NEW REGRESSION TESTS — Wizard flow bug fixes
    // ============================================================================

    [Fact]
    public async Task UpdateAsync_DeletesOrphanedEducations()
    {
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetDbContext(dbName))
        {
            context.DriverProfiles.Add(new DriverProfile
            {
                Id = 1, UserId = "owner", FirstName = "F", LastName = "L", Email = "e@e.com", PhoneNumber = "1",
                Educations = new List<DriverEducation>
                {
                    new DriverEducation { Id = 1, SchoolName = "School A", City = "Dallas", State = "TX", Level = EducationLevel.HighSchoolDiploma },
                    new DriverEducation { Id = 2, SchoolName = "School B", City = "Austin", State = "TX", Level = EducationLevel.BachelorDegree }
                }
            });
            await context.SaveChangesAsync();
        }

        using (var context = GetDbContext(dbName))
        {
            var service = new DriverProfileService(context, new Mock<IDocumentStorageService>().Object);
            var updateProfile = new DriverProfile
            {
                Id = 1, UserId = "owner", FirstName = "F", LastName = "L", Email = "e@e.com", PhoneNumber = "1",
                Educations = new List<DriverEducation>
                {
                    // Keep School A (Id=1), remove School B (Id=2)
                    new DriverEducation { Id = 1, SchoolName = "School A Updated", City = "Dallas", State = "TX", Level = EducationLevel.HighSchoolDiploma }
                }
            };
            await service.UpdateAsync(updateProfile, "owner");
        }

        using (var context = GetDbContext(dbName))
        {
            var updated = await context.DriverProfiles.Include(p => p.Educations).FirstAsync(p => p.Id == 1);
            Assert.Single(updated.Educations);
            Assert.Equal("School A Updated", updated.Educations.First().SchoolName);
            Assert.DoesNotContain(updated.Educations, e => e.SchoolName == "School B");
        }
    }

    [Fact]
    public async Task UpdateAsync_PreservesExistingEducations()
    {
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetDbContext(dbName))
        {
            context.DriverProfiles.Add(new DriverProfile
            {
                Id = 1, UserId = "owner", FirstName = "F", LastName = "L", Email = "e@e.com", PhoneNumber = "1",
                Educations = new List<DriverEducation>
                {
                    new DriverEducation { Id = 1, SchoolName = "School A", City = "Dallas", State = "TX", Level = EducationLevel.HighSchoolDiploma }
                }
            });
            await context.SaveChangesAsync();
        }

        using (var context = GetDbContext(dbName))
        {
            var service = new DriverProfileService(context, new Mock<IDocumentStorageService>().Object);
            var updateProfile = new DriverProfile
            {
                Id = 1, UserId = "owner", FirstName = "Updated", LastName = "L", Email = "e@e.com", PhoneNumber = "1",
                Educations = new List<DriverEducation>
                {
                    new DriverEducation { Id = 1, SchoolName = "School A", City = "Dallas", State = "TX", Level = EducationLevel.HighSchoolDiploma },
                    new DriverEducation { Id = 0, SchoolName = "New School", City = "Houston", State = "TX", Level = EducationLevel.BachelorDegree }
                }
            };
            await service.UpdateAsync(updateProfile, "owner");
        }

        using (var context = GetDbContext(dbName))
        {
            var updated = await context.DriverProfiles.Include(p => p.Educations).FirstAsync(p => p.Id == 1);
            Assert.Equal("Updated", updated.FirstName);
            Assert.Equal(2, updated.Educations.Count);
            Assert.Contains(updated.Educations, e => e.SchoolName == "School A");
            Assert.Contains(updated.Educations, e => e.SchoolName == "New School");
        }
    }

    [Fact]
    public async Task CreateAsync_WithCompletedApplication_PersistsAllFields()
    {
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetDbContext(dbName))
        {
            var service = new DriverProfileService(context, new Mock<IDocumentStorageService>().Object);
            var profile = new DriverProfile
            {
                UserId = "driver1", FirstName = "Jane", LastName = "Doe", Email = "jane@test.com", PhoneNumber = "555-0101",
                IsApplicationCompleted = true,
                LastWizardStep = 9,
                License = new DriverLicense
                {
                    LicenseNumber = "CDL-999", Class = CdlClass.A, IssuingState = "TX",
                    Endorsements = new List<DriverLicenseEndorsement>
                    {
                        new DriverLicenseEndorsement { Endorsement = CdlEndorsement.Hazmat }
                    }
                },
                MedicalCard = new DriverMedicalCard
                {
                    MedicalExaminerName = "Dr. Smith",
                    SelfCertification = SelfCertificationCategory.NonExceptedInterstate
                },
                EmploymentHistory = new List<DriverEmployment>
                {
                    new DriverEmployment { CompanyName = "Acme Trucking", JobTitle = "OTR Driver" }
                },
                Educations = new List<DriverEducation>
                {
                    new DriverEducation { SchoolName = "CDL School", City = "Dallas", State = "TX", Level = EducationLevel.VocationalCertificate }
                },
                Skills = new List<DriverProfileSkill>
                {
                    new DriverProfileSkill { Skill = "Hazmat" }
                },
                ViolationHistory = new List<DriverViolation>
                {
                    new DriverViolation { Type = ViolationType.MovingViolation, Description = "5mph over", OccurredDate = DateOnly.FromDateTime(DateTime.Today) }
                }
            };

            var created = await service.CreateAsync(profile);
            Assert.True(created.Id > 0);
        }

        using (var context = GetDbContext(dbName))
        {
            var loaded = await context.DriverProfiles
                .Include(p => p.License).ThenInclude(l => l!.Endorsements)
                .Include(p => p.MedicalCard)
                .Include(p => p.EmploymentHistory)
                .Include(p => p.Educations)
                .Include(p => p.Skills)
                .Include(p => p.ViolationHistory)
                .FirstAsync();

            Assert.True(loaded.IsApplicationCompleted);
            Assert.Equal(9, loaded.LastWizardStep);
            Assert.NotNull(loaded.License);
            Assert.Equal("CDL-999", loaded.License.LicenseNumber);
            Assert.Single(loaded.License.Endorsements);
            Assert.NotNull(loaded.MedicalCard);
            Assert.Single(loaded.EmploymentHistory);
            Assert.Single(loaded.Educations);
            Assert.Equal("CDL School", loaded.Educations.First().SchoolName);
            Assert.Single(loaded.Skills);
            Assert.Single(loaded.ViolationHistory);
        }
    }

    [Fact]
    public async Task UpdateAsync_WithNullLicense_DoesNotCrash()
    {
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetDbContext(dbName))
        {
            context.DriverProfiles.Add(new DriverProfile
            {
                Id = 1, UserId = "owner", FirstName = "F", LastName = "L", Email = "e@e.com", PhoneNumber = "1",
                License = new DriverLicense { Id = 1, LicenseNumber = "CDL-1", Class = CdlClass.A, IssuingState = "TX" }
            });
            await context.SaveChangesAsync();
        }

        using (var context = GetDbContext(dbName))
        {
            var service = new DriverProfileService(context, new Mock<IDocumentStorageService>().Object);
            var updateProfile = new DriverProfile
            {
                Id = 1, UserId = "owner", FirstName = "Updated", LastName = "L", Email = "e@e.com", PhoneNumber = "1",
                License = null, // Removing license
                MedicalCard = null
            };
            await service.UpdateAsync(updateProfile, "owner");
        }

        using (var context = GetDbContext(dbName))
        {
            var updated = await context.DriverProfiles.Include(p => p.License).Include(p => p.MedicalCard).FirstAsync(p => p.Id == 1);
            Assert.Equal("Updated", updated.FirstName);
            Assert.Null(updated.License);
            Assert.Null(updated.MedicalCard);
        }
    }

    [Fact]
    public async Task UpdateAsync_DeletesDocumentsFromBlobStorage()
    {
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetDbContext(dbName))
        {
            context.DriverProfiles.Add(new DriverProfile
            {
                Id = 1, UserId = "owner", FirstName = "F", LastName = "L", Email = "e@e.com", PhoneNumber = "1",
                Documents = new List<DocumentFile>
                {
                    new DocumentFile { Id = "doc-keep", FilePath = "1/keep", ContentType = "pdf", FileName = "keep.pdf" },
                    new DocumentFile { Id = "doc-remove", FilePath = "1/remove", ContentType = "pdf", FileName = "remove.pdf" }
                }
            });
            await context.SaveChangesAsync();
        }

        var mockStorage = new Mock<IDocumentStorageService>();
        using (var context = GetDbContext(dbName))
        {
            var service = new DriverProfileService(context, mockStorage.Object);
            var updateProfile = new DriverProfile
            {
                Id = 1, UserId = "owner", FirstName = "F", LastName = "L", Email = "e@e.com", PhoneNumber = "1",
                Documents = new List<DocumentFile>
                {
                    new DocumentFile { Id = "doc-keep", FilePath = "1/keep", ContentType = "pdf", FileName = "keep.pdf" }
                    // doc-remove is omitted — should be deleted
                }
            };
            await service.UpdateAsync(updateProfile, "owner");
        }

        mockStorage.Verify(x => x.DeleteAsync("doc-remove"), Times.Once);
        mockStorage.Verify(x => x.DeleteAsync("doc-keep"), Times.Never);

        using (var context = GetDbContext(dbName))
        {
            var docs = await context.DocumentFiles.ToListAsync();
            Assert.Single(docs);
            Assert.Equal("doc-keep", docs[0].Id);
        }
    }

    [Fact]
    public async Task CreateAsync_SetsLastWizardStepCorrectly()
    {
        var dbName = Guid.NewGuid().ToString();
        using (var context = GetDbContext(dbName))
        {
            var service = new DriverProfileService(context, new Mock<IDocumentStorageService>().Object);
            var profile = new DriverProfile
            {
                UserId = "user1", FirstName = "F", LastName = "L", Email = "e@e.com", PhoneNumber = "1",
                LastWizardStep = 5,
                IsApplicationCompleted = false
            };
            var created = await service.CreateAsync(profile);

            Assert.Equal(5, created.LastWizardStep);
            Assert.False(created.IsApplicationCompleted);
        }

        using (var context = GetDbContext(dbName))
        {
            var loaded = await context.DriverProfiles.FirstAsync();
            Assert.Equal(5, loaded.LastWizardStep);
        }
    }
}
