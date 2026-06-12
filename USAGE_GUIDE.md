# Driver Profile Refactoring - Usage Guide

## Quick Reference

### Filtering by CDL Endorsement

Before (NOT POSSIBLE):
```csharp
// Couldn't filter by endorsement effectively
var drivers = db.DriverProfiles
    .Where(p => p.License.Endorsements.Contains("Hazmat")) // String contains, inefficient
    .ToList();
```

After (EASY):
```csharp
// Filter drivers with specific endorsement
var search = new ProfileSearch { RequiredEndorsement = CdlEndorsement.Hazmat };
var drivers = await _profileService.GetAllPagedAsync(search);
```

This now generates efficient SQL:
```sql
SELECT * FROM "DriverProfiles" dp
INNER JOIN "DriverLicenses" dl ON dp."Id" = dl."DriverProfileId"
INNER JOIN "DriverLicenseEndorsement" dle ON dl."Id" = dle."DriverLicenseId"
WHERE dle."Endorsement" = 'Hazmat'
ORDER BY dp."LastName", dp."FirstName"
```

### Working with Endorsements in Code

```csharp
var profile = await _profileService.GetByIdAsync(driverId);

// Check if driver has specific endorsement
if (profile.License.HasEndorsement(CdlEndorsement.Hazmat))
{
    // Assign hazmat job
}

// Add an endorsement
profile.License.AddEndorsement(CdlEndorsement.Tanker);

// Remove an endorsement
profile.License.RemoveEndorsement(CdlEndorsement.Doubles);

// Get all endorsements as enum values (for UI loops)
var endorsementValues = profile.License.GetEndorsementValues();

// Cascade: deleting profile automatically deletes all license endorsements
db.DriverProfiles.Remove(profile);
await db.SaveChangesAsync(); // All endorsements deleted automatically
```

### Working with Trailer Types

```csharp
var profile = await _profileService.GetByIdAsync(driverId);
var employment = profile.EmploymentHistory.First();

// Check if experienced with specific trailer type
if (employment.HasTrailerType(TrailerType.Tanker))
{
    // Assign tanker job
}

// Add trailer type experience
employment.AddTrailerType(TrailerType.Doubles);

// Remove trailer type
employment.RemoveTrailerType(TrailerType.Flatbed);

// Get all trailer types (for UI loops)
var trailerTypes = employment.GetTrailerTypeValues();

// View all trailer types across all employment history
var allTypes = profile.AllTrailerTypes; // Computed property
```

### Working with Skills

```csharp
var profile = await _profileService.GetByIdAsync(driverId);

// Check if has specific skill (case-insensitive)
if (profile.HasSkill("cross-country"))
{
    // Offer long-haul position
}

// Add skill (automatically trimmed, duplicates prevented)
profile.AddSkill("Cross-Country Driving");

// Remove skill (case-insensitive)
profile.RemoveSkill("cross-country driving");

// Get all skills as strings (for UI loops)
var skillValues = profile.GetSkillValues();

// Cascade: deleting profile automatically deletes all skills
db.DriverProfiles.Remove(profile);
await db.SaveChangesAsync(); // All skills deleted automatically
```

### In UI Components (Razor)

**Before (ProfileCreateEdit.razor):**
```razor
@foreach (var end in Enum.GetValues<CdlEndorsement>())
{
    <input type="checkbox" 
           checked="@profile.License.Endorsements.Contains(end)"
           @onchange="e => ToggleEndorsement(profile.License.Endorsements, end, (bool)e.Value!)"/>
}

@foreach (var skill in profile.Skills)
{
    <span>@skill</span> <!-- skill is a string -->
}
```

**After (ProfileCreateEdit.razor):**
```razor
@foreach (var end in Enum.GetValues<CdlEndorsement>())
{
    <input type="checkbox" 
           checked="@profile.License.HasEndorsement(end)"
           @onchange="e => ToggleEndorsement(profile.License, end, (bool)e.Value!)"/>
}

@foreach (var skill in profile.Skills)
{
    <span>@skill.Skill</span> <!-- Access Skill property -->
}
```

### Advanced Filtering Scenarios

**Find drivers with multiple endorsements:**
```csharp
var requiredEndorsements = new[] { CdlEndorsement.Hazmat, CdlEndorsement.Tanker };

var drivers = db.DriverProfiles
    .Include(p => p.License)
    .ThenInclude(l => l.Endorsements)
    .Where(p => p.License != null &&
               requiredEndorsements.All(e => 
                   p.License.Endorsements.Any(le => le.Endorsement == e)))
    .ToList();
```

**Find drivers with experience on specific trailer types:**
```csharp
var drivers = db.DriverProfiles
    .Include(p => p.EmploymentHistory)
    .ThenInclude(e => e.TrailerTypes)
    .Where(p => p.EmploymentHistory.Any(empl =>
               empl.TrailerTypes.Any(t => t.TrailerType == TrailerType.Tanker)))
    .ToList();
```

**Find drivers with specific skills:**
```csharp
var drivers = db.DriverProfiles
    .Include(p => p.Skills)
    .Where(p => p.Skills.Any(s => s.Skill.Contains("defensive")))
    .ToList();
```

## Migration Applied

The migration `20260612000000_RefactorDriverProfileToProperRelationalSchema` was automatically applied and:

1. ✅ Created three junction tables with indexes
2. ✅ Migrated all existing data from comma-delimited strings
3. ✅ Dropped old string columns
4. ✅ Set up cascade deletion relationships
5. ✅ Ensured referential integrity

## Performance Improvements

### Before (String-based):
```sql
-- Inefficient: SELECT entire profile, then parse string in C#
SELECT * FROM "DriverProfiles"
-- C# code: profile.License.Endorsements.Contains(CdlEndorsement.Hazmat)
```

### After (Relational):
```sql
-- Efficient: Database filters with index
SELECT dp.* FROM "DriverProfiles" dp
INNER JOIN "DriverLicenses" dl ON dp."Id" = dl."DriverProfileId"
INNER JOIN "DriverLicenseEndorsement" dle ON dl."Id" = dle."DriverLicenseId"
WHERE dle."Endorsement" = 'Hazmat'
-- Index on dle."Endorsement" makes this fast
```

## Common Tasks

### Add new endorsement to job requirement
```csharp
var job = db.Jobs.First();
job.RequiredEndorsements.Add(CdlEndorsement.HazTanker);
await db.SaveChangesAsync();
```

### Filter drivers for a specific job
```csharp
var job = await db.Jobs.Include(j => j.RequiredEndorsements).FirstAsync(j => j.Id == jobId);

var qualifiedDrivers = await db.DriverProfiles
    .Include(p => p.License)
    .ThenInclude(l => l.Endorsements)
    .Where(p => p.License.Class == job.RequiredCdlClass &&
               job.RequiredEndorsements.All(req =>
                   p.License.Endorsements.Any(e => e.Endorsement == req)))
    .ToListAsync();
```

## Troubleshooting

### "Cannot compare enum directly"
Always use the helper methods or access the `.Endorsement`/`.TrailerType` properties:
```csharp
// ❌ WRONG
var drivers = db.DriverProfiles
    .Where(p => p.License.Endorsements == CdlEndorsement.Hazmat)
    .ToList();

// ✅ RIGHT
var drivers = db.DriverProfiles
    .Include(p => p.License)
    .ThenInclude(l => l.Endorsements)
    .Where(p => p.License.Endorsements.Any(e => e.Endorsement == CdlEndorsement.Hazmat))
    .ToList();
```

### "Tracking issue with junction tables"
Always include the junction tables when querying:
```csharp
// ❌ WRONG - Endorsements not loaded
var profile = db.DriverProfiles.First();

// ✅ RIGHT - Endorsements loaded with query
var profile = db.DriverProfiles
    .Include(p => p.License)
    .ThenInclude(l => l.Endorsements)
    .First();
```

### "Skill not found after adding"
Ensure you call `SaveChangesAsync()`:
```csharp
profile.AddSkill("New Skill");
await db.SaveChangesAsync(); // Required to persist

// Now it can be retrieved
var hasSkill = profile.HasSkill("New Skill"); // true
```

