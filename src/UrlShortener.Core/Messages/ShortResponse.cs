namespace UrlShortener.Core.Messages;

/// <summary>
/// Represents a response object containing information about a shortened URL.
/// </summary>
public class ShortResponse
{
    /// <summary>
    /// Gets or sets the shortened URL.
    /// </summary>
    public string? ShortUrl { get; set; }

    /// <summary>
    /// Gets or sets the original long URL.
    /// </summary>
    public string? LongUrl { get; set; }

    /// <summary>
    /// Gets or sets the title of the webpage associated with the URL.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShortResponse"/> class.
    /// </summary>
    public ShortResponse() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShortResponse"/> class with the specified parameters.
    /// </summary>
    /// <param name="host">The host URL.</param>
    /// <param name="longUrl">The original long URL.</param>
    /// <param name="endUrl">The end part of the shortened URL.</param>
    /// <param name="title">The title of the webpage associated with the URL.</param>
    public ShortResponse(string host, string longUrl, string endUrl, string title)
    {
        LongUrl = longUrl;
        ShortUrl = string.Concat(host, "/", endUrl);
        Title = title;
    }
}