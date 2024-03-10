using Microsoft.Azure.Cosmos.Table;

namespace UrlShortener.Core.Domain;

/// <summary>
/// Represents the next ID in the URL shortener system.
/// </summary>
public class NextId : TableEntity
{
    /// <summary>
    /// Gets or sets the ID value.
    /// </summary>
    public int Id { get; set; }
}