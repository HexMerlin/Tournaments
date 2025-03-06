using System.Net;
using System.Net.Http.Json;
using Tournaments.Shared.Models;
using Tournaments.Shared;
using Tournaments.Shared.Hateoas;

namespace Tournaments.Api.Tests;

[TestClass]
public class TournamentTests
{
    private static readonly TournamentsApiTestFixture _factory = new();
    private static HttpClient _client = null!;

    // Unique identifier to ensure each test run has distinct data
    private readonly string _testRunId = Guid.NewGuid().ToString("N");

    [ClassInitialize]
    public static void Init(TestContext _)
    {
        _client = _factory.Client;
    }

    [TestInitialize]
    public async Task SetUp()
    {
        // Ensure database is reset before each test
        var response = await _client.PostAsync("/api/TestUtility/reset", null);
        response.EnsureSuccessStatusCode();
    }

    [TestMethod]
    public async Task CreateTournament_ShouldReturn201()
    {
        // Arrange
        var uniqueName = $"PGL-Major_{_testRunId}";
        var newTournament = new { name = uniqueName };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tournaments", newTournament);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        ResourceTournament? createdResource = await response.Content.ReadFromJsonAsync<ResourceTournament>();
        Assert.IsNotNull(createdResource);
        var createdTournament = createdResource.Data;
        Assert.AreEqual(newTournament.name, createdTournament.Name);
        Assert.IsNull(createdTournament.ParentTournamentName);
        ResourceHelper.AssertTournamentLinks(createdResource, uniqueName);
    }

    [TestMethod]
    public async Task CreateSubTournament_ShouldReturn201()
    {
        // Arrange - create parent tournament first
        var parentName = $"PGL-Major_{_testRunId}";
        await _client.PostAsJsonAsync("/api/tournaments", new { name = parentName });

        // Create sub-tournament data
        var subTournamentName = $"Challengers-Stage_{_testRunId}";
        var subTournament = new
        {
            name = subTournamentName,
            parentTournamentName = parentName
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tournaments", subTournament);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        ResourceTournament? createdResource = await response.Content.ReadFromJsonAsync<ResourceTournament>();
        Assert.IsNotNull(createdResource);
        var createdTournament = createdResource.Data;
        Assert.AreEqual(subTournament.name, createdTournament.Name);
        Assert.AreEqual(subTournament.parentTournamentName, createdTournament.ParentTournamentName);
        ResourceHelper.AssertTournamentLinks(createdResource, subTournamentName);
    }

    [TestMethod]
    public async Task CreateNestedSubTournament_ShouldReturn201()
    {
        // Arrange - create parent tournament and a sub-tournament
        var parentName = $"PGL-Major_{_testRunId}";
        await _client.PostAsJsonAsync("/api/tournaments", new { name = parentName });
        var subName = $"Challengers-Stage_{_testRunId}";
        await _client.PostAsJsonAsync("/api/tournaments", new
        {
            name = subName,
            parentTournamentName = parentName
        });

        // Create nested sub-tournament data
        var nestedSubName = $"Group-Stage_{_testRunId}";
        var nestedSub = new
        {
            name = nestedSubName,
            parentTournamentName = subName
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tournaments", nestedSub);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        ResourceTournament? createdResource = await response.Content.ReadFromJsonAsync<ResourceTournament>();
        Assert.IsNotNull(createdResource);
        Tournament createdTournament = createdResource.Data;
        Assert.AreEqual(nestedSub.name, createdTournament.Name);
        Assert.AreEqual(nestedSub.parentTournamentName, createdTournament.ParentTournamentName);
        ResourceHelper.AssertTournamentLinks(createdResource, nestedSubName);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CreateDuplicateTournament_ShouldReturn409Conflict()
    {
        // Arrange
        var uniqueName = $"ESL-Pro-League_{_testRunId}";
        var tournament = new { name = uniqueName };

        // Create tournament first time
        var firstResponse = await _client.PostAsJsonAsync("/api/tournaments", tournament);
        Assert.AreEqual(HttpStatusCode.Created, firstResponse.StatusCode);

        // Act - try to create the same tournament again
        var duplicateResponse = await _client.PostAsJsonAsync("/api/tournaments", tournament);

        // Assert
        Assert.AreEqual(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task GetTournament_ShouldReturnTournament()
    {
        // Arrange
        var uniqueName = $"BLAST-Premier_{_testRunId}";
        await _client.PostAsJsonAsync("/api/tournaments", new { name = uniqueName });

        // Act
        var response = await _client.GetAsync($"/api/tournaments/{uniqueName}");

        // Assert
        response.EnsureSuccessStatusCode();
        ResourceTournament? retrievedResource = await response.Content.ReadFromJsonAsync<ResourceTournament>();
        Assert.IsNotNull(retrievedResource);
        Tournament retrievedTournament = retrievedResource.Data;
        Assert.AreEqual(uniqueName, retrievedTournament.Name);
        Assert.IsNull(retrievedTournament.ParentTournamentName);
        ResourceHelper.AssertTournamentLinks(retrievedResource, uniqueName);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task GetTournamentWithSubTournaments_ShouldIncludeSubTournaments()
    {
        // Arrange - create parent tournament
        var parentName = $"PGL-Major_{_testRunId}";
        await _client.PostAsJsonAsync("/api/tournaments", new { name = parentName });

        // Create two sub-tournaments under the parent
        var subName1 = $"Challengers-Stage_{_testRunId}";
        var subName2 = $"Legends-Stage_{_testRunId}";
        await _client.PostAsJsonAsync("/api/tournaments", new
        {
            name = subName1,
            parentTournamentName = parentName
        });
        await _client.PostAsJsonAsync("/api/tournaments", new
        {
            name = subName2,
            parentTournamentName = parentName
        });

        // Optional: wait briefly to ensure data is saved
        await Task.Delay(100);

        // Act
        var response = await _client.GetAsync($"/api/tournaments/{parentName}?include=sub-tournaments");

        // Assert
        response.EnsureSuccessStatusCode();
        ResourceTournament? resourceTournament = await response.Content.ReadFromJsonAsync<ResourceTournament>();
        Assert.IsNotNull(resourceTournament);

        Tournament? tournament = resourceTournament.Data;
        Assert.IsNotNull(tournament);

        ICollection<Tournament>? subTournaments = tournament.SubTournaments;

        Assert.IsNotNull(subTournaments);
        Assert.AreEqual(2, subTournaments.Count);
        // Verify the included sub-tournaments
        Assert.IsTrue(subTournaments.Any(t => t.Name == subName1));
        Assert.IsTrue(subTournaments.Any(t => t.Name == subName2));
        Assert.IsTrue(subTournaments.All(t => t.ParentTournamentName == parentName));
        Assert.IsNull(tournament.ParentTournamentName);
        ResourceHelper.AssertTournamentLinks(resourceTournament, parentName);
    }

    [TestMethod]
    public async Task UpdateTournament_ShouldModifyTournament()
    {
        // Arrange
        var uniqueName = $"PGL-Major_{_testRunId}";
        await _client.PostAsJsonAsync("/api/tournaments", new { name = uniqueName });

        // Act - update the tournament's parent (none in this case)
        var updatedTournament = new
        {
            name = uniqueName,
            parentTournamentName = (string?)null
        };
        var updateResponse = await _client.PutAsJsonAsync($"/api/tournaments/{uniqueName}", updatedTournament);

        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, updateResponse.StatusCode);

        // Verify tournament was updated
        var getResponse = await _client.GetAsync($"/api/tournaments/{uniqueName}");
        getResponse.EnsureSuccessStatusCode();
        ResourceTournament? retrievedResource = await getResponse.Content.ReadFromJsonAsync<ResourceTournament>();
        Assert.IsNotNull(retrievedResource);
        Tournament retrievedTournament = retrievedResource.Data;
        Assert.AreEqual(updatedTournament.name, retrievedTournament.Name);
        Assert.AreEqual(updatedTournament.parentTournamentName, retrievedTournament.ParentTournamentName);
        ResourceHelper.AssertTournamentLinks(retrievedResource, uniqueName);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task DeleteTournament_ShouldRemoveTournament()
    {
        // Arrange
        var uniqueName = $"ESL-Pro-League_{_testRunId}";
        await _client.PostAsJsonAsync("/api/tournaments", new { name = uniqueName });

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/tournaments/{uniqueName}");

        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify the tournament was deleted
        var getResponse = await _client.GetAsync($"/api/tournaments/{uniqueName}");
        Assert.AreEqual(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task DeleteTournament_ShouldRemoveTournamentAndSubTournaments()
    {
        // Arrange - create a tournament hierarchy
        var parentName = $"PGL-Major_{_testRunId}";
        await _client.PostAsJsonAsync("/api/tournaments", new { name = parentName });
        var subName = $"Challengers-Stage_{_testRunId}";
        await _client.PostAsJsonAsync("/api/tournaments", new
        {
            name = subName,
            parentTournamentName = parentName
        });
        var nestedSubName = $"Group-Stage_{_testRunId}";
        await _client.PostAsJsonAsync("/api/tournaments", new
        {
            name = nestedSubName,
            parentTournamentName = subName
        });

        // Act - delete the parent tournament (should cascade delete all children)
        var deleteResponse = await _client.DeleteAsync($"/api/tournaments/{parentName}");

        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify parent tournament was deleted
        var getParentResponse = await _client.GetAsync($"/api/tournaments/{parentName}");
        Assert.AreEqual(HttpStatusCode.NotFound, getParentResponse.StatusCode);
        // Verify sub-tournament was deleted
        var getSubResponse = await _client.GetAsync($"/api/tournaments/{subName}");
        Assert.AreEqual(HttpStatusCode.NotFound, getSubResponse.StatusCode);
        // Verify nested sub-tournament was deleted
        var getNestedSubResponse = await _client.GetAsync($"/api/tournaments/{nestedSubName}");
        Assert.AreEqual(HttpStatusCode.NotFound, getNestedSubResponse.StatusCode);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CreateTournamentExceedingNestingLimit_ShouldReturn400BadRequest()
    {
        // Arrange - create a chain of 5 nested tournaments (maximum allowed depth)
        var tournamentNames = new string[5];
        tournamentNames[0] = $"Level1_{_testRunId}";
        await _client.PostAsJsonAsync("/api/tournaments", new { name = tournamentNames[0] });
        for (int i = 1; i < 5; i++)
        {
            tournamentNames[i] = $"Level{i + 1}_{_testRunId}";
            var response = await _client.PostAsJsonAsync("/api/tournaments", new
            {
                name = tournamentNames[i],
                parentTournamentName = tournamentNames[i - 1]
            });
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        // Act - try to create a 6th level tournament (should exceed depth limit)
        var level6Name = $"Level6_{_testRunId}";
        var level6Response = await _client.PostAsJsonAsync("/api/tournaments", new
        {
            name = level6Name,
            parentTournamentName = tournamentNames[4]
        });

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, level6Response.StatusCode);
    }
}
