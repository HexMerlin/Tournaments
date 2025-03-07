using Microsoft.EntityFrameworkCore;
using Tournaments.Api.Data;

namespace Tournaments.Api;

/// <summary>
/// Entry point for the Tournament Management Application.
/// </summary>
public class Program
{
    /// <summary>
    /// Main entry point for the application.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Load environment-specific configuration
        var environmentName = builder.Environment.EnvironmentName;
        Console.WriteLine($"Original environment: {environmentName}");

        // We only want to use "Local" or "Azure" environments
        // Map any other environment to one of these based on configuration
        string effectiveEnvironment;

        // Check if we're running in Azure (either through WEBSITE_SITE_NAME env var or explicit ASPNETCORE_ENVIRONMENT=Azure)
        bool isRunningInAzure = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME")) || 
                                 environmentName.Equals("Azure", StringComparison.OrdinalIgnoreCase);

        if (isRunningInAzure || environmentName.Equals("Production", StringComparison.OrdinalIgnoreCase))
        {
            effectiveEnvironment = "Azure";
        }
        else
        {
            // For all other environments (Development, Staging, etc.), use Local
            effectiveEnvironment = "Local";
        }

        Console.WriteLine($"Using effective environment: {effectiveEnvironment}");

        // Load the appropriate configuration file
        Console.WriteLine($"Loading {effectiveEnvironment} configuration");
        builder.Configuration.AddJsonFile($"appsettings.{effectiveEnvironment}.json", optional: true, reloadOnChange: true);

        // Configure the database connection
        var connectionString = builder.Configuration.GetConnectionString("TournamentsApiContext");
        Console.WriteLine($"Using connection string: {connectionString}");

        builder.Services.AddDbContext<TournamentsApiContext>(options =>
            options.UseSqlServer(connectionString
                ?? throw new InvalidOperationException("Connection string 'TournamentsApiContext' not found.")));

        // Add CORS support
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowBlazorApp", policy =>
            {
                var corsOrigins = builder.Configuration.GetSection("CorsOrigins").Get<string[]>() 
                    ?? new[] { "http://localhost:5181" };
                
                // Check if we have any wildcard origins
                var hasWildcardOrigins = corsOrigins.Any(o => o.Contains("*"));
                
                if (hasWildcardOrigins)
                {
                    // If we have wildcard origins, allow any origin but still restrict to specific domains
                    policy.SetIsOriginAllowed(origin => 
                    {
                        // Allow any localhost origin
                        return origin.StartsWith("http://localhost:") || 
                               origin.StartsWith("https://localhost:");
                    })
                    .AllowAnyMethod()
                    .AllowAnyHeader();
                }
                else
                {
                    // Otherwise, use the specific origins
                    policy.WithOrigins(corsOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                }
            });
        });

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Check Database Connection on Startup
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<TournamentsApiContext>();

            try
            {
                if (dbContext.Database.CanConnect())
                {
                    Console.WriteLine("✅ Database connection successful!");
                }
                else
                {
                    Console.WriteLine("❌ Database connection failed.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Database connection error: {ex.Message}");
            }
        }

        // Configure the HTTP request pipeline.
        // Enable Swagger in all environments
        app.UseSwagger();
        app.UseSwaggerUI();

        // Use CORS
        app.UseCors("AllowBlazorApp");

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
