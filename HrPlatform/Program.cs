using HrPlatform.Components;
using HrPlatform.Components.Account;
using HrPlatform.Data;
using HrPlatform.Data.Models;
using HrPlatform.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HrPlatform;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddScoped<IdentityRedirectManager>();
        builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
        builder.Services.AddScoped<IUserTimeZoneService, UserTimeZoneService>();

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                               throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
            // Suppress pending model changes warning - migrations are properly defined
            options.ConfigureWarnings(w =>
                w.Ignore(RelationalEventId.PendingModelChangesWarning));
        }, ServiceLifetime.Transient);
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        builder.Services.AddSingleton<ISmsService, SmsService>();
        builder.Services.AddSingleton<IEmailService, EmailService>();
        builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
        builder.Services.AddSingleton<IJobMatchService, JobMatchService>();

        builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, AppUserClaimsPrincipalFactory>();
        builder.Services.AddTransient<IJobService, JobService>();
        builder.Services.AddTransient<IJobApplicationService, JobApplicationService>();
        builder.Services.AddTransient<IDriverProfileService, DriverProfileService>();
        builder.Services.AddTransient<ICompanyService, CompanyService>();
        builder.Services.AddTransient<IApplicationMessageService, ApplicationMessageService>();
        builder.Services.AddTransient<IAdminUserService, AdminUserService>();
        builder.Services.AddTransient<IInvitationService, InvitationService>();
        builder.Services.AddTransient<IJobInvitationService, JobInvitationService>();
        builder.Services.AddTransient<IDashboardService, DashboardService>();
        builder.Services.AddTransient<ILeadService, LeadService>();
        builder.Services.AddTransient<ILeadNoteService, LeadNoteService>();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddTransient<IDocumentStorageService, DatabaseDocumentStorageService>();
        // Register background job
        builder.Services.AddHostedService<DailyMaintenanceService>();
        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

            // Apply migrations automatically
            var db = services.GetRequiredService<ApplicationDbContext>();
            await db.Database.MigrateAsync();

            // Seed roles + admin user
            await DataSeed.SeedAsync(services, app.Configuration, app.Environment);
        }


        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        app.UseHttpsRedirection();

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        // Add additional endpoints required by the Identity /Account Razor components.
        app.MapAdditionalIdentityEndpoints();

        app.MapGet("/api/documents/{id}", async (string id, bool? download, ApplicationDbContext db) =>
        {
            var doc = await db.DocumentFiles.FindAsync(id);
            if (doc == null) return Results.NotFound();

            if (download == true)
                return Results.File(doc.Data, doc.ContentType, doc.FileName);

            return Results.File(doc.Data, doc.ContentType);
        }).RequireAuthorization();

        app.Run();
    }
}