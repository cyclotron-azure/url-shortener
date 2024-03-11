using System.Collections.Generic;

namespace UrlShortener.Core.Domain;

/// <summary>
/// Represents a list of click statistics entities.
/// </summary>
public class ClickStatsEntityList
{
    /// <summary>
    /// Gets or sets the list of click statistics entities.
    /// </summary>
    public List<ClickStatsEntity> ClickStatsList { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ClickStatsEntityList"/> class.
    /// </summary>
    public ClickStatsEntityList() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClickStatsEntityList"/> class with the specified list of click statistics entities.
    /// </summary>
    /// <param name="list">The list of click statistics entities.</param>
    public ClickStatsEntityList(List<ClickStatsEntity> list)
    {
        ClickStatsList = list;
    }
}