using HrPlatform.Data.Models;

namespace HrPlatform.Services;

public interface IJobMatchService
{
    public int ComputeScore(DriverProfile profile, Job job);
    
    public string ScoreBadgeClass(int score) => score switch
    {
        >= 85 => "bg-success",
        >= 60 => "bg-warning text-dark",
        _     => "bg-danger"
    };

    public string ScoreLabel(int score) => score switch
    {
        >= 85 => "Strong match",
        >= 60 => "Partial match",
        _     => "Low match"
    };
}