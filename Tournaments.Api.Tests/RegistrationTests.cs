using System.Net;
using System.Net.Http.Json;
using Tournaments.Shared.Models;
using Tournaments.Shared;
using Tournaments.Shared.Hateoas;

namespace Tournaments.Api.Tests;

/// <summary>
/// Contains tests for verifying the registration endpoints in the Tournament Management Application.
/// </summary>
[TestClass]
public class RegistrationTests
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
    /// Verifies that registering a player in a tournament returns a 201 Created status.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [TestMethod]
    [DoNotParallelize]
    public async Task RegisterPlayerInTournament_ShouldReturn201()
    {
        // Arrange
        var (playerTag, tournamentName) = await SetUpPlayerAndTournament();

        // Act
        var response = await _client.PostAsync($"/api/tournaments/{tournamentName}/players/{playerTag}", null);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        var createdResource = await response.Content.ReadFromJsonAsync<ResourceRegistration>();
        Assert.IsNotNull(createdResource, "Response body should contain a ResourceRegistration");
        var registration = createdResource.Data;
        Assert.AreEqual(tournamentName, registration.TournamentName);
        Assert.AreEqual(playerTag, registration.PlayerGamertag);
        ResourceHelper.AssertRegistrationLinks(createdResource, tournamentName, playerTag);
    }

    /// <summary>
    /// Verifies that registering a player in a sub-tournament requires registration in the parent tournament.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [TestMethod]
    [DoNotParallelize]
    public async Task RegisterPlayerInSubTournament_ShouldRequireParentRegistration()
    {
        // Arrange
        var (playerTag, parentTournamentName) = await SetUpPlayerAndTournament();

        // Create a sub-tournament (child of parent)
        var subTournamentName = $"Challengers-Stage_{_testRunId}";
        await _client.PostAsJsonAsync("/api/tournaments", new
        {
            name = subTournamentName,
            parentTournamentName = parentTournamentName
        });

        // Act - try to register in sub-tournament without parent registration (should fail)
        var failedResponse = await _client.PostAsync($"/api/tournaments/{subTournamentName}/players/{playerTag}", null);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, failedResponse.StatusCode);

        // Now register in parent tournament
        var parentRegisterResponse = await _client.PostAsync($"/api/tournaments/{parentTournamentName}/players/{playerTag}", null);
        parentRegisterResponse.EnsureSuccessStatusCode();

        // Act again - register in sub-tournament after parent registration (should succeed)
        var successResponse = await _client.PostAsync($"/api/tournaments/{subTournamentName}/players/{playerTag}", null);

        // Assert success and verify returned registration
        Assert.AreEqual(HttpStatusCode.Created, successResponse.StatusCode);
        ResourceRegistration? subRegResource = await successResponse.Content.ReadFromJsonAsync<ResourceRegistration>();
        Assert.IsNotNull(subRegResource);
        Registration subRegistration = subRegResource.Data;
        Assert.AreEqual(subTournamentName, subRegistration.TournamentName);
        Assert.AreEqual(playerTag, subRegistration.PlayerGamertag);
        ResourceHelper.AssertRegistrationLinks(subRegResource, subTournamentName, playerTag);
    }

    /// <summary>
    /// Verifies that registering a duplicate player in a tournament returns a 409 Conflict status.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [TestMethod]
    [DoNotParallelize]
    public async Task RegisterDuplicatePlayerInTournament_ShouldReturn409Conflict()
    {
        // Arrange
        var (playerTag, tournamentName) = await SetUpPlayerAndTournament();

        // Register player once
        await _client.PostAsync($"/api/tournaments/{tournamentName}/players/{playerTag}", null);

        // Act - try to register the same player again in the same tournament
        var response = await _client.PostAsync($"/api/tournaments/{tournamentName}/players/{playerTag}", null);

        // Assert
        Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
    }

    /// <summary>
    /// Verifies that retrieving registration details returns the expected registration data.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task GetRegistrationDetails_ShouldReturnRegistration()
    {
        // Reset the database at the start of this test
        var resetResponse = await _client.PostAsync("/api/TestUtility/reset", null);
        resetResponse.EnsureSuccessStatusCode();

        // Create a unique player
        var playerGuid = Guid.NewGuid().ToString("N");
        var playerTag = $"HexMerlin_REG_{playerGuid}";
        await _client.PostAsJsonAsync("/api/players", new
        {
            gamertag = playerTag,
            name = "John Doe",
            age = 31
        });

        // Create a unique tournament
        var tournamentGuid = Guid.NewGuid().ToString("N");
        var tournamentName = $"PGL-Major_REG_{tournamentGuid}";
        await _client.PostAsJsonAsync("/api/tournaments", new
        {
            name = tournamentName
        });

        // Ensure the tournament exists before registering
        var tournamentCheck = await _client.GetAsync($"/api/tournaments/{tournamentName}");
        if (tournamentCheck.StatusCode != HttpStatusCode.OK)
        {
            Assert.Fail($"Tournament {tournamentName} was not found before registration. Status: {tournamentCheck.StatusCode}");
        }

        // Register the player in the tournament
        var registerResult = await _client.PostAsync($"/api/tournaments/{tournamentName}/players/{playerTag}", null);
        registerResult.EnsureSuccessStatusCode();

        // Wait briefly to ensure the registration is saved
        await Task.Delay(500);

        // Act - retrieve registration details
        var getResult = await _client.GetAsync($"/api/tournaments/{tournamentName}/players/{playerTag}");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, getResult.StatusCode);
        ResourceRegistration? resource = await getResult.Content.ReadFromJsonAsync<ResourceRegistration>();
        Assert.IsNotNull(resource);
        var registration = resource.Data;
        Assert.AreEqual(tournamentName, registration.TournamentName);
        Assert.AreEqual(playerTag, registration.PlayerGamertag);
        ResourceHelper.AssertRegistrationLinks(resource, tournamentName, playerTag);
    }

    /// <summary>
    /// Verifies that removing a player from a tournament returns a 204 No Content status.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [TestMethod]
    [DoNotParallelize]
    public async Task RemovePlayerFromTournament_ShouldReturn204NoContent()
    {
        // Arrange
        var (playerTag, tournamentName) = await SetUpPlayerAndTournament();
        // Register player in the tournament
        await _client.PostAsync($"/api/tournaments/{tournamentName}/players/{playerTag}", null);

        // Act
        var response = await _client.DeleteAsync($"/api/registrations/{tournamentName}/{playerTag}");

        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

        // Verify the player is no longer registered (GET should return 404)
        var checkResponse = await _client.GetAsync($"/api/tournaments/{tournamentName}/players/{playerTag}");
        Assert.AreEqual(HttpStatusCode.NotFound, checkResponse.StatusCode);
    }

    /// <summary>
    /// Verifies that removing a player from a tournament cascades to sub-tournaments.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [TestMethod]
    [DoNotParallelize]
    public async Task RemovePlayerFromTournament_ShouldCascadeToSubTournaments()
    {
        // Arrange
        var (playerTag, parentTournamentName) = await SetUpPlayerAndTournament();

        // Create sub-tournament and nested sub-tournament
        var subTournamentName = $"Challengers-Stage_{_testRunId}";
        await _client.PostAsJsonAsync("/api/tournaments", new
        {
            name = subTournamentName,
            parentTournamentName = parentTournamentName
        });
        var nestedSubName = $"Group-Stage_{_testRunId}";
        await _client.PostAsJsonAsync("/api/tournaments", new
        {
            name = nestedSubName,
            parentTournamentName = subTournamentName
        });

        // Register player in parent and both sub-tournaments
        await _client.PostAsync($"/api/tournaments/{parentTournamentName}/players/{playerTag}", null);
        await _client.PostAsync($"/api/tournaments/{subTournamentName}/players/{playerTag}", null);
        await _client.PostAsync($"/api/tournaments/{nestedSubName}/players/{playerTag}", null);

        // Act - remove from the middle sub-tournament
        var response = await _client.DeleteAsync($"/api/registrations/{subTournamentName}/{playerTag}");

        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

        // Verify the player is removed from the sub-tournament
        var subCheck = await _client.GetAsync($"/api/tournaments/{subTournamentName}/players/{playerTag}");
        Assert.AreEqual(HttpStatusCode.NotFound, subCheck.StatusCode);

        // Verify the player is removed from the nested sub-tournament
        var nestedCheck = await _client.GetAsync($"/api/tournaments/{nestedSubName}/players/{playerTag}");
        Assert.AreEqual(HttpStatusCode.NotFound, nestedCheck.StatusCode);

        // Verify the player is still registered in the parent tournament
        var parentCheck = await _client.GetAsync($"/api/tournaments/{parentTournamentName}/players/{playerTag}");
        Assert.AreEqual(HttpStatusCode.OK, parentCheck.StatusCode);
    }

    /// <summary>
    /// Verifies that getting registration details for a non-existent player returns a 404 Not Found status.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [TestMethod]
    [DoNotParallelize]
    public async Task GetRegistrationForNonExistentPlayer_ShouldReturn404NotFound()
    {
        // Arrange
        var tournamentName = $"PGL-Major_{_testRunId}";
        await _client.PostAsJsonAsync("/api/tournaments", new { name = tournamentName });

        // Act
        var response = await _client.GetAsync($"/api/tournaments/{tournamentName}/players/NonExistentPlayer");

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// Verifies that getting registration details for a non-existent tournament returns a 404 Not Found status.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [TestMethod]
    [DoNotParallelize]
    public async Task GetRegistrationForNonExistentTournament_ShouldReturn404NotFound()
    {
        // Arrange
        var playerTag = $"HexMerlin_{_testRunId}";
        await _client.PostAsJsonAsync("/api/players", new
        {
            gamertag = playerTag,
            name = "John Doe",
            age = 31
        });

        // Act
        var response = await _client.GetAsync($"/api/tournaments/NonExistentTournament/players/{playerTag}");

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}
