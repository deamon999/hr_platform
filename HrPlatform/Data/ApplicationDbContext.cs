using HrPlatform.Data.Entities;
using HrPlatform.Data.Enums;
using HrPlatform.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HrPlatform.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<DriverProfile> DriverProfiles => Set<DriverProfile>();
    public DbSet<DriverLicense> DriverLicenses => Set<DriverLicense>();
    public DbSet<DriverMedicalCard> DriverMedicalCards => Set<DriverMedicalCard>();
    public DbSet<DriverEmployment> DriverEmployments => Set<DriverEmployment>();
    public DbSet<DriverEducation> DriverEducations => Set<DriverEducation>();
    public DbSet<DriverLicenseEndorsement> DriverLicenseEndorsements => Set<DriverLicenseEndorsement>();
    public DbSet<DriverEmploymentTrailerType> DriverEmploymentTrailerTypes => Set<DriverEmploymentTrailerType>();
    public DbSet<DriverProfileSkill> DriverProfileSkills => Set<DriverProfileSkill>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<Invitation> Invitations { get; set; }
    public DbSet<JobInvitation> JobInvitations { get; set; }
    public DbSet<ApplicationMessage> ApplicationMessages => Set<ApplicationMessage>();
    public DbSet<DriverViolation> DriverViolations => Set<DriverViolation>();
    public DbSet<DocumentFile> DocumentFiles { get; set; }
    public DbSet<Lead> Leads { get; set; }
    public DbSet<LeadNote> LeadNotes { get; set; }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<ApplicationUser>(e =>
        {
            e.HasOne(u => u.DriverProfile)
                .WithOne(dp => dp.User)
                .HasForeignKey<DriverProfile>(dp => dp.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            e.Ignore(u => u.driverProfileId);

            // Deletes the user if their associated Company is deleted
            e.HasOne(u => u.Company)
                .WithMany(c => c.Users)
                .HasForeignKey(u => u.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<DriverProfile>(e =>
        {
            e.Ignore(p => p.AllTrailerTypes);
        });

        b.Entity<DriverEducation>(e =>
        {
            e.Property(p => p.Level).HasConversion<string>();
        });

        b.Entity<DriverLicense>(e =>
        {
            e.HasOne(l => l.DriverProfile)
                .WithOne(p => p.License)
                .HasForeignKey<DriverLicense>(l => l.DriverProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(l => l.LicenseNumber).IsUnique();
            e.Property(l => l.Class).HasConversion<string>();
        });

        b.Entity<DriverMedicalCard>(e =>
        {
            e.HasOne(m => m.DriverProfile)
                .WithOne(p => p.MedicalCard)
                .HasForeignKey<DriverMedicalCard>(m => m.DriverProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<DriverEmployment>(e =>
        {
            e.HasOne(em => em.DriverProfile)
                .WithMany(p => p.EmploymentHistory)
                .HasForeignKey(em => em.DriverProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<DriverViolation>(e => {
            e.HasOne(v => v.DriverProfile)
                .WithMany(p => p.ViolationHistory)
                .HasForeignKey(v => v.DriverProfileId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Property(v => v.Type).HasConversion<string>();
        });

        b.Entity<DocumentFile>(e =>
        {
            e.HasOne(x => x.DriverProfile)
                .WithMany(p => p.Documents)
                .HasForeignKey(x => x.DriverProfileId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);
        });

        //**** Junction Tables ****

        b.Entity<DriverLicenseEndorsement>(dle =>
        {
            dle.HasKey(x => x.Id);
            dle.HasOne(x => x.DriverLicense)
                .WithMany(l => l.Endorsements)
                .HasForeignKey(x => x.DriverLicenseId)
                .OnDelete(DeleteBehavior.Cascade);
            dle.Property(x => x.Endorsement).HasConversion<string>();
            // Index for faster queries by endorsement type
            dle.HasIndex(x => x.Endorsement);
        });

        b.Entity<DriverEmploymentTrailerType>(dett =>
        {
            dett.HasKey(x => x.Id);
            dett.HasOne(x => x.DriverEmployment)
                .WithMany(em => em.TrailerTypes)
                .HasForeignKey(x => x.DriverEmploymentId)
                .OnDelete(DeleteBehavior.Cascade);
            dett.Property(x => x.TrailerType).HasConversion<string>();
            // Index for faster queries by trailer type
            dett.HasIndex(x => x.TrailerType);
        });

        b.Entity<DriverProfileSkill>(dps =>
        {
            dps.HasKey(x => x.Id);
            dps.HasOne(x => x.DriverProfile)
                .WithMany(p => p.Skills)
                .HasForeignKey(x => x.DriverProfileId)
                .OnDelete(DeleteBehavior.Cascade);
            // Index for faster skill lookups
            dps.HasIndex(x => x.Skill);
        });

        //*******************************************

        b.Entity<Job>(e =>
        {
            e.Property(j => j.PayPeriod).HasConversion<string>();
            e.Property(j => j.RequiredCdlClass).HasConversion<string>();
            e.Property(j => j.EmploymentType).HasConversion<string>();
            e.Property(j => j.RequiredTrailerType).HasConversion<string>();
            var endorsementComparer = new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<CdlEndorsement>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList());

            e.Property(j => j.RequiredEndorsements).HasConversion(
                v => string.Join(',', v.Select(x => x.ToString())),
                v => v.Length == 0
                    ? new List<CdlEndorsement>()
                    : v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(Enum.Parse<CdlEndorsement>)
                        .ToList()
            ).Metadata.SetValueComparer(endorsementComparer);
        });

        b.Entity<Company>(e =>
        {
            e.HasMany(c => c.Jobs)
                .WithOne(j => j.Company)
                .HasForeignKey(j => j.CompanyId)
                // CHANGED: Restrict -> Cascade
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<JobApplication>(e =>
        {
            e.HasIndex(a => new { a.JobId, a.UserId }).IsUnique();
            e.HasOne(a => a.Job)
                .WithMany(j => j.Applications)
                .HasForeignKey(a => a.JobId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.User)
                .WithMany(p => p.Applications)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Property(a => a.Status).HasConversion<string>();
        });

        b.Entity<ApplicationMessage>(e =>
        {
            e.HasKey(m => m.Id);
            e.HasOne(m => m.JobApplication)
                .WithMany()
                .HasForeignKey(m => m.JobApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Invitation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.Phone);
            entity.Property(e => e.Token).IsRequired().HasMaxLength(64);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);

            // 2. UPDATE THESE TO USE THE NAVIGATION PROPERTIES
            entity.HasOne(e => e.Company)
                .WithMany() // Leave empty if Company doesn't have a List<Invitation>
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Job)
                .WithMany() // Leave empty if Job doesn't have a List<Invitation>
                .HasForeignKey(e => e.JobId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── JobInvitation ────────────────────────────────────
        b.Entity<JobInvitation>(e =>
        {
            e.HasKey(x => x.Id);

            // One invitation per driver per job
            e.HasIndex(x => new { x.UserId, x.JobId }).IsUnique();

            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Job)
                .WithMany()
                .HasForeignKey(x => x.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            e.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(20);
        });

        b.Entity<Lead>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Company)
                .WithMany()
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.ConvertedUser)
                .WithMany()
                .HasForeignKey(x => x.ConvertedUserId)
                .OnDelete(DeleteBehavior.SetNull);
            e.Property(x => x.Status).HasConversion<string>();

            e.HasIndex(x => new { x.CompanyId, x.Email }).IsUnique().AreNullsDistinct(false);
            e.HasIndex(x => new { x.CompanyId, x.Phone }).IsUnique().AreNullsDistinct(false);
        });

        b.Entity<LeadNote>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Lead)
                .WithMany(l => l.Notes)
                .HasForeignKey(x => x.LeadId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.AuthorUser)
                .WithMany()
                .HasForeignKey(x => x.AuthorUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
