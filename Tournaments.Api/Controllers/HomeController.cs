using Microsoft.AspNetCore.Mvc;
using Tournaments.Shared.Hateoas;

namespace Tournaments.Api.Controllers;

/// <summary>
/// Provides API discovery endpoints for the Tournament Management Application.
/// </summary>
[ApiController]
public class HomeController : ControllerBase
{
    /// <summary>
    /// Entry point that provides links to all available resources and operations.
    /// </summary>
    /// <returns>A JSON response with links to all available resources and operations.</returns>
    /// <response code="200">Returns a JSON response with links to all available resources and operations.</response>
    [HttpGet("/api")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetApiRoot()
    {
        var links = new List<Link>
        {
            new Link("/api/players", "players", HttpMethod.Get.Method),
            new Link("/api/players", "create-player", HttpMethod.Post.Method),
            new Link("/api/players/{gamertag}", "get-player", HttpMethod.Get.Method),
            new Link("/api/players/{gamertag}", "update-player", HttpMethod.Put.Method),
            new Link("/api/players/{gamertag}", "delete-player", HttpMethod.Delete.Method),

            new Link("/api/tournaments", "tournaments", HttpMethod.Get.Method),
            new Link("/api/tournaments", "create-tournament", HttpMethod.Post.Method),
            new Link("/api/tournaments/{name}", "get-tournament", HttpMethod.Get.Method),
            new Link("/api/tournaments/{name}", "update-tournament", HttpMethod.Put.Method),
            new Link("/api/tournaments/{name}", "delete-tournament", HttpMethod.Delete.Method),

            new Link("/api/registrations", "registrations", HttpMethod.Get.Method),
            new Link("/api/registrations/{id}", "get-registration", HttpMethod.Get.Method),
            new Link("/api/registrations/{id}", "update-registration", HttpMethod.Put.Method),
            new Link("/api/registrations/{id}", "delete-registration", HttpMethod.Delete.Method),

            new Link("/api/status", "api-status", HttpMethod.Get.Method)
        };

        return Ok(new { links });
    }

    /// <summary>
    /// Redirects to the API discovery endpoint.
    /// </summary>
    /// <returns>A redirection to the API discovery endpoint.</returns>
    [HttpGet("/")]
    public ActionResult RedirectToApiRoot()
    {
        return RedirectToAction(nameof(GetApiRoot));
    }
}
