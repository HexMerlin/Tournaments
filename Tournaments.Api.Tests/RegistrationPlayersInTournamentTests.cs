using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Tournaments.Api.Tests;

/// <summary>
/// Contains tests for verifying the registration of players in tournaments in the Tournament Management Application.
/// </summary>
[TestClass]
public class RegistrationPlayersInTournamentTests
{
    private static readonly TournamentsApiTestFixture _factory = new();
    private static HttpClient _client = null!;

    // Unique identifier to ensure each test run has distinct data
    private readonly string _testRunId = Guid.NewGuid().ToString("N");

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
    /// Sets up the test by resetting the database.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [TestInitialize]
    public async Task SetUp()
    {
        // Ensure database is reset before each test
        var response = await _client.PostAsync("/api/TestUtility/reset", null);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Sets up a player and a tournament for testing.
    /// </summary>
    /// <returns>A tuple containing the player tag and tournament name.</returns>
    private async Task<(string playerTag, string tournamentName)> SetUpPlayerAndTournament()
    {
        // Create a new player
        var playerTag = $"HexMerlin_{_testRunId}";
        await _client.PostAsJsonAsync("/api/players", new
        {
            gamertag = playerTag,
            name = "John Doe",
            age = 31
        });

        // Create a new tournament
        var tournamentName = $"PGL-Major_{_testRunId}";
        await _client.PostAsJsonAsync("/api/tournaments", new
        {
            name = tournamentName
        });

        return (playerTag, tournamentName);
    }

    /// <summary>
    /// Verifies that getting players in a tournament returns all registered players.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [TestMethod]
    [DoNotParallelize]
    public async Task GetPlayersInTournament_ShouldReturnAllPlayers()
    {
        // Arrange
        var tournamentName = $"PGL-Major_{_testRunId}";
        await _client.PostAsJsonAsync("/api/tournaments", new { name = tournamentName });

        // Create multiple players
        var player1Tag = $"HexMerlin_{_testRunId}";
        var player2Tag = $"ShadowValkyrie_{_testRunId}";

        await _client.PostAsJsonAsync("/api/players", new
        {
            gamertag = player1Tag,
            name = "John Doe",
            age = 31
        });

        await _client.PostAsJsonAsync("/api/players", new
        {
            gamertag = player2Tag,
            name = "Emily Johnson",
            age = 25
        });

        // Register both players in the tournament
        await _client.PostAsync($"/api/tournaments/{tournamentName}/players/{player1Tag}", null);
        await _client.PostAsync($"/api/tournaments/{tournamentName}/players/{player2Tag}", null);

        // Act
        var response = await _client.GetAsync($"/api/tournaments/{tournamentName}/players");

        // Assert
        response.EnsureSuccessStatusCode();

        // Parse response using JsonDocument
        string jsonContent = await response.Content.ReadAsStringAsync();
        using var jsonDocument = JsonDocument.Parse(jsonContent);
        var root = jsonDocument.RootElement;

        // Verify tournament info exists and has correct values
        Assert.IsTrue(root.TryGetProperty("tournament", out var tournamentElement),
            "Response should have a 'tournament' property");
        Assert.AreEqual(tournamentName, tournamentElement.GetProperty("name").GetString());

        // Verify players array exists and has two players
        Assert.IsTrue(root.TryGetProperty("players", out var playersElement),
            "Response should have a 'players' property");
        Assert.AreEqual(2, playersElement.GetArrayLength());

        // Check that both our players are in the response
        bool foundPlayer1 = false;
        bool foundPlayer2 = false;

        foreach (var playerEntry in playersElement.EnumerateArray())
        {
            var playerElement = playerEntry.GetProperty("player");
            var gamertag = playerElement.GetProperty("gamertag").GetString();

            if (gamertag == player1Tag)
                foundPlayer1 = true;
            else if (gamertag == player2Tag)
                foundPlayer2 = true;

            // Verify each player has proper HATEOAS links using ResourceHelper
            var linksElement = playerEntry.GetProperty("links");
            Assert.AreEqual(3, linksElement.GetArrayLength());

            // Use ResourceHelper to verify links
            ResourceHelper.AssertJsonElementHasLink(linksElement, "self", $"/api/players/{gamertag}", HttpMethod.Get.Method);
            ResourceHelper.AssertJsonElementHasLink(linksElement, "update", $"/api/players/{gamertag}", HttpMethod.Put.Method);
            ResourceHelper.AssertJsonElementHasLink(linksElement, "delete", $"/api/registrations/{tournamentName}/{gamertag}", HttpMethod.Delete.Method);
        }

        Assert.IsTrue(foundPlayer1, $"Player '{player1Tag}' not found in tournament players");
        Assert.IsTrue(foundPlayer2, $"Player '{player2Tag}' not found in tournament players");

        // Verify resource links
        Assert.IsTrue(root.TryGetProperty("links", out var resourceLinksElement),
            "Response should have a 'links' property");
        Assert.AreEqual(6, resourceLinksElement.GetArrayLength());

        // Use ResourceHelper to verify links
        ResourceHelper.AssertJsonElementHasLink(resourceLinksElement, "self", $"/api/tournaments/{tournamentName}", HttpMethod.Get.Method);
        ResourceHelper.AssertJsonElementHasLink(resourceLinksElement, "update", $"/api/tournaments/{tournamentName}", HttpMethod.Put.Method);
        ResourceHelper.AssertJsonElementHasLink(resourceLinksElement, "delete", $"/api/tournaments/{tournamentName}", HttpMethod.Delete.Method);
        ResourceHelper.AssertJsonElementHasLink(resourceLinksElement, "sub-tournaments",
            $"/api/tournaments/{tournamentName}?include=sub-tournaments", HttpMethod.Get.Method);
        ResourceHelper.AssertJsonElementHasLink(resourceLinksElement, "register-player",
            $"/api/tournaments/{tournamentName}/players/{{gamertag}}", HttpMethod.Post.Method);
        ResourceHelper.AssertJsonElementHasLink(resourceLinksElement, "registered-players",
            $"/api/tournaments/{tournamentName}/players", HttpMethod.Get.Method);
    }

    /// <summary>
    /// Verifies that getting players in an empty tournament returns an empty collection.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [TestMethod]
    [DoNotParallelize]
    public async Task GetPlayersInTournament_WithEmptyTournament_ShouldReturnEmptyCollection()
    {
        // Arrange
        var tournamentName = $"EmptyTournament_{_testRunId}";
        await _client.PostAsJsonAsync("/api/tournaments", new { name = tournamentName });

        // Act
        var response = await _client.GetAsync($"/api/tournaments/{tournamentName}/players");

        // Assert
        response.EnsureSuccessStatusCode();

        // Debug: Print the actual JSON response to see its structure
        string jsonContent = await response.Content.ReadAsStringAsync();

        // Use JsonDocument to navigate the JSON in a more flexible way
        using var jsonDocument = JsonDocument.Parse(jsonContent);
        var root = jsonDocument.RootElement;

        // Verify tournament info exists
        Assert.IsTrue(root.TryGetProperty("tournament", out var tournamentElement),
            "Response should have a 'tournament' property");
        Assert.AreEqual(tournamentName, tournamentElement.GetProperty("name").GetString());

        // Verify players array exists and is empty
        Assert.IsTrue(root.TryGetProperty("players", out var playersElement),
            "Response should have a 'players' property");
        Assert.AreEqual(0, playersElement.GetArrayLength());

        // Verify links exist
        Assert.IsTrue(root.TryGetProperty("links", out var linksElement),
            "Response should have a 'links' property");
        Assert.AreEqual(6, linksElement.GetArrayLength());

        // Use ResourceHelper to verify links
        ResourceHelper.AssertJsonElementHasLink(linksElement, "registered-players",
            $"/api/tournaments/{tournamentName}/players", HttpMethod.Get.Method);
    }

    /// <summary>
    /// Verifies that getting players in a nonexistent tournament returns a 404 Not Found status.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [TestMethod]
    [DoNotParallelize]
    public async Task GetPlayersInTournament_WithNonexistentTournament_ShouldReturn404()
    {
        // Act
        var response = await _client.GetAsync($"/api/tournaments/NonExistentTournament/players");

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// Verifies that getting players in a parent-child tournament structure returns the correct players.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [TestMethod]
    [DoNotParallelize]
    public async Task GetPlayersInTournament_WithParentChildTournaments_ShouldReturnCorrectPlayers()
    {
        // Arrange
        // Create parent tournament and child tournament
        var parentTournamentName = $"ParentTournament_{_testRunId}";
        var childTournamentName = $"ChildTournament_{_testRunId}";

        await _client.PostAsJsonAsync("/api/tournaments", new { name = parentTournamentName });
        await _client.PostAsJsonAsync("/api/tournaments", new
        {
            name = childTournamentName,
            parentTournamentName
        });

        // Create players
        var playerInParentOnlyTag = $"ParentPlayer_{_testRunId}";
        var playerInBothTag = $"BothPlayer_{_testRunId}";

        await _client.PostAsJsonAsync("/api/players", new
        {
            gamertag = playerInParentOnlyTag,
            name = "Parent Only Player",
            age = 30
        });

        await _client.PostAsJsonAsync("/api/players", new
        {
            gamertag = playerInBothTag,
            name = "Both Tournaments Player",
            age = 25
        });

        // Register players
        await _client.PostAsync($"/api/tournaments/{parentTournamentName}/players/{playerInParentOnlyTag}", null);
        await _client.PostAsync($"/api/tournaments/{parentTournamentName}/players/{playerInBothTag}", null);
        await _client.PostAsync($"/api/tournaments/{childTournamentName}/players/{playerInBothTag}", null);

        // Act - Get players from parent tournament
        var parentResponse = await _client.GetAsync($"/api/tournaments/{parentTournamentName}/players");

        // Assert for parent tournament
        parentResponse.EnsureSuccessStatusCode();
        string parentJsonContent = await parentResponse.Content.ReadAsStringAsync();
        using var parentJsonDocument = JsonDocument.Parse(parentJsonContent);
        var parentRoot = parentJsonDocument.RootElement;

        // Verify tournament info exists
        Assert.IsTrue(parentRoot.TryGetProperty("tournament", out var parentTournamentElement),
            "Parent response should have a 'tournament' property");
        Assert.AreEqual(parentTournamentName, parentTournamentElement.GetProperty("name").GetString());

        // Verify players array exists and has two players
        Assert.IsTrue(parentRoot.TryGetProperty("players", out var parentPlayersElement),
            "Parent response should have a 'players' property");
        Assert.AreEqual(2, parentPlayersElement.GetArrayLength(), "Parent tournament should have 2 players");

        // Verify both players exist in parent tournament
        var parentPlayerTags = new List<string>();
        foreach (var playerEntry in parentPlayersElement.EnumerateArray())
        {
            var player = playerEntry.GetProperty("player");
            string? gamertag = player.GetProperty("gamertag").GetString();
            Assert.IsNotNull(gamertag, "Found a player with null gamertag in tournament response");
            parentPlayerTags.Add(gamertag);
        }

        Assert.IsTrue(parentPlayerTags.Contains(playerInParentOnlyTag),
            $"Player '{playerInParentOnlyTag}' should be in parent tournament");
        Assert.IsTrue(parentPlayerTags.Contains(playerInBothTag),
            $"Player '{playerInBothTag}' should be in parent tournament");

        // Act - Get players from child tournament
        var childResponse = await _client.GetAsync($"/api/tournaments/{childTournamentName}/players");

        // Assert for child tournament
        childResponse.EnsureSuccessStatusCode();
        string childJsonContent = await childResponse.Content.ReadAsStringAsync();
        using var childJsonDocument = JsonDocument.Parse(childJsonContent);
        var childRoot = childJsonDocument.RootElement;

        // Verify tournament info exists
        Assert.IsTrue(childRoot.TryGetProperty("tournament", out var childTournamentElement),
            "Child response should have a 'tournament' property");
        Assert.AreEqual(childTournamentName, childTournamentElement.GetProperty("name").GetString());
        Assert.AreEqual(parentTournamentName, childTournamentElement.GetProperty("parentTournamentName").GetString());

        // Verify players array exists and has one player
        Assert.IsTrue(childRoot.TryGetProperty("players", out var childPlayersElement),
            "Child response should have a 'players' property");
        Assert.AreEqual(1, childPlayersElement.GetArrayLength(), "Child tournament should have 1 player");

        // Verify the correct player is in child tournament
        var childPlayerEntry = childPlayersElement[0];
        var childPlayer = childPlayerEntry.GetProperty("player");
        var childPlayerGamertag = childPlayer.GetProperty("gamertag").GetString();
        Assert.AreEqual(playerInBothTag, childPlayerGamertag,
            $"Child tournament should have player '{playerInBothTag}'");
    }

    /// <summary>
    /// Verifies that the links in the response for getting players in a tournament have the correct format.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [TestMethod]
    [DoNotParallelize]
    public async Task GetPlayersInTournament_ShouldHaveCorrectLinks()
    {
        // Arrange
        var (playerTag, tournamentName) = await SetUpPlayerAndTournament();
        await _client.PostAsync($"/api/tournaments/{tournamentName}/players/{playerTag}", null);

        // Act
        var response = await _client.GetAsync($"/api/tournaments/{tournamentName}/players");

        // Assert
        response.EnsureSuccessStatusCode();

        // Parse response using JsonDocument
        string jsonContent = await response.Content.ReadAsStringAsync();
        using var jsonDocument = JsonDocument.Parse(jsonContent);
        var root = jsonDocument.RootElement;

        // Verify links exist
        Assert.IsTrue(root.TryGetProperty("links", out var linksElement),
            "Response should have a 'links' property");
        Assert.AreEqual(6, linksElement.GetArrayLength());

        // Use ResourceHelper to verify all links
        ResourceHelper.AssertJsonElementHasLink(linksElement, "self", $"/api/tournaments/{tournamentName}", HttpMethod.Get.Method);
        ResourceHelper.AssertJsonElementHasLink(linksElement, "update", $"/api/tournaments/{tournamentName}", HttpMethod.Put.Method);
        ResourceHelper.AssertJsonElementHasLink(linksElement, "delete", $"/api/tournaments/{tournamentName}", HttpMethod.Delete.Method);
        ResourceHelper.AssertJsonElementHasLink(linksElement, "sub-tournaments",
            $"/api/tournaments/{tournamentName}?include=sub-tournaments", HttpMethod.Get.Method);
        ResourceHelper.AssertJsonElementHasLink(linksElement, "register-player",
            $"/api/tournaments/{tournamentName}/players/{{gamertag}}", HttpMethod.Post.Method);
        ResourceHelper.AssertJsonElementHasLink(linksElement, "registered-players",
            $"/api/tournaments/{tournamentName}/players", HttpMethod.Get.Method);
    }
}
