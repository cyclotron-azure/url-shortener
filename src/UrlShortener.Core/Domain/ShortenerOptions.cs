namespace UrlShortener.Core.Domain;

/// <summary>
/// Represents the settings for the URL shortener.
/// </summary>
public class ShortenerOptions
{
    /// <summary>
    /// Gets or sets the default redirect URL.
    /// </summary>
    public string? DefaultRedirectUrl { get; set; }

    /// <summary>
    /// Gets or sets the custom domain for the shortened URLs.
    /// </summary>
    public string? CustomDomain { get; set; }

    /// <summary>
    /// Gets or sets the data storage mechanism for the shortened URLs.
    /// </summary>
    public string? DataStorage { get; set; }
}