using Tournaments.Shared.Models;

namespace Tournaments.Shared.Hateoas;

/// <summary>
/// Represents a tournament resource wrapper with hypermedia links for HATEOAS.
/// </summary>
public class ResourceTournament : Resource<Tournament>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceTournament"/> class.
    /// </summary>
    public ResourceTournament() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceTournament"/> class with the specified tournament data.
    /// </summary>
    /// <param name="tournament">The tournament data.</param>
    public ResourceTournament(Tournament tournament) : base(tournament) { }
}

