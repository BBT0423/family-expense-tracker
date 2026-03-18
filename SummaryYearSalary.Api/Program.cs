using System.Globalization;

using SummaryYearSalary.Api.Helpers;
using SummaryYearSalary.Api.Middlewares;
using SummaryYearSalary.Application;
using SummaryYearSalary.Application.Interfaces.IServices;
using SummaryYearSalary.Domain.Common;
using SummaryYearSalary.Domain.Entities;
using SummaryYearSalary.Infrastructure;
using SummaryYearSalary.Infrastructure.Data;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

using Serilog;

namespace SummaryYearSalary.Api;

public static partial class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        ConfigureBuilderServices(builder);
        ConfigureDatabase(builder);

        static void ConfigureBuilderServices(WebApplicationBuilder builder)
        {
            builder.Host.UseSerilog((context, configuration) =>
                 configuration.ReadFrom.Configuration(builder.Configuration)
                 .Enrich.With<LocalTimeEnricher>()
                 .WriteTo.Console(
                     outputTemplate: "{LocalTimestamp} [{Level}] {Message}{NewLine}{Exception}",
                     formatProvider: CultureInfo.InvariantCulture));

            // Add services to the container.    
            builder.Services.AddApiServiceRegistration(builder.Configuration)
                .AddApplicationServiceRegistration()
                .AddInfrastructureServiceRegistration(builder.Configuration);

            // Register custom fonts for QuestPDF
            var fontsPath = Path.Combine(builder.Environment.ContentRootPath, "Fonts");

            // Register NotoSansThai fonts
            using (var regularFont = File.OpenRead(Path.Combine(fontsPath, "NotoSansThai-Regular.ttf")))
            {
                QuestPDF.Drawing.FontManager.RegisterFont(regularFont);
            }

            using (var boldFont = File.OpenRead(Path.Combine(fontsPath, "NotoSansThai-Bold.ttf")))
            {
                QuestPDF.Drawing.FontManager.RegisterFont(boldFont);
            }

            QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = true;
        }

        static void ConfigureDatabase(WebApplicationBuilder builder)
        {
            builder.Services.AddDbContext<SummaryYearSalaryDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("SummaryYearSalaryConnection")));
        }


        static void HandleGlobalError(IApplicationBuilder errorApp) => errorApp.Run(async context =>
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/problem+json";

            var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
            var problem = new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Status = 500,
                Title = "An unexpected error occurred!",
                Detail = exceptionHandlerPathFeature?.Error.Message,
                Instance = context.Request.Path
            };
            await context.Response.WriteAsJsonAsync(problem);
        });

        var app = builder.Build();
        
        await InitializeDatabaseAsync(app);
        ConfigureMiddleware(app);
        
        // Set global culture info
        var cultureInfo = new CultureInfo("en-US");
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

        // Health check endpoint
        app.MapGet("/health", () => Results.Ok(new
        {
            status = "Healthy",
            timestamp = DateTimeProvider.GetBangkokTimeZone(),
            version = "1.0.0"
        }))
        .AllowAnonymous();

        if (app.Environment.IsDevelopment())
        {
            app.MapGet("/", () => Results.Redirect("/swagger")).AllowAnonymous();
        }

        Log.Information("Starting Secure API application");
        await app.RunAsync();

        static async Task InitializeDatabaseAsync(WebApplication app)
        {
            // ==================== DATABASE INITIALIZATION ====================
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<SummaryYearSalaryDbContext>();
                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    var otpService = services.GetRequiredService<IOtpService>();

                    // Apply migrations
                    await context.Database.MigrateAsync();

                    // Seed roles
                    await SeedRolesAsync(roleManager);

                    // Seed admin user
                    await SeedAdminUserAsync(userManager, otpService);

                    Log.Information("Database initialized successfully");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An error occurred while initializing the database");
                }
            }
        }

        static void ConfigureMiddleware(WebApplication app)
        {
            // Global error handler
            app.UseExceptionHandler(HandleGlobalError);

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            // Custom security middleware
            app.UseSecurityHeaders();              // OWASP #8: Security headers
            app.UseCustomExceptionHandler();       // OWASP #8: Error handling
            app.UseRequestValidation();            // Input validation and suspicious pattern detection
            app.UseAuditLogging();                 // OWASP #9: API inventory and audit

            // Rate limiting (OWASP #4)
            app.UseRateLimiter();

            // CORS
            app.UseCors();

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "Public")),
                RequestPath = "/Public"
            });
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
        }
    }


    static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { "Admin", "User" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                Log.Information("Created role: {Role}", role);
            }
        }
    }

    static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, IOtpService otpService)
    {
        var adminEmail = "admin@innofinite.dev";

        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true,
                IsMfaEnabled = true,
                MfaSecret = otpService.GenerateSecret()
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123456");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                Log.Information("Created admin user: {Email}", adminEmail);
            }
        }
    }
}