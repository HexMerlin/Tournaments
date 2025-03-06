using Tournaments.Shared.Models;

namespace Tournaments.Web.Services;

/// <summary>
/// Interface for registration-related operations.
/// </summary>
public interface IRegistrationService
{
    /// <summary>
    /// Registers a player in a tournament.
    /// </summary>
    /// <param name="tournamentName">The name of the tournament.</param>
    /// <param name="playerGamertag">The gamertag of the player.</param>
    /// <returns>The created registration.</returns>
    Task<Registration> RegisterPlayerAsync(string tournamentName, string playerGamertag);

    /// <summary>
    /// Gets a registration by tournament name and player gamertag.
    /// </summary>
    /// <param name="tournamentName">The name of the tournament.</param>
    /// <param name="playerGamertag">The gamertag of the player.</param>
    /// <returns>The registration.</returns>
    Task<Registration> GetRegistrationAsync(string tournamentName, string playerGamertag);

    /// <summary>
    /// Gets all players registered in a tournament.
    /// </summary>
    /// <param name="tournamentName">The name of the tournament.</param>
    /// <returns>A collection of players registered in the tournament.</returns>
    Task<IEnumerable<Player>> GetPlayersInTournamentAsync(string tournamentName);

    /// <summary>
    /// Gets all tournaments a player is registered in.
    /// </summary>
    /// <param name="playerGamertag">The gamertag of the player.</param>
    /// <returns>A collection of tournaments the player is registered in.</returns>
    Task<IEnumerable<Tournament>> GetTournamentsForPlayerAsync(string playerGamertag);

    /// <summary>
    /// Removes a player from a tournament.
    /// </summary>
    /// <param name="tournamentName">The name of the tournament.</param>
    /// <param name="playerGamertag">The gamertag of the player.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemovePlayerFromTournamentAsync(string tournamentName, string playerGamertag);
} 