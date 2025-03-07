using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Tournaments.Api.Data;
using Tournaments.Shared.Hateoas;
using Tournaments.Shared.Models;

namespace Tournaments.Api.Controllers;

/// <summary>
/// Provides API endpoints for managing players in the Tournament Management Application.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class PlayersController : ControllerBase
{
    private readonly TournamentsApiContext _context;
    private readonly ILogger<PlayersController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlayersController"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger for this controller.</param>
    public PlayersController(TournamentsApiContext context, ILogger<PlayersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new player.
    /// </summary>
    /// <param name="player">The player data.</param>
    /// <returns>The created player with hypermedia links.</returns>
    /// <response code="201">Returns the newly created player.</response>
    /// <response code="409">If a player with the same gamertag already exists.</response>
    [HttpPost(Name = "CreatePlayer")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ResourcePlayer>> PostPlayer(Player player)
    {
        // Prevent duplicate gamertags
        if (PlayerExists(player.Gamertag))
            return Conflict($"Player with gamertag '{player.Gamertag}' already exists.");

        _context.Player.Add(player);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            if (PlayerExists(player.Gamertag))
                return Conflict();
            throw;
        }
        catch (ArgumentException ex) when (ex.Message.Contains("same key has already been added"))
        {
            return Conflict();
        }

        // Build resource with hypermedia links for the created player
        var resource = BuildResourcePlayer(player);
        // Return 201 Created with Location header and resource in body
        return CreatedAtAction(nameof(GetPlayer), new { gamertag = player.Gamertag }, resource);
    }

    /// <summary>
    /// Gets all players.
    /// </summary>
    /// <returns>A list of players with hypermedia links.</returns>
    /// <response code="200">Returns the list of players.</response>
    [HttpGet(Name = "GetAllPlayers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ResourcePlayer>>> GetPlayers()
    {
        // Log database connection details
        var connection = _context.Database.GetDbConnection();
        _logger.LogInformation($"Database: {connection.Database} on {connection.DataSource}");
        
        // Log player count for diagnostics
        var count = await _context.Player.CountAsync();
        _logger.LogInformation($"Player count: {count}");
        
        var players = await _context.Player.ToListAsync();
        var resources = players.Select(p => BuildResourcePlayer(p)).ToList();
        return Ok(resources);  // 200 OK with list of players (each with links)
    }

    /// <summary>
    /// Gets a player by gamertag.
    /// </summary>
    /// <param name="gamertag">The player's gamertag.</param>
    /// <returns>The player with hypermedia links.</returns>
    /// <response code="200">Returns the player.</response>
    /// <response code="404">If the player is not found.</response>
    [HttpGet("{gamertag}", Name = "GetPlayer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResourcePlayer>> GetPlayer(string gamertag)
    {
        var player = await _context.Player.FindAsync(gamertag);
        if (player == null)
            return NotFound();

        var resource = BuildResourcePlayer(player);
        return Ok(resource);  // 200 OK with player resource
    }

    /// <summary>
    /// Updates a player's details.
    /// </summary>
    /// <param name="gamertag">The player's gamertag.</param>
    /// <param name="player">The updated player data.</param>
    /// <returns>No content.</returns>
    /// <response code="204">If the update is successful.</response>
    /// <response code="400">If the gamertag in the URL does not match the request body.</response>
    /// <response code="404">If the player is not found.</response>
    [HttpPut("{gamertag}", Name = "UpdatePlayer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutPlayer(string gamertag, Player player)
    {
        if (gamertag != player.Gamertag)
            return BadRequest("Gamertag in URL must match request body.");

        _context.Entry(player).State = EntityState.Modified;
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PlayerExists(gamertag))
                return NotFound();
            throw;
        }

        return NoContent();  // 204 No Content (update successful, no body returned)
    }

    /// <summary>
    /// Deletes a player by gamertag.
    /// </summary>
    /// <param name="gamertag">The player's gamertag.</param>
    /// <returns>No content.</returns>
    /// <response code="204">If the deletion is successful.</response>
    /// <response code="404">If the player is not found.</response>
    [HttpDelete("{gamertag}", Name = "DeletePlayer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePlayer(string gamertag)
    {
        var player = await _context.Player
                                   .Include(p => p.Registrations)
                                   .FirstOrDefaultAsync(p => p.Gamertag == gamertag);
        if (player == null)
            return NotFound();

        // Remove all registrations for this player to maintain referential integrity
        if (player.Registrations?.Count > 0)
            _context.Registration.RemoveRange(player.Registrations);

        _context.Player.Remove(player);
        await _context.SaveChangesAsync();
        return NoContent();  // 204 No Content on successful deletion
    }

    /// <summary>
    /// Gets all tournaments a player is registered in.
    /// </summary>
    /// <param name="gamertag">The player's gamertag.</param>
    /// <returns>A list of tournaments with hypermedia links.</returns>
    /// <response code="200">Returns the list of tournaments.</response>
    /// <response code="404">If the player is not found.</response>
    [HttpGet("{gamertag}/tournaments", Name = "GetTournamentsForPlayer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<ResourceTournament>>> GetTournamentsForPlayer(string gamertag)
    {
        // Verify player exists
        var player = await _context.Player.FindAsync(gamertag);
        if (player == null)
            return NotFound($"Player '{gamertag}' not found.");

        // Get all registrations for the specified player, including tournament data
        var registrations = await _context.Registration
                                .Include(r => r.Tournament)
                                .Where(r => r.PlayerGamertag == gamertag)
                                .ToListAsync();

        // Extract tournaments from registrations and build resources
        var tournamentResources = registrations
            .Select(r => r.Tournament)
            .Select(t => new ResourceTournament(t)
            {
                Links = new List<Link>
                {
                    new(
                        href: $"/api/tournaments/{t.Name}",
                        rel: "self",
                        method: HttpMethod.Get.Method
                    ),
                    new(
                        href: $"/api/tournaments/{t.Name}",
                        rel: "update",
                        method: HttpMethod.Put.Method
                    ),
                    new(
                        href: $"/api/tournaments/{t.Name}",
                        rel: "delete",
                        method: HttpMethod.Delete.Method
                    ),
                    new(
                        href: $"/api/tournaments/{t.Name}/players",
                        rel: "registered-players",
                        method: HttpMethod.Get.Method
                    ),
                    new(
                        href: $"/api/registrations/{t.Name}/{gamertag}",
                        rel: "unregister",
                        method: HttpMethod.Delete.Method
                    )
                }
            })
            .ToList();

        return Ok(tournamentResources);
    }

    /// <summary>
    /// Builds a <see cref="ResourcePlayer"/> with HATEOAS links.
    /// </summary>
    /// <param name="player">The player data.</param>
    /// <returns>A <see cref="ResourcePlayer"/> with HATEOAS links.</returns>
    private ResourcePlayer BuildResourcePlayer(Player player)
    {
        var resource = new ResourcePlayer(player);

        // Self link (GET this player) - using relative path with lowercase route
        resource.Links.Add(new Link(
            href: $"/api/players/{player.Gamertag}",
            rel: "self",
            method: HttpMethod.Get.Method
        ));

        // Update link (PUT this player) - using relative path with lowercase route
        resource.Links.Add(new Link(
            href: $"/api/players/{player.Gamertag}",
            rel: "update",
            method: HttpMethod.Put.Method
        ));

        // Delete link (DELETE this player) - using relative path with lowercase route
        resource.Links.Add(new Link(
            href: $"/api/players/{player.Gamertag}",
            rel: "delete",
            method: HttpMethod.Delete.Method
        ));

        // Get tournaments for this player
        resource.Links.Add(new Link(
            href: $"/api/players/{player.Gamertag}/tournaments",
            rel: "tournaments",
            method: HttpMethod.Get.Method
        ));

        return resource;
    }

    /// <summary>
    /// Checks if a player with the specified gamertag exists.
    /// </summary>
    /// <param name="gamertag">The player's gamertag.</param>
    /// <returns><see langword="true"/> <c>iff</c> a player with the specified gamertag exists.</returns>
    private bool PlayerExists(string gamertag)
        => _context.Player.Any(e => e.Gamertag == gamertag);
}
