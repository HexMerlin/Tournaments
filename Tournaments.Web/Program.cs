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

        // Configure HttpClient for API communication
        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5241") });

        // Register services
        builder.Services.AddScoped<IPlayerService, PlayerService>();
        builder.Services.AddScoped<ITournamentService, TournamentService>();
        builder.Services.AddScoped<IRegistrationService, RegistrationService>();
        builder.Services.AddScoped<IApiStatusService, ApiStatusService>();

        await builder.Build().RunAsync();
    }
}
