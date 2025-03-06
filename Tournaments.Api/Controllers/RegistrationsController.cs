using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tournaments.Api.Data;
using Tournaments.Shared.Hateoas;
using Tournaments.Shared.Models;

namespace Tournaments.Api.Controllers;

/// <summary>
/// Provides API endpoints for managing registrations in the Tournament Management Application.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="RegistrationsController"/> class.
/// </remarks>
/// <param name="context">The database context.</param>
[Route("api/[controller]")]
[ApiController]
public class RegistrationsController(TournamentsApiContext context) : ControllerBase
{
    private readonly TournamentsApiContext _context = context;

    /// <summary>
    /// Registers a player in a tournament.
    /// </summary>
    /// <param name="tournamentName">The name of the tournament.</param>
    /// <param name="gamertag">The player's gamertag.</param>
    /// <returns>The created registration with hypermedia links.</returns>
    /// <response code="201">Returns the newly created registration.</response>
    /// <response code="400">If the tournament name or player gamertag is not provided, or if the player is not registered in the parent tournament.</response>
    /// <response code="404">If the tournament or player is not found.</response>
    /// <response code="409">If the player is already registered in the tournament.</response>
    [HttpPost("/api/tournaments/{tournamentName}/players/{gamertag}", Name = "CreateRegistration")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ResourceRegistration>> PostRegistration(string tournamentName, string gamertag)
    {
        if (string.IsNullOrEmpty(tournamentName) || string.IsNullOrEmpty(gamertag))
            return BadRequest("Tournament name and player gamertag are required.");

        // Verify tournament exists
        Tournament? tournament = await _context.Tournament.FirstOrDefaultAsync(t => t.Name == tournamentName);
        if (tournament == null)
            return NotFound($"Tournament '{tournamentName}' not found.");

        // Verify player exists
        Player? player = await _context.Player.FindAsync(gamertag);
        if (player == null)
            return NotFound($"Player '{gamertag}' not found.");

        // Check if already registered in this tournament
        bool exists = await _context.Registration.AnyAsync(r => r.TournamentName == tournamentName && r.PlayerGamertag == gamertag);
        if (exists)
            return Conflict($"Player '{gamertag}' is already registered in tournament '{tournamentName}'.");

        // If tournament has a parent, enforce that player is registered in the parent first
        if (!string.IsNullOrEmpty(tournament.ParentTournamentName))
        {
            bool inParent = await _context.Registration.AnyAsync(r =>
                                r.TournamentName == tournament.ParentTournamentName &&
                                r.PlayerGamertag == gamertag);
            if (!inParent)
            {
                return BadRequest($"Player must be registered in parent tournament '{tournament.ParentTournamentName}' first.");
            }
        }

        // Create and save the new registration
        var newRegistration = new Registration
        {
            TournamentName = tournamentName,
            PlayerGamertag = gamertag
        };
        _context.Registration.Add(newRegistration);
        await _context.SaveChangesAsync();  // Save to generate Id

        // Build resource and return 201 Created with links
        ResourceRegistration resource = BuildResourceRegistration(newRegistration);
        return CreatedAtAction(
            nameof(GetRegistrationByTournamentAndPlayer),
            new { tournamentName, gamertag },
            resource
        );
    }

    /// <summary>
    /// Gets all registrations.
    /// </summary>
    /// <returns>A list of registrations with hypermedia links.</returns>
    /// <response code="200">Returns the list of registrations.</response>
    [HttpGet(Name = "GetAllRegistrations")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ResourceRegistration>>> GetRegistrations()
    {
        List<Registration> registrations = await _context.Registration.ToListAsync();
        List<ResourceRegistration> resources = [.. registrations.Select(r => BuildResourceRegistration(r))];
        return Ok(resources);
    }

    /// <summary>
    /// Gets a registration by ID.
    /// </summary>
    /// <param name="id">The registration ID.</param>
    /// <returns>The registration with hypermedia links.</returns>
    /// <response code="200">Returns the registration.</response>
    /// <response code="404">If the registration is not found.</response>
    [HttpGet("{id}", Name = "GetRegistrationById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResourceRegistration>> GetRegistration(int id)
    {
        Registration? registration = await _context.Registration.FindAsync(id);
        if (registration == null)
            return NotFound();

        ResourceRegistration resource = BuildResourceRegistration(registration);
        return Ok(resource);
    }

    /// <summary>
    /// Gets a registration by tournament name and player gamertag.
    /// </summary>
    /// <param name="tournamentName">The name of the tournament.</param>
    /// <param name="gamertag">The player's gamertag.</param>
    /// <returns>The registration with hypermedia links.</returns>
    /// <response code="200">Returns the registration.</response>
    /// <response code="404">If the registration is not found.</response>
    [HttpGet("/api/tournaments/{tournamentName}/players/{gamertag}", Name = "GetRegistrationByTournamentAndPlayer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResourceRegistration>> GetRegistrationByTournamentAndPlayer(string tournamentName, string gamertag)
    {
        Registration? registration = await _context.Registration
                                         .Include(r => r.Player)      // Include related data (optional for validation)
                                         .Include(r => r.Tournament)
                                         .FirstOrDefaultAsync(r => r.TournamentName == tournamentName && r.PlayerGamertag == gamertag);
        if (registration == null)
            return NotFound();

        ResourceRegistration resource = BuildResourceRegistration(registration);
        return Ok(resource);
    }

    /// <summary>
    /// Gets all players registered in a tournament.
    /// </summary>
    /// <param name="tournamentName">The name of the tournament.</param>
    /// <returns>A list of players with hypermedia links.</returns>
    /// <response code="200">Returns the list of players.</response>
    /// <response code="404">If the tournament is not found.</response>
    [HttpGet("/api/tournaments/{tournamentName}/players", Name = "GetPlayersInTournament")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<ResourcePlayer>>> GetPlayersInTournament(string tournamentName)
    {
        // Verify tournament exists
        Tournament? tournament = await _context.Tournament.FirstOrDefaultAsync(t => t.Name == tournamentName);
        if (tournament == null)
            return NotFound($"Tournament '{tournamentName}' not found.");

        // Get all registrations for the specified tournament, including player data
        List<Registration> registrations = await _context.Registration
                                   .Include(r => r.Player)
                                   .Where(r => r.TournamentName == tournamentName)
                                   .ToListAsync();

        // If no players are registered, return an empty list (not 404)
        var result = new
        {
            tournament = new
            {
                name = tournamentName,
                parentTournamentName = tournament.ParentTournamentName
            },
            players = registrations.Select(r => new
            {
                player = r.Player,
                links = new List<Link> {
                            new(
                                href: $"/api/players/{r.PlayerGamertag}",
                                rel: "self",
                                method: HttpMethod.Get.Method
                            ),
                            new(
                                href: $"/api/players/{r.PlayerGamertag}",
                                rel: "update",
                                method: HttpMethod.Put.Method
                            ),
                            new(
                                href: $"/api/registrations/{tournamentName}/{r.PlayerGamertag}",
                                rel: "delete",
                                method: HttpMethod.Delete.Method
                            )
                        }
            }),
            links = new List<Link> {
                        new(
                            href: $"/api/tournaments/{tournamentName}",
                            rel: "self",
                            method: HttpMethod.Get.Method
                        ),
                        new(
                            href: $"/api/tournaments/{tournamentName}",
                            rel: "update",
                            method: HttpMethod.Put.Method
                        ),
                        new(
                            href: $"/api/tournaments/{tournamentName}",
                            rel: "delete",
                            method: HttpMethod.Delete.Method
                        ),
                        new(
                            href: $"/api/tournaments/{tournamentName}?include=sub-tournaments",
                            rel: "sub-tournaments",
                            method: HttpMethod.Get.Method
                        ),
                        new(
                            href: $"/api/tournaments/{tournamentName}/players/{{gamertag}}",
                            rel: "register-player",
                            method: HttpMethod.Post.Method
                        ),
                        new(
                            href: $"/api/tournaments/{tournamentName}/players",
                            rel: "registered-players",
                            method: HttpMethod.Get.Method
                        )
                    }
        };

        return Ok(result);
    }

    /// <summary>
    /// Updates a registration.
    /// </summary>
    /// <param name="id">The registration ID.</param>
    /// <param name="registration">The updated registration data.</param>
    /// <returns>No content.</returns>
    /// <response code="204">If the update is successful.</response>
    /// <response code="400">If the registration ID in the URL does not match the request body.</response>
    /// <response code="404">If the registration is not found.</response>
    [HttpPut("{id}", Name = "UpdateRegistration")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutRegistration(int id, Registration registration)
    {
        if (id != registration.Id)
            return BadRequest("Registration ID in URL must match request body.");

        _context.Entry(registration).State = EntityState.Modified;
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!RegistrationExists(id))
                return NotFound();
            throw;
        }

        return NoContent();
    }

    /// <summary>
    /// Deletes a registration by ID.
    /// </summary>
    /// <param name="id">The registration ID.</param>
    /// <returns>No content.</returns>
    /// <response code="204">If the deletion is successful.</response>
    /// <response code="404">If the registration is not found.</response>
    [HttpDelete("{id}", Name = "DeleteRegistrationById")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRegistration(int id)
    {
        Registration? registration = await _context.Registration.FindAsync(id);
        if (registration == null)
            return NotFound();

        _context.Registration.Remove(registration);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Deletes a registration by tournament name and player gamertag.
    /// </summary>
    /// <param name="tournamentName">The name of the tournament.</param>
    /// <param name="gamertag">The player's gamertag.</param>
    /// <returns>No content.</returns>
    /// <response code="204">If the deletion is successful.</response>
    /// <response code="404">If the registration is not found.</response>
    [HttpDelete("/api/registrations/{tournamentName}/{gamertag}", Name = "DeleteRegistrationByTournamentAndPlayer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRegistrationByTournamentAndPlayer(string tournamentName, string gamertag)
    {
        Registration? registration = await _context.Registration
                                         .FirstOrDefaultAsync(r => r.TournamentName == tournamentName && r.PlayerGamertag == gamertag);
        if (registration == null)
            return NotFound();

        // Also remove registrations in all sub-tournaments of the given tournament
        List<string> subTournaments = await GetAllSubTournamentNames(tournamentName);
        List<Registration> subRegistrations = await _context.Registration
                               .Where(r => subTournaments.Contains(r.TournamentName) && r.PlayerGamertag == gamertag)
                               .ToListAsync();
        _context.Registration.RemoveRange(subRegistrations);

        // Remove the primary registration
        _context.Registration.Remove(registration);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Gets all sub-tournament names under a given tournament (recursive).
    /// </summary>
    /// <param name="tournamentName">The name of the tournament.</param>
    /// <returns>A list of sub-tournament names.</returns>
    private async Task<List<string>> GetAllSubTournamentNames(string tournamentName)
    {
        List<string> result = [tournamentName];
        // Find direct sub-tournaments
        List<string> subs = await _context.Tournament
                                 .Where(t => t.ParentTournamentName == tournamentName)
                                 .Select(t => t.Name)
                                 .ToListAsync();
        foreach (string subName in subs)
        {
            result.AddRange(await GetAllSubTournamentNames(subName));
        }
        return result;
    }

    /// <summary>
    /// Checks if a registration with the specified ID exists.
    /// </summary>
    /// <param name="id">The registration ID.</param>
    /// <returns><see langword="true"/> <c>iff</c> a registration with the specified ID exists.</returns>
    private bool RegistrationExists(int id)
        => _context.Registration.Any(e => e.Id == id);

    /// <summary>
    /// Builds a <see cref="ResourceRegistration"/> with HATEOAS links.
    /// </summary>
    /// <param name="registration">The registration data.</param>
    /// <returns>A <see cref="ResourceRegistration"/> with HATEOAS links.</returns>
    private static ResourceRegistration BuildResourceRegistration(Registration registration)
    {
        ResourceRegistration resource = new ResourceRegistration(registration);

        // Self link - using relative path with lowercase routes
        resource.Links.Add(new Link(
            href: $"/api/tournaments/{registration.TournamentName}/players/{registration.PlayerGamertag}",
            rel: "self",
            method: HttpMethod.Get.Method
        ));

        // Delete link - using relative path with lowercase routes
        resource.Links.Add(new Link(
            href: $"/api/registrations/{registration.TournamentName}/{registration.PlayerGamertag}",
            rel: "delete",  // Changed from "delete-registration" to "delete"
            method: HttpMethod.Delete.Method
        ));

        // Link to the player - using relative path with lowercase routes
        resource.Links.Add(new Link(
            href: $"/api/players/{registration.PlayerGamertag}",
            rel: "player",
            method: HttpMethod.Get.Method
        ));

        // Link to the tournament - using relative path with lowercase routes
        resource.Links.Add(new Link(
            href: $"/api/tournaments/{registration.TournamentName}",
            rel: "tournament",
            method: HttpMethod.Get.Method
        ));

        return resource;
    }
}
