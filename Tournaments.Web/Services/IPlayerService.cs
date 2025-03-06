using Tournaments.Shared.Models;

namespace Tournaments.Web.Services;

/// <summary>
/// Interface for player-related operations.
/// </summary>
public interface IPlayerService
{
    /// <summary>
    /// Gets all players.
    /// </summary>
    /// <returns>A collection of players.</returns>
    Task<IEnumerable<Player>> GetPlayersAsync();

    /// <summary>
    /// Gets a player by gamertag.
    /// </summary>
    /// <param name="gamertag">The gamertag of the player.</param>
    /// <returns>The player with the specified gamertag.</returns>
    Task<Player> GetPlayerAsync(string gamertag);

    /// <summary>
    /// Creates a new player.
    /// </summary>
    /// <param name="player">The player to create.</param>
    /// <returns>The created player.</returns>
    Task<Player> CreatePlayerAsync(Player player);

    /// <summary>
    /// Updates a player.
    /// </summary>
    /// <param name="gamertag">The gamertag of the player to update.</param>
    /// <param name="player">The updated player information.</param>
    /// <returns>The updated player.</returns>
    Task<Player> UpdatePlayerAsync(string gamertag, Player player);

    /// <summary>
    /// Deletes a player.
    /// </summary>
    /// <param name="gamertag">The gamertag of the player to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeletePlayerAsync(string gamertag);
} 