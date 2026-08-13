using HrPlatform.Data;
using Microsoft.EntityFrameworkCore;

namespace HrPlatform.Services;

public class DailyMaintenanceService(
    IServiceProvider services,
    ILogger<DailyMaintenanceService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        // Wait for app to fully start
        await Task.Delay(TimeSpan.FromMinutes(1), ct);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                logger.LogInformation("Starting daily maintenance tasks...");
                await CheckCredentialExpiriesAsync();
                await CleanUnconfirmedUsersAsync();
                logger.LogInformation("Daily maintenance tasks completed.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Daily maintenance tasks failed");
            }

            // Run once per day at next midnight
            var now = DateTime.Now;
            var next = now.Date.AddDays(1);
            await Task.Delay(next - now, ct);
        }
    }

    private async Task CleanUnconfirmedUsersAsync()
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var adminUserService = scope.ServiceProvider.GetRequiredService<IAdminUserService>();

        var cutoffDate = DateTime.UtcNow.AddHours(-24);

        var unconfirmedUsers = await db.Users
            .Where(u => !u.EmailConfirmed && u.TermsAcceptedDate <= cutoffDate)
            .ToListAsync();

        if (unconfirmedUsers.Count > 0)
        {
            logger.LogInformation("Found {Count} unconfirmed users older than 24 hours. Initiating cleanup...", unconfirmedUsers.Count);
            
            foreach (var user in unconfirmedUsers)
            {
                logger.LogInformation("Deleting unconfirmed user {UserId} ({Email}) created on {Date}", 
                    user.Id, user.Email, user.TermsAcceptedDate);
                
                await adminUserService.DeleteAsync(user.Id);
            }
        }
    }

    private async Task CheckCredentialExpiriesAsync()
    {
        using var scope = services.CreateScope();
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

            logger.LogInformation(
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
                                                                     <p><a href="#" style="background:#0d6efd;color:#fff;
                                                                        padding:10px 20px;text-decoration:none;border-radius:4px;">
                                                                        Update My Profile</a></p>
                                                                     """;

    private static string BuildMedicalAlert(
        string name, DateOnly expiry, int days) => $"""
                                                    <p>Hi {name},</p>
                                                    <p>Your <strong>DOT medical card</strong> expires on
                                                    <strong>{expiry:MMMM dd, yyyy}</strong> ({days} days).</p>
                                                    <p>An expired medical card disqualifies you from commercial driving.
                                                    Schedule your physical now and update your card details afterward.</p>
                                                    <p><a href="#" style="background:#dc3545;color:#fff;
                                                       padding:10px 20px;text-decoration:none;border-radius:4px;">
                                                       Update Medical Card</a></p>
                                                    """;
}