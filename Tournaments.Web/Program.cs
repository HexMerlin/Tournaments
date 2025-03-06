using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Tournaments.Web;
using Tournaments.Web.Services;

namespace Tournaments.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        var environment = builder.HostEnvironment.Environment;
        Console.WriteLine($"Running in environment: {environment}");
        
        // In Blazor WebAssembly, configuration files are loaded from wwwroot by default
        // No need to explicitly load them, but we'll log what we're using
        
        // Configure services with the loaded configuration
        builder.Services.AddScoped(sp => 
        {
            // Get configuration
            var configuration = builder.Configuration;
            var apiBaseUrl = configuration["ApiBaseUrl"];
            
            // Fallback values based on environment
            if (string.IsNullOrEmpty(apiBaseUrl))
            {
                apiBaseUrl = environment == "Azure" 
                    ? "https://tournaments-api.azurewebsites.net" 
                    : "https://localhost:7169";
                
                Console.WriteLine($"Warning: ApiBaseUrl not found in configuration, using fallback: {apiBaseUrl}");
            }
            
            Console.WriteLine($"Using API base URL: {apiBaseUrl}");
            
            return new HttpClient { BaseAddress = new Uri(apiBaseUrl) };
        });

        // Register services
        builder.Services.AddScoped<IPlayerService, PlayerService>();
        builder.Services.AddScoped<ITournamentService, TournamentService>();
        builder.Services.AddScoped<IRegistrationService, RegistrationService>();
        builder.Services.AddScoped<IApiStatusService, ApiStatusService>();

        await builder.Build().RunAsync();
    }
}
