using HrPlatform.Data.Models;

namespace HrPlatform.Services;

public class JobMatchService : IJobMatchService
{
    public int ComputeScore(DriverProfile profile, Job job)
    {
        int score = 0, max = 0;

        // CDL class (30 points)
        if (job.RequiredCdlClass.HasValue)
        {
            max += 30;
            if (profile.License?.Class == job.RequiredCdlClass.Value)
                score += 30;
        }

        // Endorsements (25 points — partial credit per endorsement)
        if (job.RequiredEndorsements.Any())
        {
            max += 25;
            if (profile.License?.Endorsements != null)
            {
                var matched = job.RequiredEndorsements
                    .Count(e => profile.License.HasEndorsement(e));
                score += 25 * matched / job.RequiredEndorsements.Count;
            }
        }

        // Years experience (20 points)
        if (job.MinYearsExperience > 0)
        {
            max += 20;
            if (profile.YearsOfExperience >= job.MinYearsExperience)
                score += 20;
            else if (profile.YearsOfExperience > 0)
                score += 20 * profile.YearsOfExperience / job.MinYearsExperience;
        }

        // Trailer type (25 points)
        if (job.RequiredTrailerType.HasValue)
        {
            max += 25;
            if (profile.AllTrailerTypes.Contains(job.RequiredTrailerType.Value))
                score += 25;
        }

        return max == 0 ? 100 : score * 100 / max;
    }
}