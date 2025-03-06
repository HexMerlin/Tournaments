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

        // Load configuration
        builder.Services.AddScoped(sp => 
        {
            // Get configuration
            var configuration = builder.Configuration;
            var apiBaseUrl = configuration["ApiBaseUrl"] ?? "http://localhost:5241";
            
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
