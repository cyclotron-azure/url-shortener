using UrlShortener.Core.Domain;

namespace UrlShortener.Core.Messages;

/// <summary>
/// Represents a request to shorten a URL.
/// </summary>
public class ShortRequest
{
    /// <summary>
    /// Gets or sets the vanity URL.
    /// </summary>
    public string Vanity { get; set; }

    /// <summary>
    /// Gets or sets the original URL.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Gets or sets the title of the URL.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the schedules for the shortened URL.
    /// </summary>
    public Schedule[] Schedules { get; set; }
}