using Microsoft.EntityFrameworkCore;
using Tournaments.Shared.Models;

namespace Tournaments.Api.Data;

/// <summary>
/// Represents the database context for the Tournament Management Application.
/// </summary>
public class TournamentsApiContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TournamentsApiContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by the DbContext.</param>
    public TournamentsApiContext(DbContextOptions<TournamentsApiContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the DbSet of players.
    /// </summary>
    public DbSet<Player> Player { get; set; } = default!;

    /// <summary>
    /// Gets or sets the DbSet of tournaments.
    /// </summary>
    public DbSet<Tournament> Tournament { get; set; } = default!;

    /// <summary>
    /// Gets or sets the DbSet of registrations.
    /// </summary>
    public DbSet<Registration> Registration { get; set; } = default!;
}
