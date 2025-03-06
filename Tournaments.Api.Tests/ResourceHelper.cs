using System.Text.Json;
using Tournaments.Shared.Hateoas;

namespace Tournaments.Api.Tests;

/// <summary>
/// Provides helper methods for validating hypermedia links in resources.
/// </summary>
public static class ResourceHelper
{
    /// <summary>
    /// Validates that a resource contains the expected hypermedia links.
    /// </summary>
    /// <param name="links">The list of links to validate.</param>
    /// <param name="rel">The relation of the expected link.</param>
    /// <param name="expectedHref">The expected href of the link.</param>
    /// <param name="expectedMethod">The expected method of the link.</param>
    public static void AssertHasLink(List<Link> links, string rel, string expectedHref, string expectedMethod)
    {
        var link = links.SingleOrDefault(l => l.Rel == rel);
        Assert.IsNotNull(link, $"Expected link with rel '{rel}' was not found.");
        Assert.AreEqual(expectedHref, link!.Href, $"Href for '{rel}' link is incorrect.");
        Assert.AreEqual(expectedMethod, link.Method, $"Method for '{rel}' link is incorrect.");
    }

    /// <summary>
    /// Validates that a <see cref="ResourcePlayer"/> contains the expected hypermedia links.
    /// </summary>
    /// <param name="resource">The resource to validate.</param>
    /// <param name="expectedGamertag">The expected gamertag of the player.</param>
    public static void AssertPlayerLinks(ResourcePlayer resource, string expectedGamertag)
    {
        string basePath = $"/api/players/{expectedGamertag}";
        AssertHasLink(resource.Links, "self", basePath, HttpMethod.Get.Method);
        AssertHasLink(resource.Links, "update", basePath, HttpMethod.Put.Method);
        AssertHasLink(resource.Links, "delete", basePath, HttpMethod.Delete.Method);
    }

    /// <summary>
    /// Validates that a <see cref="ResourceTournament"/> contains the expected hypermedia links.
    /// </summary>
    /// <param name="resource">The resource to validate.</param>
    /// <param name="expectedTournamentName">The expected name of the tournament.</param>
    public static void AssertTournamentLinks(ResourceTournament resource, string expectedTournamentName)
    {
        string basePath = $"/api/tournaments/{expectedTournamentName}";
        AssertHasLink(resource.Links, "self", basePath, HttpMethod.Get.Method);
        AssertHasLink(resource.Links, "update", basePath, HttpMethod.Put.Method);
        AssertHasLink(resource.Links, "delete", basePath, HttpMethod.Delete.Method);
    }

    /// <summary>
    /// Validates that a <see cref="ResourceRegistration"/> contains the expected hypermedia links.
    /// </summary>
    /// <param name="resource">The resource to validate.</param>
    /// <param name="expectedTournamentName">The expected name of the tournament.</param>
    /// <param name="expectedPlayerTag">The expected gamertag of the player.</param>
    public static void AssertRegistrationLinks(ResourceRegistration resource, string expectedTournamentName, string expectedPlayerTag)
    {
        string selfPath = $"/api/tournaments/{expectedTournamentName}/players/{expectedPlayerTag}";
        string deletePath = $"/api/registrations/{expectedTournamentName}/{expectedPlayerTag}";
        AssertHasLink(resource.Links, "self", selfPath, HttpMethod.Get.Method);
        AssertHasLink(resource.Links, "delete", deletePath, HttpMethod.Delete.Method);
    }

    /// <summary>
    /// Validates that a JSON element contains the expected hypermedia link.
    /// </summary>
    /// <param name="linksElement">The JSON element containing the links.</param>
    /// <param name="rel">The relation of the expected link.</param>
    /// <param name="expectedHref">The expected href of the link.</param>
    /// <param name="expectedMethod">The expected method of the link.</param>
    public static void AssertJsonElementHasLink(JsonElement linksElement, string rel, string expectedHref, string expectedMethod)
    {
        bool found = false;
        foreach (var link in linksElement.EnumerateArray())
        {
            if (link.GetProperty("rel").GetString() == rel &&
                link.GetProperty("href").GetString() == expectedHref &&
                link.GetProperty("method").GetString() == expectedMethod)
            {
                found = true;
                break;
            }
        }
        Assert.IsTrue(found, $"Expected link with rel '{rel}', href '{expectedHref}', method '{expectedMethod}' was not found.");
    }
}
