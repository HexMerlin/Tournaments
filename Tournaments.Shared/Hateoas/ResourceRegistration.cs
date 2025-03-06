using Tournaments.Shared.Models;

namespace Tournaments.Shared.Hateoas;

/// <summary>
/// Represents a registration resource wrapper with hypermedia links for HATEOAS.
/// </summary>
public class ResourceRegistration : Resource<Registration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceRegistration"/> class.
    /// </summary>
    public ResourceRegistration() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceRegistration"/> class with the specified registration data.
    /// </summary>
    /// <param name="registration">The registration data.</param>
    public ResourceRegistration(Registration registration) : base(registration) { }
}

