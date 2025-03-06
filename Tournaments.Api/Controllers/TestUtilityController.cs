
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
    /// <response code="400">If the operation is attempted in a non-development environment.</response>
    /// <response code="500">If an error occurs during the reset operation.</response>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResetDatabase()
    {
        // Only allow in development environment
        if (!_environment.IsDevelopment())
        {
            return BadRequest("This operation is only allowed in development environment");
        }

        try
        {
            // Remove all registrations
            var registrations = await _context.Registration.ToListAsync();
            _context.Registration.RemoveRange(registrations);

            // Remove all tournaments
            var tournaments = await _context.Tournament.ToListAsync();
            _context.Tournament.RemoveRange(tournaments);

            // Remove all players
            var players = await _context.Player.ToListAsync();
            _context.Player.RemoveRange(players);

            await _context.SaveChangesAsync();

            return Ok("Database reset completed successfully");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error resetting database: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the current status of the database.
    /// </summary>
    /// <returns>An object containing the counts of players, tournaments, and registrations, and a flag indicating if the database is empty.</returns>
    /// <response code="200">Returns the current status of the database.</response>
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
                Players = playerCount,
                Tournaments = tournamentCount,
                Registrations = registrationCount,
                IsEmpty = playerCount == 0 && tournamentCount == 0 && registrationCount == 0
            };
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error getting database status: {ex.Message}");
        }
    }
}
