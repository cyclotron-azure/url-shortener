namespace UrlShortener.Core.Messages;

/// <summary>
/// Represents a request to retrieve click statistics for a specific URL.
/// </summary>
public class UrlClickStatsRequest
{
    /// <summary>
    /// Gets or sets the vanity URL associated with the click statistics.
    /// </summary>
    public string Vanity { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="UrlClickStatsRequest"/> class with the specified vanity URL.
    /// </summary>
    /// <param name="vanity">The vanity URL associated with the click statistics.</param>
    public UrlClickStatsRequest(string vanity)
    {
        Vanity = vanity;
    }
}