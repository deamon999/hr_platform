using HrPlatform.Data.Entities;
using HrPlatform.Data.Enums;
using HrPlatform.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HrPlatform.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<DriverProfile> DriverProfiles => Set<DriverProfile>();
    public DbSet<DriverLicense> DriverLicenses => Set<DriverLicense>();
    public DbSet<DriverMedicalCard> DriverMedicalCards => Set<DriverMedicalCard>();
    public DbSet<DriverEmployment> DriverEmployments => Set<DriverEmployment>();
    public DbSet<DriverEducation> DriverEducations => Set<DriverEducation>();
    public DbSet<DriverCertification> DriverCertifications => Set<DriverCertification>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<Invitation> Invitations { get; set; }
    public DbSet<JobInvitation> JobInvitations { get; set; }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Job>(e =>
        {
            e.Property(j => j.PayPeriod).HasConversion<string>();
            e.Property(j => j.RequiredCdlClass).HasConversion<string>();
            e.Property(j => j.EmploymentType).HasConversion<string>();
            e.Property(j => j.RequiredTrailerType).HasConversion<string>();
            e.Property(j => j.RequiredEndorsements).HasConversion(
                v => string.Join(',', v.Select(e => e.ToString())),
                v => v.Length == 0
                    ? new List<CdlEndorsement>()
                    : v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(Enum.Parse<CdlEndorsement>)
                        .ToList()
            );
        });

        b.Entity<Company>(e =>
        {
            e.HasMany(c => c.Jobs).WithOne(j => j.Company).HasForeignKey(j => j.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<DriverProfile>(e =>
        {
            e.HasIndex(p => p.UserId).IsUnique();
            e.Ignore(p => p.AllTrailerTypes);
        });

        b.Entity<DriverLicense>(e =>
        {
            e.HasOne(l => l.DriverProfile)
                .WithOne(p => p.License)
                .HasForeignKey<DriverLicense>(l => l.DriverProfileId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(l => l.LicenseNumber).IsUnique();
            e.Property(l => l.Class).HasConversion<string>();
            e.Property(l => l.Endorsements).HasConversion(
                v => string.Join(',', v.Select(e => e.ToString())),
                v => v.Length == 0
                    ? new List<CdlEndorsement>()
                    : v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(Enum.Parse<CdlEndorsement>)
                        .ToList()
            );
        });

        b.Entity<DriverMedicalCard>(e =>
        {
            e.HasOne(m => m.DriverProfile)
                .WithOne(p => p.MedicalCard)
                .HasForeignKey<DriverMedicalCard>(m => m.DriverProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<DriverProfile>().Property(p => p.Skills).HasConversion(
            v => string.Join(',', v),
            v => v.Length == 0
                ? new List<string>()
                : v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
        );

        b.Entity<DriverEmployment>(e =>
        {
            e.HasOne(em => em.DriverProfile)
                .WithMany(p => p.EmploymentHistory)
                .HasForeignKey(em => em.DriverProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // Trailer types stored as "DryVan,Flatbed,Reefer" — no junction table
            e.Property(em => em.TrailerTypes).HasConversion(
                v => string.Join(',', v.Select(t => t.ToString())),
                v => v.Length == 0
                    ? new List<TrailerType>()
                    : v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(Enum.Parse<TrailerType>)
                        .ToList()
            );
        });

        b.Entity<DriverEducation>(e =>
        {
            e.HasOne(ed => ed.DriverProfile)
                .WithMany(p => p.EducationHistory)
                .HasForeignKey(ed => ed.DriverProfileId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Property(ed => ed.Level).HasConversion<string>();
        });

        b.Entity<DriverCertification>(e =>
        {
            e.HasOne(c => c.DriverProfile)
                .WithMany(p => p.Certifications)
                .HasForeignKey(c => c.DriverProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<JobApplication>(e =>
        {
            e.HasIndex(a => new { a.JobId, a.DriverProfileId }).IsUnique();
            e.HasOne(a => a.Job)
                .WithMany(j => j.Applications)
                .HasForeignKey(a => a.JobId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.DriverProfile)
                .WithMany(p => p.Applications)
                .HasForeignKey(a => a.DriverProfileId)
                .OnDelete(DeleteBehavior.Restrict);
            e.Property(a => a.Status).HasConversion<string>();
        });

        b.Entity<Invitation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.Email);
            entity.Property(e => e.Token).IsRequired().HasMaxLength(64);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
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
    }
}