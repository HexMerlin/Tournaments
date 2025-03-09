using Tournaments.Shared.Models;

namespace Tournaments.Web.Services;

/// <summary>
/// Interface for tournament-related operations.
/// </summary>
public interface ITournamentService
{
    /// <summary>
    /// Gets all tournaments.
    /// </summary>
    /// <returns>A collection of tournaments.</returns>
    Task<IEnumerable<Tournament>> GetTournamentsAsync();

    /// <summary>
    /// Gets a tournament by name.
    /// </summary>
    /// <param name="name">The name of the tournament.</param>
    /// <param name="includeSubTournaments">Whether to include sub-tournaments.</param>
    /// <returns>The tournament with the specified name.</returns>
    Task<Tournament> GetTournamentAsync(string name, bool includeSubTournaments = false);

    /// <summary>
    /// Creates a new tournament.
    /// </summary>
    /// <param name="tournament">The tournament to create.</param>
    /// <returns>The created tournament.</returns>
    Task<Tournament> CreateTournamentAsync(Tournament tournament);

    /// <summary>
    /// Updates a tournament.
    /// </summary>
    /// <param name="name">The name of the tournament to update.</param>
    /// <param name="tournament">The updated tournament information.</param>
    /// <returns>The updated tournament.</returns>
    Task<Tournament> UpdateTournamentAsync(string name, Tournament tournament);

    /// <summary>
    /// Deletes a tournament.
    /// </summary>
    /// <param name="name">The name of the tournament to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteTournamentAsync(string name);

    /// <summary>
    /// Gets all root tournaments with their complete hierarchy of sub-tournaments.
    /// </summary>
    /// <returns>A collection of root tournaments with their sub-tournament hierarchies.</returns>
    Task<IEnumerable<Tournament>> GetAllTournamentsWithHierarchyAsync();
} 