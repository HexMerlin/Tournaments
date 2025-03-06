using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tournaments.Api.Data;

namespace Tournaments.Api.Tests;

/// <summary>
/// Provides a test fixture for the Tournament Management Application API, using an in-memory database.
/// </summary>
public class TournamentsApiTestFixture : WebApplicationFactory<Program>, IDisposable
{
    private readonly string _databaseName = $"TestDb_{Guid.NewGuid()}";
    public HttpClient Client { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TournamentsApiTestFixture"/> class.
    /// </summary>
    public TournamentsApiTestFixture()
    {
        Client = CreateClient();
        ResetDatabase();
    }

    /// <summary>
    /// Configures the web host for the test fixture.
    /// </summary>
    /// <param name="builder">The web host builder.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext configuration
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TournamentsApiContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Use in-memory database for testing
            services.AddDbContext<TournamentsApiContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            // Build the service provider to ensure the database is created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TournamentsApiContext>();
            db.Database.EnsureCreated();
        });
    }

    /// <summary>
    /// Resets the database by clearing all tables.
    /// </summary>
    public void ResetDatabase()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TournamentsApiContext>();

        // Clear all tables instead of deleting and recreating the database
        if (db.Registration.Any())
        {
            db.Registration.RemoveRange(db.Registration);
        }

        if (db.Tournament.Any())
        {
            db.Tournament.RemoveRange(db.Tournament);
        }

        if (db.Player.Any())
        {
            db.Player.RemoveRange(db.Player);
        }

        db.SaveChanges();
    }
}
