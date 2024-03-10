using UrlShortener.Core.Domain;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace UrlShortener.WebApi
{
    /// <summary>
    /// Provides utility methods for URL shortening.
    /// </summary>
    public static class Utility
    {
        //reshuffled for randomisation, same unique characters just jumbled up, you can replace with your own version
        private const string ConversionCode = "FjTG0s5dgWkbLf_8etOZqMzNhmp7u6lUJoXIDiQB9-wRxCKyrPcv4En3Y21aASHV";
        private static readonly int Base = ConversionCode.Length;
        //sets the length of the unique code to add to vanity
        private const int MinVanityCodeLength = 5;

        /// <summary>
        /// Gets a valid end URL based on the provided vanity code.
        /// </summary>
        /// <param name="vanity">The vanity code.</param>
        /// <param name="stgHelper">The storage table helper.</param>
        /// <returns>A valid end URL.</returns>
        public static async Task<string> GetValidEndUrl(string vanity, StorageTableHelper stgHelper)
        {
            if (string.IsNullOrEmpty(vanity))
            {
                var newKey = await stgHelper.GetNextTableId().ConfigureAwait(false);
                string getCode() => Encode(newKey);
                if (await stgHelper.IfShortUrlEntityExistByVanity(getCode()).ConfigureAwait(false))
                    return await GetValidEndUrl(vanity, stgHelper).ConfigureAwait(false);

                return string.Join(string.Empty, getCode());
            }
            else
            {
                return string.Join(string.Empty, vanity);
            }
        }

        /// <summary>
        /// Encodes an integer into a unique random token.
        /// </summary>
        /// <param name="i">The integer to encode.</param>
        /// <returns>A unique random token.</returns>
        public static string Encode(int i)
        {
            if (i == 0)
                return ConversionCode[0].ToString();

            return GenerateUniqueRandomToken(i);
        }

        /// <summary>
        /// Gets the short URL based on the host and vanity code.
        /// </summary>
        /// <param name="host">The host of the URL.</param>
        /// <param name="vanity">The vanity code.</param>
        /// <returns>The short URL.</returns>
        public static string GetShortUrl(string host, string vanity)
        {
            return host + "/" + vanity;
        }

        /// <summary>
        /// Generates a unique, random, and alphanumeric token for use as a URL.
        /// </summary>
        /// <param name="uniqueId">The unique identifier.</param>
        /// <returns>A unique, random, and alphanumeric token.</returns>
        public static string GenerateUniqueRandomToken(int uniqueId)
        {
            using (var generator = RandomNumberGenerator.Create())
            {
                //minimum size I would suggest is 5, longer the better but we want short URLs!
                var bytes = new byte[MinVanityCodeLength];
                generator.GetBytes(bytes);
                var chars = bytes
                    .Select(b => ConversionCode[b % ConversionCode.Length]);
                var token = new string(chars.ToArray());
                var reversedToken = string.Join(string.Empty, token.Reverse());
                return uniqueId + reversedToken;
            }
        }
    }
}