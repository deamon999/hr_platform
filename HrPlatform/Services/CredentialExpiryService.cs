using HrPlatform.Data;
using Microsoft.EntityFrameworkCore;

namespace HrPlatform.Services;

public class CredentialExpiryService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<CredentialExpiryService> _logger;

    public CredentialExpiryService(
        IServiceProvider services,
        ILogger<CredentialExpiryService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        // Wait for app to fully start
        await Task.Delay(TimeSpan.FromMinutes(1), ct);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await RunChecksAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Credential expiry check failed");
            }

            // Run once per day at next midnight
            var now = DateTime.Now;
            var next = now.Date.AddDays(1);
            await Task.Delay(next - now, ct);
        }
    }

    private async Task RunChecksAsync()
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var email = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var in30 = today.AddDays(30);
        var in7 = today.AddDays(7);

        // --- CDL License alerts ---
        var expiringLicenses = await db.DriverLicenses
            .Include(l => l.DriverProfile)
            .ThenInclude(p => p.User)
            .Where(l => l.ExpiryDate == in30 || l.ExpiryDate == in7)
            .ToListAsync();

        foreach (var lic in expiringLicenses)
        {
            var user = lic.DriverProfile?.User;
            if (user?.Email is null) continue;

            int daysLeft = lic.ExpiryDate.DayNumber - today.DayNumber;
            var name = $"{user.FirstName} {user.LastName}".Trim();

            await email.SendEmailAsync(
                user.Email, name,
                $"Action required: Your CDL expires in {daysLeft} days",
                BuildLicenseAlert(name, lic.Class.ToString(), lic.ExpiryDate, daysLeft));

            _logger.LogInformation(
                "License expiry alert sent to {Email} ({Days} days)",
                user.Email, daysLeft);
        }

        // --- DOT Medical Card alerts ---
        var expiringCards = await db.DriverMedicalCards
            .Include(m => m.DriverProfile)
            .ThenInclude(p => p.User)
            .Where(m => m.ExpiryDate == in30 || m.ExpiryDate == in7)
            .ToListAsync();

        foreach (var card in expiringCards)
        {
            var user = card.DriverProfile?.User;
            if (user?.Email is null) continue;

            int daysLeft = card.ExpiryDate.DayNumber - today.DayNumber;
            var name = $"{user.FirstName} {user.LastName}".Trim();

            await email.SendEmailAsync(
                user.Email, name,
                $"Action required: Your DOT medical card expires in {daysLeft} days",
                BuildMedicalAlert(name, card.ExpiryDate, daysLeft));
        }
    }

    private static string BuildLicenseAlert(
        string name, string cdlClass, DateOnly expiry, int days) => $"""
                                                                     <p>Hi {name},</p>
                                                                     <p>Your <strong>CDL-{cdlClass}</strong> license expires on
                                                                     <strong>{expiry:MMMM dd, yyyy}</strong> ({days} days from today).</p>
                                                                     <p>Log in to update your license details once renewed so your
                                                                     profile stays visible to recruiters.</p>
                                                                     <p><a href=\"#\" style=\"background:#0d6efd;color:#fff;
                                                                        padding:10px 20px;text-decoration:none;border-radius:4px;\">
                                                                        Update My Profile</a></p>
                                                                     """;

    private static string BuildMedicalAlert(
        string name, DateOnly expiry, int days) => $"""
                                                    <p>Hi {name},</p>
                                                    <p>Your <strong>DOT medical card</strong> expires on
                                                    <strong>{expiry:MMMM dd, yyyy}</strong> ({days} days).</p>
                                                    <p>An expired medical card disqualifies you from commercial driving.
                                                    Schedule your physical now and update your card details afterward.</p>
                                                    <p><a href=\"#\" style=\"background:#dc3545;color:#fff;
                                                       padding:10px 20px;text-decoration:none;border-radius:4px;\">
                                                       Update Medical Card</a></p>
                                                    """;
}