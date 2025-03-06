using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tournaments.Shared.Hateoas;
using Tournaments.Shared.Models;

namespace Tournaments.Web.Services;

/// <summary>
/// Implementation of the IRegistrationService interface.
/// </summary>
public class RegistrationService : IRegistrationService
{
    private readonly HttpClient _httpClient;
    private const string ApiEndpoint = "/api/tournaments";
    private const string RegistrationsEndpoint = "/api/registrations";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="RegistrationService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    public RegistrationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc/>
    public async Task<Registration> RegisterPlayerAsync(string tournamentName, string playerGamertag)
    {
        var response = await _httpClient.PostAsync($"{ApiEndpoint}/{tournamentName}/players/{playerGamertag}", null);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to register player: {errorContent}");
        }
        
        var resourceRegistration = await response.Content.ReadFromJsonAsync<ResourceRegistration>(JsonOptions);
        return resourceRegistration?.Data ?? throw new InvalidOperationException($"Failed to register player {playerGamertag} in tournament {tournamentName}: Response data was null.");
    }

    /// <inheritdoc/>
    public async Task<Registration> GetRegistrationAsync(string tournamentName, string playerGamertag)
    {
        var response = await _httpClient.GetFromJsonAsync<ResourceRegistration>($"{ApiEndpoint}/{tournamentName}/players/{playerGamertag}", JsonOptions);
        return response?.Data ?? throw new InvalidOperationException($"Registration for player {playerGamertag} in tournament {tournamentName} not found.");
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Player>> GetPlayersInTournamentAsync(string tournamentName)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{ApiEndpoint}/{tournamentName}/players");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Failed to get players in tournament: {errorContent}");
            }
            
            var jsonString = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(jsonString);
            
            if (jsonDocument.RootElement.TryGetProperty("players", out var playersElement))
            {
                var players = new List<Player>();
                
                foreach (var playerElement in playersElement.EnumerateArray())
                {
                    if (playerElement.TryGetProperty("player", out var playerData))
                    {
                        try
                        {
                            var options = new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            };
                            
                            var player = JsonSerializer.Deserialize<Player>(playerData.ToString(), options);
                            if (player != null)
                            {
                                players.Add(player);
                            }
                        }
                        catch (JsonException ex)
                        {
                            Console.WriteLine($"Error deserializing player: {ex.Message}");
                            Console.WriteLine($"JSON data: {playerData}");
                        }
                    }
                }
                
                return players;
            }
            
            return Array.Empty<Player>();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error loading players in tournament: {ex.Message}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Tournament>> GetTournamentsForPlayerAsync(string playerGamertag)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/players/{playerGamertag}/tournaments");
            
            if (!response.IsSuccessStatusCode)
            {
                // If player has no tournaments, return an empty list instead of throwing an error
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return Array.Empty<Tournament>();
                }
                
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Failed to get tournaments for player: {errorContent}");
            }
            
            var resourceTournaments = await response.Content.ReadFromJsonAsync<List<ResourceTournament>>(JsonOptions);
            return resourceTournaments?.Select(r => r.Data) ?? Array.Empty<Tournament>();
        }
        catch (HttpRequestException ex)
        {
            // Handle network-related exceptions
            throw new InvalidOperationException($"Error connecting to the API: {ex.Message}", ex);
        }
        catch (Exception ex) when (!(ex is InvalidOperationException))
        {
            // Handle other exceptions (but don't wrap InvalidOperationException again)
            throw new InvalidOperationException($"Error loading tournaments for player: {ex.Message}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task RemovePlayerFromTournamentAsync(string tournamentName, string playerGamertag)
    {
        var response = await _httpClient.DeleteAsync($"{RegistrationsEndpoint}/{tournamentName}/{playerGamertag}");
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to remove player from tournament: {errorContent}");
        }
    }
} 