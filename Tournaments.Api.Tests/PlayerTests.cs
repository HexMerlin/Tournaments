using System.Net;
using System.Net.Http.Json;
using Tournaments.Shared.Hateoas;

namespace Tournaments.Api.Tests;

/// <summary>
/// Contains tests for verifying the Player endpoints in the Tournament Management Application.
/// </summary>
[TestClass]
public class PlayerTests
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
    /// Verifies that creating a player returns a 201 Created status.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task CreatePlayer_ShouldReturn201()
    {
        // Arrange
        var uniqueTag = $"HexMerlin_{_testRunId}";
        var newPlayer = new
        {
            gamertag = uniqueTag,
            name = "John Doe",
            age = 31
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/players", newPlayer);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        ResourcePlayer? createdResource = await response.Content.ReadFromJsonAsync<ResourcePlayer>();
        Assert.IsNotNull(createdResource, "Response body should contain a ResourcePlayer");
        var createdPlayer = createdResource.Data;
        // Verify all expected properties in the returned ResourcePlayer
        Assert.AreEqual(newPlayer.gamertag, createdPlayer.Gamertag);
        Assert.AreEqual(newPlayer.name, createdPlayer.Name);
        Assert.AreEqual(newPlayer.age, createdPlayer.Age);
        // Verify hypermedia links (self, update, delete)
        ResourceHelper.AssertPlayerLinks(createdResource, uniqueTag);
    }

    /// <summary>
    /// Verifies that creating a duplicate player returns a 409 Conflict status.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [TestMethod]
    [DoNotParallelize]
    public async Task CreateDuplicatePlayer_ShouldReturn409Conflict()
    {
        // Arrange
        var uniqueTag = $"ShadowValkyrie_{_testRunId}";
        var player = new
        {
            gamertag = uniqueTag,
            name = "Emily Johnson",
            age = 25
        };

        // Create player first time
        var firstResponse = await _client.PostAsJsonAsync("/api/players", player);
        Assert.AreEqual(HttpStatusCode.Created, firstResponse.StatusCode);

        // Wait a moment to ensure first request is fully processed
        await Task.Delay(100);

        // Act - try to create the same player again
        var duplicateResponse = await _client.PostAsJsonAsync("/api/players", player);

        // Assert - should get a conflict response
        Assert.AreEqual(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
    }

    /// <summary>
    /// Verifies that retrieving a player returns the expected player data.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [TestMethod]
    [DoNotParallelize]
    public async Task GetPlayer_ShouldReturnPlayer()
    {
        // Arrange
        var uniqueTag = $"TheDude_{_testRunId}";
        var player = new
        {
            gamertag = uniqueTag,
            name = "Jeff Lebowski",
            age = 45
        };

        // Create player
        var createResponse = await _client.PostAsJsonAsync("/api/players", player);
        Assert.AreEqual(HttpStatusCode.Created, createResponse.StatusCode);

        // Act
        var response = await _client.GetAsync($"/api/players/{uniqueTag}");

        // Assert
        response.EnsureSuccessStatusCode();
        var retrievedResource = await response.Content.ReadFromJsonAsync<ResourcePlayer>();
        Assert.IsNotNull(retrievedResource, "Should return a ResourcePlayer");
        var retrievedPlayer = retrievedResource.Data;
        Assert.AreEqual(player.gamertag, retrievedPlayer.Gamertag);
        Assert.AreEqual(player.name, retrievedPlayer.Name);
        Assert.AreEqual(player.age, retrievedPlayer.Age);
        ResourceHelper.AssertPlayerLinks(retrievedResource, uniqueTag);
    }

    /// <summary>
    /// Verifies that updating a player modifies the player data.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task UpdatePlayer_ShouldModifyPlayer()
    {
        // Arrange
        var uniqueTag = $"HexMerlin_{_testRunId}";
        var player = new
        {
            gamertag = uniqueTag,
            name = "John Doe",
            age = 31
        };
        // Create player
        await _client.PostAsJsonAsync("/api/players", player);

        // Act - update the player
        var updatedPlayer = new
        {
            gamertag = uniqueTag, // same tag
            name = "John Updated",
            age = 32
        };
        var updateResponse = await _client.PutAsJsonAsync($"/api/players/{uniqueTag}", updatedPlayer);

        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, updateResponse.StatusCode);

        // Verify player was updated by retrieving it
        var getResponse = await _client.GetAsync($"/api/players/{uniqueTag}");
        getResponse.EnsureSuccessStatusCode();
        var retrievedResource = await getResponse.Content.ReadFromJsonAsync<ResourcePlayer>();
        Assert.IsNotNull(retrievedResource);
        var retrievedPlayer = retrievedResource.Data;
        Assert.AreEqual(updatedPlayer.name, retrievedPlayer.Name);
        Assert.AreEqual(updatedPlayer.age, retrievedPlayer.Age);
        ResourceHelper.AssertPlayerLinks(retrievedResource, uniqueTag);
    }

    /// <summary>
    /// Verifies that deleting a player removes the player data.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [TestMethod]
    [DoNotParallelize]
    public async Task DeletePlayer_ShouldRemovePlayer()
    {
        // Arrange
        var uniqueTag = $"ShadowValkyrie_{_testRunId}";
        var player = new
        {
            gamertag = uniqueTag,
            name = "Emily Johnson",
            age = 25
        };
        // Create player
        await _client.PostAsJsonAsync("/api/players", player);

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/players/{uniqueTag}");

        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify player was deleted (GET should return 404)
        var getResponse = await _client.GetAsync($"/api/players/{uniqueTag}");
        Assert.AreEqual(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
