using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Tournaments.Shared.Models;
/// <summary>
/// Represents a tournament in the Tournament Management Application.
/// </summary>
public class Tournament
{
    /// <summary>
    /// Gets or sets the unique name of the tournament.
    /// </summary>
    [Key]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the name of the parent tournament, if any.
    /// </summary>
    public string? ParentTournamentName { get; set; } = null;

    /// <summary>
    /// Gets or sets the parent tournament.
    /// </summary>
    [ForeignKey("ParentTournamentName")]
    [JsonIgnore]
    public Tournament? ParentTournament { get; set; } = null;

    /// <summary>
    /// Gets or sets the sub-tournaments of this tournament.
    /// </summary>
    public ICollection<Tournament> SubTournaments { get; set; } = new List<Tournament>();

    /// <summary>
    /// Gets or sets the registrations for this tournament.
    /// </summary>
    [JsonIgnore]
    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}
