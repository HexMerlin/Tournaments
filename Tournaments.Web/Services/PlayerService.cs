using System.Net.Http.Json;
using System.Text.Json;
using Tournaments.Shared.Hateoas;
using Tournaments.Shared.Models;

namespace Tournaments.Web.Services;

/// <summary>
/// Implementation of the IPlayerService interface.
/// </summary>
public class PlayerService : IPlayerService
{
    private readonly HttpClient _httpClient;
    private const string ApiEndpoint = "/api/players";

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayerService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    public PlayerService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Player>> GetPlayersAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<ResourcePlayer>>(ApiEndpoint);
        return response?.Select(r => r.Data) ?? Array.Empty<Player>();
    }

    /// <inheritdoc/>
    public async Task<Player> GetPlayerAsync(string gamertag)
    {
        var response = await _httpClient.GetFromJsonAsync<ResourcePlayer>($"{ApiEndpoint}/{gamertag}");
        return response?.Data ?? throw new InvalidOperationException($"Player with gamertag {gamertag} not found.");
    }

    /// <inheritdoc/>
    public async Task<Player> CreatePlayerAsync(Player player)
    {
        var response = await _httpClient.PostAsJsonAsync(ApiEndpoint, player);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to create player: {errorContent}");
        }
        
        var resourcePlayer = await response.Content.ReadFromJsonAsync<ResourcePlayer>();
        return resourcePlayer?.Data ?? throw new InvalidOperationException("Failed to create player: Response data was null.");
    }

    /// <inheritdoc/>
    public async Task<Player> UpdatePlayerAsync(string gamertag, Player player)
    {
        var response = await _httpClient.PutAsJsonAsync($"{ApiEndpoint}/{gamertag}", player);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to update player: {errorContent}");
        }
        
        // For PUT requests that return 204 No Content
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            return player;
        }
        
        var resourcePlayer = await response.Content.ReadFromJsonAsync<ResourcePlayer>();
        return resourcePlayer?.Data ?? throw new InvalidOperationException($"Failed to update player with gamertag {gamertag}: Response data was null.");
    }

    /// <inheritdoc/>
    public async Task DeletePlayerAsync(string gamertag)
    {
        var response = await _httpClient.DeleteAsync($"{ApiEndpoint}/{gamertag}");
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to delete player: {errorContent}");
        }
    }
} 