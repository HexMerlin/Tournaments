using Tournaments.Shared.Models;

namespace Tournaments.Shared.Hateoas;

/// <summary>
/// Represents a player resource wrapper with hypermedia links for HATEOAS.
/// </summary>
public class ResourcePlayer : Resource<Player>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResourcePlayer"/> class.
    /// </summary>
    public ResourcePlayer() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourcePlayer"/> class with the specified player data.
    /// </summary>
    /// <param name="player">The player data.</param>
    public ResourcePlayer(Player player) : base(player) { }
}

