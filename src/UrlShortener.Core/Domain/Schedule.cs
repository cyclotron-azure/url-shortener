using Cronos;

namespace UrlShortener.Core.Domain
{
    /// <summary>
    /// Represents a schedule for a URL shortener.
    /// </summary>
    public class Schedule
    {
        /// <summary>
        /// Gets or sets the start date and time of the schedule.
        /// </summary>
        public DateTime Start { get; set; } = DateTime.Now.AddMonths(-6);

        /// <summary>
        /// Gets or sets the end date and time of the schedule.
        /// </summary>
        public DateTime End { get; set; } = DateTime.Now.AddMonths(6);

        /// <summary>
        /// Gets or sets the alternative URL for the schedule.
        /// </summary>
        public string AlternativeUrl { get; set; } = "";

        /// <summary>
        /// Gets or sets the cron expression for the schedule.
        /// </summary>
        public string Cron { get; set; } = "* * * * *";

        /// <summary>
        /// Gets or sets the duration in minutes for the schedule.
        /// </summary>
        public int DurationMinutes { get; set; } = 0;

        /// <summary>
        /// Gets a displayable URL based on the maximum length specified.
        /// </summary>
        /// <param name="max">The maximum length of the displayable URL.</param>
        /// <returns>The displayable URL.</returns>
        public string GetDisplayableUrl(int max)
        {
            var length = AlternativeUrl.ToString().Length;
            if (length >= max)
            {
                return string.Concat(AlternativeUrl.Substring(0, max-1), "...");
            }
            return AlternativeUrl;
        }

        /// <summary>
        /// Checks if the schedule is active at the specified point in time.
        /// </summary>
        /// <param name="pointInTime">The point in time to check.</param>
        /// <returns>True if the schedule is active, false otherwise.</returns>
        public bool IsActive(DateTime pointInTime)
        {
            var bufferStart = pointInTime.AddMinutes(-DurationMinutes);
            var expires = pointInTime.AddMinutes(DurationMinutes);

            CronExpression expression = CronExpression.Parse(Cron);
            var occurences = expression.GetOccurrences(bufferStart, expires);

            foreach (DateTime d in occurences)
            {
                if (d < pointInTime && d < expires)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
