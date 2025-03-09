using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Tournaments.Shared.Models;

/// <summary>
/// Represents a player participating in tournaments.
/// </summary>
public class Player
{
    /// <summary>
    /// Gets or sets the unique gamertag of the player.
    /// </summary>
    [Key]
    public required string Gamertag { get; set; }

    /// <summary>
    /// Gets or sets the name of the player.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the age of the player.
    /// </summary>
    [Range(1, 200)]
    public int Age { get; set; } = 1;

    /// <summary>
    /// Gets or sets the registrations of the player.
    /// </summary>
    [JsonIgnore]
    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}
