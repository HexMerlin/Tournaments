using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Tournaments.Api.Data;

/// <summary>
/// Factory for creating DbContext instances at design time (for migrations).
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TournamentsApiContext>
{
    /// <summary>
    /// Creates a new instance of the TournamentsApiContext for use with migrations.
    /// </summary>
    /// <param name="args">Arguments provided by the design-time service.</param>
    /// <returns>A new instance of TournamentsApiContext.</returns>
    public TournamentsApiContext CreateDbContext(string[] args)
    {
        // Get the active environment from args, must be either "Local" or "Azure"
        var environment = args.Length > 0 ? args[0] : "Local";
        
        if (!environment.Equals("Local", StringComparison.OrdinalIgnoreCase) && 
            !environment.Equals("Azure", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Invalid environment '{environment}'. Must be either 'Local' or 'Azure'.");
        }

        Console.WriteLine($"DesignTimeDbContextFactory using environment: {environment}");
        
        // Build configuration
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"appsettings.{environment}.json", optional: false)
            .Build();

        // Get the connection string
        var connectionString = configuration.GetConnectionString("TournamentsApiContext")
            ?? throw new InvalidOperationException($"Connection string 'TournamentsApiContext' not found in appsettings.{environment}.json");
            
        Console.WriteLine($"DesignTimeDbContextFactory using connection string: {connectionString}");

        // Create DbContext options
        var optionsBuilder = new DbContextOptionsBuilder<TournamentsApiContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new TournamentsApiContext(optionsBuilder.Options);
    }
} 