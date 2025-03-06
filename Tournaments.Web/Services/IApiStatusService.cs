namespace Tournaments.Web.Services;

/// <summary>
/// Interface for API status-related operations.
/// </summary>
public interface IApiStatusService
{
    /// <summary>
    /// Gets the current status of the API.
    /// </summary>
    /// <returns>A string representing the API status.</returns>
    Task<string> GetStatusAsync();

    /// <summary>
    /// Resets the database to a clean state.
    /// </summary>
    /// <returns>A string representing the result of the reset operation.</returns>
    Task<string> ResetDatabaseAsync();
} 