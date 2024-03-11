using UrlShortener.Core.Domain;

namespace UrlShortener.Core.Messages;

/// <summary>
/// Represents a response containing a list of short URLs.
/// </summary>
public class ListResponse
{
    /// <summary>
    /// Gets or sets the list of short URLs.
    /// </summary>
    public List<ShortUrlEntity> UrlList { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ListResponse"/> class.
    /// </summary>
    public ListResponse() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ListResponse"/> class with the specified list of short URLs.
    /// </summary>
    /// <param name="list">The list of short URLs.</param>
    public ListResponse(List<ShortUrlEntity> list)
    {
        UrlList = list;
    }
}