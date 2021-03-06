using System;
using System.Collections.Generic;
using System.Linq;

namespace Aegis.Model.Helpers
{
    public static class Extensions
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0);
        
        public static uint ToUnixTime(this DateTime dt)
        {
            TimeSpan span = dt - Epoch;
            return (uint) span.TotalSeconds;
        }

        public static DateTime DateFromUnixTime(this uint timestamp)
        {
            TimeSpan span = TimeSpan.FromSeconds(timestamp);
            return Epoch + span;
        }

        public static bool IsEmpty(this Guid guid) => guid == Guid.Empty;

        public static bool IsNotEmpty(this Guid guid) => guid != Guid.Empty;

        public static bool IsSpecified(this string s) => !string.IsNullOrEmpty(s);

        public static bool IsNullOrEmpty<T>(this ICollection<T> collection) =>
            collection == null || collection.Count == 0;

        public static bool EqualsNoCase(this string self, string other) =>
            string.Equals(self, other, StringComparison.InvariantCultureIgnoreCase);

        public static T AggregateOrDefault<T>(this IEnumerable<T> source, Func<T, T, T> aggregator)
        {
            if (!source.Any())
                return default;

            return source.Aggregate(aggregator);
        }
    }
}