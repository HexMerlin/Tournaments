using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tournaments.Api.Data;

namespace Tournaments.Api.Controllers;

/// <summary>
/// Provides utility endpoints for testing purposes in the Tournament Management Application.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class TestUtilityController : ControllerBase
{
    private readonly TournamentsApiContext _context;
    private readonly IWebHostEnvironment _environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestUtilityController"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="environment">The web host environment.</param>
    public TestUtilityController(TournamentsApiContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    /// <summary>
    /// Resets the database by removing all players, tournaments, and registrations.
    /// </summary>
    /// <returns>A status message indicating the result of the operation.</returns>
    /// <response code="200">If the database reset is successful.</response>
    /// <response code="400">If the operation is attempted in a production environment.</response>
    /// <response code="500">If an error occurs during the reset operation.</response>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResetDatabase()
    {
        // Allow in Local, Development, and Azure environments, but not in Production
        var allowedEnvironments = new[] { "Local", "Development", "Azure" };
        if (!allowedEnvironments.Contains(_environment.EnvironmentName))
        {
            return BadRequest($"This operation is only allowed in {string.Join(", ", allowedEnvironments)} environments");
        }

        try
        {
            // Remove all registrations
            await _context.Registration.ExecuteDeleteAsync();

            // Remove all tournaments
            await _context.Tournament.ExecuteDeleteAsync();

            // Remove all players
            await _context.Player.ExecuteDeleteAsync();

            return Ok(new { message = "Database reset successful" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error resetting database: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the status of the API without accessing the database.
    /// </summary>
    /// <returns>A simple status message.</returns>
    /// <response code="200">If the API is running.</response>
    [HttpGet("api-status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<object> GetApiStatus()
    {
        return new
        {
            status = "API is running",
            environment = _environment.EnvironmentName,
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        };
    }

    /// <summary>
    /// Gets the status of the database.
    /// </summary>
    /// <returns>Information about the database contents.</returns>
    /// <response code="200">If the database status is retrieved successfully.</response>
    /// <response code="500">If an error occurs while retrieving the database status.</response>
    [HttpGet("status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<object>> GetDatabaseStatus()
    {
        try
        {
            var playerCount = await _context.Player.CountAsync();
            var tournamentCount = await _context.Tournament.CountAsync();
            var registrationCount = await _context.Registration.CountAsync();

            return new
            {
                players = playerCount,
                tournaments = tournamentCount,
                registrations = registrationCount,
                isEmpty = playerCount == 0 && tournamentCount == 0 && registrationCount == 0
            };
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error getting database status: {ex.Message}");
        }
    }
}
