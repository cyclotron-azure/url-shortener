namespace UrlShortener.Core.Domain;

/// <summary>
/// Represents a list of click dates for a specific URL.
/// </summary>
public class ClickDateList
{
    /// <summary>
    /// Gets or sets the list of click dates.
    /// </summary>
    public List<ClickDate> Items { get; set; } = [];

    /// <summary>
    /// Gets or sets the URL associated with the click dates.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClickDateList"/> class.
    /// </summary>
    public ClickDateList()
    {
        Url = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClickDateList"/> class with the specified list of click dates.
    /// </summary>
    /// <param name="list">The list of click dates.</param>
    public ClickDateList(List<ClickDate> list)
    {
        Items = list;
        Url = string.Empty;
    }
}