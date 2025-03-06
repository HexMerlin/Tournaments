using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Tournaments.Shared.Models;
using Tournaments.Shared;
using Tournaments.Shared.Hateoas;

namespace Tournaments.Api.Tests;

/// <summary>
/// Contains tests for verifying the database state and operations in the Tournament Management Application.
/// </summary>
[TestClass]
public class DatabaseTests
{
    private static readonly TournamentsApiTestFixture _factory = new();
    private static HttpClient _client = null!;

    /// <summary>
    /// Initializes the test class.
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
    [TestInitialize]
    public void TestSetup()
    {
        _factory.ResetDatabase();
    }

    /// <summary>
    /// Verifies that the initial database status is empty.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task CheckInitialDatabaseStatus_ShouldBeEmpty()
    {
        // GET: api/TestUtility/status
        var response = await _client.GetAsync("/api/TestUtility/status");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(content);
        var json = jsonDoc.RootElement;

        // Verify database is empty
        Assert.IsTrue(json.GetProperty("isEmpty").GetBoolean(), "Database should be empty initially");
        Assert.AreEqual(0, json.GetProperty("players").GetInt32());
        Assert.AreEqual(0, json.GetProperty("tournaments").GetInt32());
        Assert.AreEqual(0, json.GetProperty("registrations").GetInt32());
    }

    /// <summary>
    /// Verifies that resetting the database clears all data.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [TestMethod]
    [DoNotParallelize]
    public async Task ResetDatabase_ShouldClearAllData()
    {
        // First, create some data to ensure we have something to reset
        await _client.PostAsJsonAsync("/api/players", new
        {
            gamertag = "TestPlayer",
            name = "Test Player",
            age = 25
        });

        // Then reset the database
        var resetResponse = await _client.PostAsync("/api/TestUtility/reset", null);
        resetResponse.EnsureSuccessStatusCode();

        // Verify database is empty after reset
        var statusResponse = await _client.GetAsync("/api/TestUtility/status");
        statusResponse.EnsureSuccessStatusCode();

        var content = await statusResponse.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(content);
        var json = jsonDoc.RootElement;

        Assert.IsTrue(json.GetProperty("isEmpty").GetBoolean(), "Database should be empty after reset");
        Assert.AreEqual(0, json.GetProperty("players").GetInt32());
        Assert.AreEqual(0, json.GetProperty("tournaments").GetInt32());
        Assert.AreEqual(0, json.GetProperty("registrations").GetInt32());
    }

    /// <summary>
    /// Verifies that the database is empty after a reset operation.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDatabaseEmptyAfterReset()
    {
        // Create some data
        await _client.PostAsJsonAsync("/api/players", new
        {
            gamertag = "TestPlayer",
            name = "Test Player",
            age = 25
        });
        await _client.PostAsJsonAsync("/api/tournaments", new
        {
            name = "TestTournament"
        });

        // Reset the database
        await _client.PostAsync("/api/TestUtility/reset", null);

        // Verify players collection is empty
        var playersResponse = await _client.GetAsync("/api/players");
        playersResponse.EnsureSuccessStatusCode();
        var playersList = await playersResponse.Content.ReadFromJsonAsync<List<ResourcePlayer>>();
        Assert.IsNotNull(playersList);
        Assert.AreEqual(0, playersList.Count, "Players collection should be empty after reset");

        // Verify tournaments collection is empty
        var tournamentsResponse = await _client.GetAsync("/api/tournaments");
        tournamentsResponse.EnsureSuccessStatusCode();
        var tournamentsList = await tournamentsResponse.Content.ReadFromJsonAsync<List<ResourceTournament>>();
        Assert.IsNotNull(tournamentsList);
        Assert.AreEqual(0, tournamentsList.Count, "Tournaments collection should be empty after reset");
    }
}
