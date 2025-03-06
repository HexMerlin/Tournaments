using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;


namespace Tournaments.Shared.Models;
/// <summary>
/// Represents the registration of a player in a tournament.
/// </summary>
public class Registration
{
    /// <summary>
    /// Gets or sets the unique identifier for the registration.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the tournament.
    /// </summary>
    public required string TournamentName { get; set; }

    /// <summary>
    /// Gets or sets the gamertag of the player.
    /// </summary>
    public required string PlayerGamertag { get; set; }

    /// <summary>
    /// Gets or sets the tournament associated with the registration.
    /// </summary>
    [ForeignKey("TournamentName")]
    [JsonIgnore]
    public Tournament Tournament { get; set; } = null!;

    /// <summary>
    /// Gets or sets the player associated with the registration.
    /// </summary>
    [ForeignKey("PlayerGamertag")]
    [JsonIgnore]
    public Player Player { get; set; } = null!;
}
