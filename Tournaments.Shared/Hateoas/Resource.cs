namespace Tournaments.Shared.Hateoas;


/// <summary>
/// Represents a resource wrapper with hypermedia links for HATEOAS.
/// </summary>
/// <typeparam name="T">The type of the resource data.</typeparam>
public class Resource<T> where T : class
{
    /// <summary>
    /// Gets or sets the resource data.
    /// </summary>
    public T Data { get; set; }

    /// <summary>
    /// Gets or sets the list of hypermedia links.
    /// </summary>
    public List<Link> Links { get; set; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Resource{T}"/> class.
    /// </summary>
    public Resource() => Data = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="Resource{T}"/> class with the specified data.
    /// </summary>
    /// <param name="data">The resource data.</param>
    public Resource(T data) => Data = data;
}

