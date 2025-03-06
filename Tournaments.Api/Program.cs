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

        // Configure the database connection
        builder.Services.AddDbContext<TournamentsApiContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("TournamentsApiContext")
                ?? throw new InvalidOperationException("Connection string 'TournamentsApiContext' not found.")));

        // Add CORS support
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowBlazorApp", policy =>
            {
                policy.WithOrigins("http://localhost:5181")
                      .AllowAnyMethod()
                      .AllowAnyHeader();
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
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Use CORS
        app.UseCors("AllowBlazorApp");

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
