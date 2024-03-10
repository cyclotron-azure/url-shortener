using Microsoft.Azure.Cosmos.Table;
using System;

namespace UrlShortener.Core.Domain;

public class ClickStatsEntity : TableEntity
{
    /// <summary>
    /// Gets or sets the datetime of the click.
    /// </summary>
    public string Datetime { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClickStatsEntity"/> class.
    /// </summary>
    public ClickStatsEntity() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClickStatsEntity"/> class with the specified vanity.
    /// </summary>
    /// <param name="vanity">The vanity associated with the click.</param>
    public ClickStatsEntity(string vanity)
    {
        PartitionKey = vanity;
        RowKey = Guid.NewGuid().ToString();
        Datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
    }
}