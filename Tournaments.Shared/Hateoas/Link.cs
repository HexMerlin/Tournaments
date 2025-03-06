namespace Tournaments.Shared.Hateoas;

/// <summary>
/// Represents a hypermedia link used in HATEOAS.
/// </summary>
public class Link
{
    /// <summary>
    /// Gets or sets the URL of the link.
    /// </summary>
    public string Href { get; set; }

    /// <summary>
    /// Gets or sets the relationship of the link.
    /// </summary>
    public string Rel { get; set; }

    /// <summary>
    /// Gets or sets the HTTP method of the link.
    /// </summary>
    public string Method { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Link"/> class.
    /// </summary>
    /// <param name="href">The URL of the link.</param>
    /// <param name="rel">The relationship of the link.</param>
    /// <param name="method">The HTTP method of the link.</param>
    public Link(string href, string rel, string method)
    {
        Href = href;
        Rel = rel;
        Method = method;
    }
}

