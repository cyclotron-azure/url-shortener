using UrlShortener.Core.Domain;
using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Core.Messages;

/// <summary>
/// Represents a request to create a short URL.
/// </summary>
public class ShortUrlRequest
{
    private string _vanity;

    /// <summary>
    /// Gets or sets the title of the short URL.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the vanity URL for the short URL.
    /// </summary>
    public string Vanity
    {
        get
        {
            return _vanity != null ? _vanity : string.Empty;
        }
        set
        {
            _vanity = value;
        }
    }

    /// <summary>
    /// Gets or sets the original URL to be shortened.
    /// </summary>
    [Required]
    public string Url { get; set; }

    private List<Schedule> _schedules;

    /// <summary>
    /// Gets or sets the schedules for the short URL.
    /// </summary>
    public List<Schedule> Schedules
    {
        get
        {
            if (_schedules == null)
            {
                _schedules = new List<Schedule>();
            }
            return _schedules;
        }
        set
        {
            _schedules = value;
        }
    }
}