using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tournaments.Api.Data;
using Tournaments.Shared.Hateoas;
using Tournaments.Shared.Models;

namespace Tournaments.Api.Controllers;

/// <summary>
/// Provides API endpoints for managing tournaments in the Tournament Management Application.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class TournamentsController : ControllerBase
{
    private readonly TournamentsApiContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="TournamentsController"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public TournamentsController(TournamentsApiContext context) => _context = context;

    /// <summary>
    /// Creates a new tournament.
    /// </summary>
    /// <param name="tournament">The tournament data.</param>
    /// <returns>The created tournament with hypermedia links.</returns>
    /// <response code="201">Returns the newly created tournament.</response>
    /// <response code="409">If a tournament with the same name already exists.</response>
    /// <response code="400">If the maximum tournament nesting depth is exceeded.</response>
    [HttpPost(Name = "CreateTournament")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResourceTournament>> PostTournament(Tournament tournament)
    {
        // Check for duplicate tournament name
        if (TournamentExists(tournament.Name))
            return Conflict($"Tournament '{tournament.Name}' already exists.");

        // Enforce nesting depth constraint (max 5 levels deep)
        if (!string.IsNullOrEmpty(tournament.ParentTournamentName))
        {
            var parent = await _context.Tournament.FindAsync(tournament.ParentTournamentName);
            if (parent != null)
            {
                int parentDepth = await GetTournamentDepth(parent);
                if (parentDepth >= 5)
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Maximum tournament nesting depth exceeded",
                        Detail = "Tournaments can only be nested up to 5 levels deep (parent-child-child-child-child)",
                        Status = StatusCodes.Status400BadRequest
                    });
                }
            }
        }

        _context.Tournament.Add(tournament);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // In case a concurrent request created the same tournament
            if (TournamentExists(tournament.Name))
                return Conflict();
            throw;
        }
        catch (ArgumentException ex) when (ex.Message.Contains("same key has already been added"))
        {
            return Conflict();
        }

        var resource = BuildResourceTournament(tournament);
        return CreatedAtAction(nameof(GetTournament), new { name = tournament.Name }, resource);
    }

    /// <summary>
    /// Gets all tournaments.
    /// </summary>
    /// <returns>A list of tournaments with hypermedia links.</returns>
    /// <response code="200">Returns the list of tournaments.</response>
    [HttpGet(Name = "GetAllTournaments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ResourceTournament>>> GetTournaments()
    {
        var tournaments = await _context.Tournament.ToListAsync();
        var resources = tournaments.Select(t => BuildResourceTournament(t)).ToList();
        return Ok(resources);
    }

    /// <summary>
    /// Gets a tournament by name.
    /// </summary>
    /// <param name="name">The name of the tournament.</param>
    /// <param name="include">Optional query parameter to include sub-tournaments.</param>
    /// <returns>The tournament with hypermedia links.</returns>
    /// <response code="200">Returns the tournament.</response>
    /// <response code="404">If the tournament is not found.</response>
    [HttpGet("{name}", Name = "GetTournament")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResourceTournament>> GetTournament(string name, [FromQuery] string? include = null)
    {
        Tournament? tournament;

        if (include == "sub-tournaments")
        {
            // Load tournament with its sub-tournaments if requested
            tournament = await _context.Tournament
                                       .Include(t => t.SubTournaments)
                                       .FirstOrDefaultAsync(t => t.Name == name);
        }
        else
        {
            // Load just the tournament
            tournament = await _context.Tournament.FirstOrDefaultAsync(t => t.Name == name);
        }

        if (tournament == null)
            return NotFound();

        // Build the resource with HATEOAS links
        // The BuildResourceTournament method will recursively handle sub-tournaments if they're loaded
        var resource = BuildResourceTournament(tournament);

        return Ok(resource);
    }

    /// <summary>
    /// Updates a tournament's details.
    /// </summary>
    /// <param name="name">The name of the tournament.</param>
    /// <param name="tournament">The updated tournament data.</param>
    /// <returns>No content.</returns>
    /// <response code="204">If the update is successful.</response>
    /// <response code="400">If the tournament name in the URL does not match the request body.</response>
    /// <response code="404">If the tournament is not found.</response>
    [HttpPut("{name}", Name = "UpdateTournament")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutTournament(string name, Tournament tournament)
    {
        if (name != tournament.Name)
        {
            return BadRequest("Tournament name in URL must match the request body.");
        }

        // Find existing tournament
        var existing = await _context.Tournament.FindAsync(name);
        if (existing == null)
            return NotFound();

        // Validate hierarchy constraints if parent tournament is changing
        if (existing.ParentTournamentName != tournament.ParentTournamentName)
        {
            // Prevent circular parent reference
            if (tournament.Name == tournament.ParentTournamentName)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid tournament hierarchy",
                    Detail = "A tournament cannot be its own parent",
                    Status = StatusCodes.Status400BadRequest
                });
            }
            // Check max depth constraint for new parent
            if (!string.IsNullOrEmpty(tournament.ParentTournamentName))
            {
                var parent = await _context.Tournament.FindAsync(tournament.ParentTournamentName);
                if (parent != null)
                {
                    int parentDepth = await GetTournamentDepth(parent);
                    if (parentDepth >= 5)
                    {
                        return BadRequest(new ProblemDetails
                        {
                            Title = "Maximum tournament nesting depth exceeded",
                            Detail = "Tournaments can only be nested up to 5 levels deep (parent-child-child-child-child)",
                            Status = StatusCodes.Status400BadRequest
                        });
                    }
                }
            }
        }

        // Replace the existing tournament with the new data
        existing.ParentTournamentName = tournament.ParentTournamentName;
        existing.SubTournaments = tournament.SubTournaments;
        existing.Registrations = tournament.Registrations;

        _context.Entry(existing).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TournamentExists(name))
                return NotFound();
            throw;
        }

        return NoContent(); // 204 No Content on successful update
    }

    /// <summary>
    /// Deletes a tournament by name.
    /// </summary>
    /// <param name="name">The name of the tournament.</param>
    /// <returns>No content.</returns>
    /// <response code="204">If the deletion is successful.</response>
    /// <response code="404">If the tournament is not found.</response>
    /// <response code="500">If an error occurs during the deletion.</response>
    [HttpDelete("{name}", Name = "DeleteTournament")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteTournament(string name)
    {
        // Find the tournament and include sub-tournaments for cascading delete
        var tournament = await _context.Tournament
                                      .Include(t => t.SubTournaments)
                                      .FirstOrDefaultAsync(t => t.Name == name);
        if (tournament == null)
            return NotFound();

        try
        {
            // Recursively delete tournament and its descendants, plus related registrations
            await DeleteTournamentAndSubTournamentsRecursively(tournament);
            await _context.SaveChangesAsync();
            return NoContent(); // 204 No Content when deletion succeeds
        }
        catch (Exception ex)
        {
            // Log the exception (could also use a logging framework)
            Console.WriteLine($"Error deleting tournament: {ex.Message}");
            // Return 500 with error details
            return StatusCode(500, new { error = "Failed to delete tournament", message = ex.Message });
        }
    }

    /// <summary>
    /// Recursively deletes a tournament, its sub-tournaments, and their registrations.
    /// </summary>
    /// <param name="tournament">The tournament to delete.</param>
    private async Task DeleteTournamentAndSubTournamentsRecursively(Tournament tournament)
    {
        // Ensure sub-tournaments are loaded
        if (!_context.Entry(tournament).Collection(t => t.SubTournaments).IsLoaded)
        {
            await _context.Entry(tournament).Collection(t => t.SubTournaments).LoadAsync();
        }
        // Recursively delete each sub-tournament
        foreach (var sub in tournament.SubTournaments.ToList())
        {
            await DeleteTournamentAndSubTournamentsRecursively(sub);
        }
        // Delete all registrations for this tournament
        var registrations = await _context.Registration
                                          .Where(r => r.TournamentName == tournament.Name)
                                          .ToListAsync();
        if (registrations.Any())
        {
            _context.Registration.RemoveRange(registrations);
        }
        // Remove the tournament itself
        _context.Tournament.Remove(tournament);
    }

    /// <summary>
    /// Computes the nesting depth of a tournament (for hierarchy constraint).
    /// </summary>
    /// <param name="tournament">The tournament to compute the depth for.</param>
    /// <returns>The nesting depth of the tournament.</returns>
    private async Task<int> GetTournamentDepth(Tournament tournament)
    {
        const int rootDepth = 1;
        if (tournament.ParentTournamentName == null)
            return rootDepth;
        // If parent already loaded, recurse
        if (tournament.ParentTournament != null)
            return await GetTournamentDepth(tournament.ParentTournament) + 1;
        // Otherwise, load the parent and recurse
        var parent = await _context.Tournament.FindAsync(tournament.ParentTournamentName);
        return parent == null ? rootDepth : (await GetTournamentDepth(parent) + 1);
    }

    /// <summary>
    /// Checks if a tournament with the specified name exists.
    /// </summary>
    /// <param name="name">The name of the tournament.</param>
    /// <returns><see langword="true"/> <c>iff</c> a tournament with the specified name exists.</returns>
    private bool TournamentExists(string name)
        => _context.Tournament.Any(e => e.Name == name);

    /// <summary>
    /// Builds a <see cref="ResourceTournament"/> with HATEOAS links.
    /// </summary>
    /// <param name="tournament">The tournament data.</param>
    /// <returns>A <see cref="ResourceTournament"/> with HATEOAS links.</returns>
    private ResourceTournament BuildResourceTournament(Tournament tournament)
    {
        ResourceTournament resource = new ResourceTournament(tournament);

        // Self link - using relative URL with lowercase route
        resource.Links.Add(new Link(
            href: $"/api/tournaments/{tournament.Name}",
            rel: "self",
            method: HttpMethod.Get.Method
        ));

        // Update link - using relative URL with lowercase route
        resource.Links.Add(new Link(
            href: $"/api/tournaments/{tournament.Name}",
            rel: "update",
            method: HttpMethod.Put.Method
        ));

        // Delete link - using relative URL with lowercase route
        resource.Links.Add(new Link(
            href: $"/api/tournaments/{tournament.Name}",
            rel: "delete",
            method: HttpMethod.Delete.Method
        ));

        // Get tournament with sub-tournaments link
        resource.Links.Add(new Link(
            href: $"/api/tournaments/{tournament.Name}?include=sub-tournaments",
            rel: "sub-tournaments",
            method: HttpMethod.Get.Method
        ));

        // Register player in tournament link (templated)
        resource.Links.Add(new Link(
            href: $"/api/tournaments/{tournament.Name}/players/{{gamertag}}",
            rel: "register-player",
            method: HttpMethod.Post.Method
        ));

        // View registered players in tournament link
        resource.Links.Add(new Link(
            href: $"/api/tournaments/{tournament.Name}/players",
            rel: "registered-players",
            method: HttpMethod.Get.Method
        ));

        // Link to parent tournament if it exists
        if (!string.IsNullOrEmpty(tournament.ParentTournamentName))
        {
            resource.Links.Add(new Link(
                href: $"/api/tournaments/{tournament.ParentTournamentName}",
                rel: "parent-tournament",
                method: HttpMethod.Get.Method
            ));
        }

        // We don't need to process sub-tournaments here anymore since we're not adding them to the resource
        // The Tournament model already contains its SubTournaments property that will be serialized

        return resource;
    }
}
