using System.Net.Http.Json;
using System.Text.Json;
using Tournaments.Shared.Hateoas;
using Tournaments.Shared.Models;

namespace Tournaments.Web.Services;

/// <summary>
/// Implementation of the ITournamentService interface.
/// </summary>
public class TournamentService : ITournamentService
{
    private readonly HttpClient _httpClient;
    private const string ApiEndpoint = "/api/tournaments";

    /// <summary>
    /// Initializes a new instance of the <see cref="TournamentService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    public TournamentService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Tournament>> GetTournamentsAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<ResourceTournament>>(ApiEndpoint);
        return response?.Select(r => r.Data) ?? Array.Empty<Tournament>();
    }

    /// <inheritdoc/>
    public async Task<Tournament> GetTournamentAsync(string name, bool includeSubTournaments = false)
    {
        string url = $"{ApiEndpoint}/{name}";
        if (includeSubTournaments)
        {
            url += "?include=sub-tournaments";
        }

        var response = await _httpClient.GetFromJsonAsync<ResourceTournament>(url);
        return response?.Data ?? throw new InvalidOperationException($"Tournament with name {name} not found.");
    }

    /// <inheritdoc/>
    public async Task<Tournament> CreateTournamentAsync(Tournament tournament)
    {
        var response = await _httpClient.PostAsJsonAsync(ApiEndpoint, tournament);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to create tournament: {errorContent}");
        }
        
        var resourceTournament = await response.Content.ReadFromJsonAsync<ResourceTournament>();
        return resourceTournament?.Data ?? throw new InvalidOperationException("Failed to create tournament: Response data was null.");
    }

    /// <inheritdoc/>
    public async Task<Tournament> UpdateTournamentAsync(string name, Tournament tournament)
    {
        var response = await _httpClient.PutAsJsonAsync($"{ApiEndpoint}/{name}", tournament);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to update tournament: {errorContent}");
        }
        
        // For PUT requests that return 204 No Content
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            return tournament;
        }
        
        var resourceTournament = await response.Content.ReadFromJsonAsync<ResourceTournament>();
        return resourceTournament?.Data ?? throw new InvalidOperationException($"Failed to update tournament with name {name}: Response data was null.");
    }

    /// <inheritdoc/>
    public async Task DeleteTournamentAsync(string name)
    {
        var response = await _httpClient.DeleteAsync($"{ApiEndpoint}/{name}");
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to delete tournament: {errorContent}");
        }
    }
} 