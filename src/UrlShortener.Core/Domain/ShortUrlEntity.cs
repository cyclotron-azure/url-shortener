using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace UrlShortener.Core.Domain
{
    /// <summary>
    /// Represents a short URL entity.
    /// </summary>
    public class ShortUrlEntity : TableEntity
    {
        /// <summary>
        /// Gets or sets the original URL.
        /// </summary>
        public string Url { get; set; }

        private string _activeUrl { get; set; }

        /// <summary>
        /// Gets the active URL based on the schedules.
        /// </summary>
        public string ActiveUrl
        {
            get
            {
                if (String.IsNullOrEmpty(_activeUrl))
                    _activeUrl = GetActiveUrl();
                return _activeUrl;
            }
        }

        /// <summary>
        /// Gets or sets the title of the short URL.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the short URL.
        /// </summary>
        public string ShortUrl { get; set; }

        /// <summary>
        /// Gets or sets the number of clicks on the short URL.
        /// </summary>
        public int Clicks { get; set; }

        /// <summary>
        /// Gets or sets whether the short URL is archived.
        /// </summary>
        public bool? IsArchived { get; set; }

        /// <summary>
        /// Gets or sets the raw property for schedules.
        /// </summary>
        public string SchedulesPropertyRaw { get; set; }

        private List<Schedule> _schedules { get; set; }

        /// <summary>
        /// Gets or sets the schedules associated with the short URL.
        /// </summary>
        [IgnoreProperty]
        public List<Schedule> Schedules
        {
            get
            {
                if (_schedules == null)
                {
                    if (String.IsNullOrEmpty(SchedulesPropertyRaw))
                    {
                        _schedules = new List<Schedule>();
                    }
                    else
                    {
                        _schedules = JsonSerializer.Deserialize<Schedule[]>(SchedulesPropertyRaw).ToList<Schedule>();
                    }
                }
                return _schedules;
            }
            set
            {
                _schedules = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShortUrlEntity"/> class.
        /// </summary>
        public ShortUrlEntity() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShortUrlEntity"/> class with the specified long URL and end URL.
        /// </summary>
        /// <param name="longUrl">The long URL.</param>
        /// <param name="endUrl">The end URL.</param>
        public ShortUrlEntity(string longUrl, string endUrl)
        {
            Initialize(longUrl, endUrl, string.Empty, null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShortUrlEntity"/> class with the specified long URL, end URL, and schedules.
        /// </summary>
        /// <param name="longUrl">The long URL.</param>
        /// <param name="endUrl">The end URL.</param>
        /// <param name="schedules">The schedules.</param>
        public ShortUrlEntity(string longUrl, string endUrl, Schedule[] schedules)
        {
            Initialize(longUrl, endUrl, string.Empty, schedules);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShortUrlEntity"/> class with the specified long URL, end URL, title, and schedules.
        /// </summary>
        /// <param name="longUrl">The long URL.</param>
        /// <param name="endUrl">The end URL.</param>
        /// <param name="title">The title.</param>
        /// <param name="schedules">The schedules.</param>
        public ShortUrlEntity(string longUrl, string endUrl, string title, Schedule[] schedules)
        {
            Initialize(longUrl, endUrl, title, schedules);
        }

        private void Initialize(string longUrl, string endUrl, string title, Schedule[] schedules)
        {
            PartitionKey = endUrl.First().ToString();
            RowKey = endUrl;
            Url = longUrl;
            Title = title;
            Clicks = 0;
            IsArchived = false;

            if (schedules?.Length > 0)
            {
                Schedules = schedules.ToList<Schedule>();
                SchedulesPropertyRaw = JsonSerializer.Serialize<List<Schedule>>(Schedules);
            }
        }

        /// <summary>
        /// Gets a new instance of the <see cref="ShortUrlEntity"/> class with the specified long URL, end URL, title, and schedules.
        /// </summary>
        /// <param name="longUrl">The long URL.</param>
        /// <param name="endUrl">The end URL.</param>
        /// <param name="title">The title.</param>
        /// <param name="schedules">The schedules.</param>
        /// <returns>A new instance of the <see cref="ShortUrlEntity"/> class.</returns>
        public static ShortUrlEntity GetEntity(string longUrl, string endUrl, string title, Schedule[] schedules)
        {
            return new ShortUrlEntity
            {
                PartitionKey = endUrl.First().ToString(),
                RowKey = endUrl,
                Url = longUrl,
                Title = title,
                Schedules = schedules.ToList<Schedule>()
            };
        }

        private string GetActiveUrl()
        {
            if (Schedules != null)
                return GetActiveUrl(DateTime.UtcNow);
            return Url;
        }

        private string GetActiveUrl(DateTime pointInTime)
        {
            var link = Url;
            var active = Schedules.Where(s =>
                s.End > pointInTime && // hasn't ended
                s.Start < pointInTime // already started
            ).OrderBy(s => s.Start); // order by start to process first link

            foreach (var sched in active.ToArray())
            {
                if (sched.IsActive(pointInTime))
                {
                    link = sched.AlternativeUrl;
                    break;
                }
            }
            return link;
        }
    }
}