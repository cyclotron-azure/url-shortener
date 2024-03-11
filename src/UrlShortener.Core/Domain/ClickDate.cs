namespace UrlShortener.Core.Domain;

/// <summary>
/// The domain class for recording clicked events.
/// </summary>
public class ClickDate
{
    /// <summary>
    /// Gets or sets the date when the click occurred.
    /// </summary>
    public string? DateClicked { get; set; }

    /// <summary>
    /// Gets or sets the count of clicks that occurred on the specified date.
    /// </summary>
    public int Count { get; set; }
}