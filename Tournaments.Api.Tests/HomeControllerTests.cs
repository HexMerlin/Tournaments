using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Tournaments.Shared.Hateoas;

namespace Tournaments.Api.Tests;

/// <summary>
/// Contains tests for verifying the HomeController endpoints in the Tournament Management Application.
/// </summary>
[TestClass]
public class HomeControllerTests
{
    private static readonly TournamentsApiTestFixture _factory = new();
    private static HttpClient _client = null!;

    /// <summary>
    /// Initializes the test class by setting up the HTTP client.
    /// </summary>
    /// <param name="_">The test context, not used in this method.</param>
    [ClassInitialize]
    public static void Init(TestContext _)
    {
        _client = _factory.Client;
    }

    /// <summary>
    /// Verifies that the API root endpoint returns success with the expected links.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task GetApiRoot_ShouldReturnSuccessWithLinks()
    {
        // Arrange - no arrangement needed

        // Act
        var response = await _client.GetAsync("/api");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        // Get response JSON
        string jsonContent = await response.Content.ReadAsStringAsync();
        using var jsonDocument = JsonDocument.Parse(jsonContent);
        var root = jsonDocument.RootElement;

        // Verify links array exists
        Assert.IsTrue(root.TryGetProperty("links", out var linksElement),
            "Response should have a 'links' property");
        Assert.AreEqual(JsonValueKind.Array, linksElement.ValueKind);

        // Verify required links are present
        AssertHasApiRootLink(linksElement, "players", "/api/players", HttpMethod.Get.Method);
        AssertHasApiRootLink(linksElement, "create-player", "/api/players", HttpMethod.Post.Method);
        AssertHasApiRootLink(linksElement, "get-player", "/api/players/{gamertag}", HttpMethod.Get.Method);
        AssertHasApiRootLink(linksElement, "update-player", "/api/players/{gamertag}", HttpMethod.Put.Method);
        AssertHasApiRootLink(linksElement, "delete-player", "/api/players/{gamertag}", HttpMethod.Delete.Method);

        AssertHasApiRootLink(linksElement, "tournaments", "/api/tournaments", HttpMethod.Get.Method);
        AssertHasApiRootLink(linksElement, "create-tournament", "/api/tournaments", HttpMethod.Post.Method);
        AssertHasApiRootLink(linksElement, "get-tournament", "/api/tournaments/{name}", HttpMethod.Get.Method);
        AssertHasApiRootLink(linksElement, "update-tournament", "/api/tournaments/{name}", HttpMethod.Put.Method);
        AssertHasApiRootLink(linksElement, "delete-tournament", "/api/tournaments/{name}", HttpMethod.Delete.Method);

        AssertHasApiRootLink(linksElement, "registrations", "/api/registrations", HttpMethod.Get.Method);
        AssertHasApiRootLink(linksElement, "get-registration", "/api/registrations/{id}", HttpMethod.Get.Method);
        AssertHasApiRootLink(linksElement, "update-registration", "/api/registrations/{id}", HttpMethod.Put.Method);
        AssertHasApiRootLink(linksElement, "delete-registration", "/api/registrations/{id}", HttpMethod.Delete.Method);

        AssertHasApiRootLink(linksElement, "api-status", "/api/status", HttpMethod.Get.Method);
    }

    /// <summary>
    /// Verifies that the root endpoint redirects to the API root endpoint.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task GetRoot_ShouldRedirectToApiRoot()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        // Verify the final URL after the redirect
        var finalUrl = response.RequestMessage?.RequestUri?.ToString();
        Assert.IsTrue(finalUrl?.EndsWith("/api") ?? false, "The final URL should end with /api");
    }

    /// <summary>
    /// Verifies that the links in the API root endpoint have the correct format.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task GetApiRoot_LinksHaveCorrectFormat()
    {
        // Arrange - no arrangement needed

        // Act
        var response = await _client.GetAsync("/api");

        // Assert
        response.EnsureSuccessStatusCode();

        // Parse response
        string jsonContent = await response.Content.ReadAsStringAsync();
        using var jsonDocument = JsonDocument.Parse(jsonContent);
        var root = jsonDocument.RootElement;

        // Verify links structure
        Assert.IsTrue(root.TryGetProperty("links", out var linksElement));

        // Check that each link has the required properties
        foreach (var link in linksElement.EnumerateArray())
        {
            // A link must have href, rel, and method properties
            Assert.IsTrue(link.TryGetProperty("href", out var hrefElement),
                "Each link should have an 'href' property");
            Assert.IsTrue(link.TryGetProperty("rel", out var relElement),
                "Each link should have a 'rel' property");
            Assert.IsTrue(link.TryGetProperty("method", out var methodElement),
                "Each link should have a 'method' property");

            // href should be a non-empty string starting with "/"
            string href = hrefElement.GetString()!;
            Assert.IsFalse(string.IsNullOrEmpty(href), "href should not be empty");
            Assert.IsTrue(href.StartsWith("/"), "href should be a relative URL starting with '/'");

            // rel should be a non-empty string
            string rel = relElement.GetString()!;
            Assert.IsFalse(string.IsNullOrEmpty(rel), "rel should not be empty");

            // method should be a valid HTTP method
            string method = methodElement.GetString()!;
            Assert.IsTrue(new[] { HttpMethod.Get.Method, HttpMethod.Post.Method, HttpMethod.Put.Method, HttpMethod.Delete.Method, HttpMethod.Patch.Method }.Contains(method),
                $"'{method}' should be a valid HTTP method");
        }
    }

    /// <summary>
    /// Verifies that the API root endpoint is HATEOAS compliant.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task GetApiRoot_IsHateoasCompliant()
    {
        // Arrange - no arrangement needed

        // Act
        var response = await _client.GetAsync("/api");

        // Assert
        response.EnsureSuccessStatusCode();

        // Get MIME type
        Assert.IsTrue(response.Content.Headers.ContentType != null,
            "Response should include Content-Type header");
        Assert.IsTrue(response.Content.Headers.ContentType.MediaType == "application/json",
            "Response Content-Type should be application/json");

        // Parse response
        string jsonContent = await response.Content.ReadAsStringAsync();
        using var jsonDocument = JsonDocument.Parse(jsonContent);
        var root = jsonDocument.RootElement;

        // Verify links exist
        Assert.IsTrue(root.TryGetProperty("links", out _),
            "Response should have hypermedia links");

        // Make one of the link requests to verify navigation
        // Find the tournaments link
        string tournamentsUrl = FindLinkHref(root, "tournaments");
        Assert.IsFalse(string.IsNullOrEmpty(tournamentsUrl),
            "API root should contain a link to tournaments");

        // Follow the link
        var tournamentsResponse = await _client.GetAsync(tournamentsUrl);
        tournamentsResponse.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Asserts that the API root contains a link with the specified properties.
    /// </summary>
    /// <param name="linksElement">The JSON element containing the links.</param>
    /// <param name="rel">The relation of the link.</param>
    /// <param name="expectedHref">The expected href of the link.</param>
    /// <param name="expectedMethod">The expected method of the link.</param>
    private void AssertHasApiRootLink(JsonElement linksElement, string rel, string expectedHref, string expectedMethod)
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
        Assert.IsTrue(found, $"Missing link with rel='{rel}', href='{expectedHref}', method='{expectedMethod}'");
    }

    /// <summary>
    /// Finds the href of a link with the specified relation in the JSON element.
    /// </summary>
    /// <param name="root">The JSON element containing the links.</param>
    /// <param name="rel">The relation of the link.</param>
    /// <returns>The href of the link.</returns>
    private string FindLinkHref(JsonElement root, string rel)
    {
        if (root.TryGetProperty("links", out var linksElement))
        {
            foreach (var link in linksElement.EnumerateArray())
            {
                if (link.GetProperty("rel").GetString() == rel)
                {
                    return link.GetProperty("href").GetString() ?? string.Empty;
                }
            }
        }
        return string.Empty;
    }
}
