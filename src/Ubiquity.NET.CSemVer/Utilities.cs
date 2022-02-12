using System;

namespace Ubiquity.NET.CSemVer
{
    public static class Utilities
    {
        /// <summary>Gets a build index based on a time stamp</summary>
        /// <param name="timeStamp">Time stamp to use to create the build index</param>
        /// <returns>
        /// Build index as a string. The time stamp is converted to UTC (if not already in UTC form)
        /// so that the resulting index is consistent across builds on different machines/locales.
        /// </returns>
        public static string ToBuildIndex( this DateTime timeStamp )
        {
            // establish an increasing build index based on the number of seconds from a common UTC date
            timeStamp = timeStamp.ToUniversalTime( );
            var midnightTodayUtc = new DateTime( timeStamp.Year, timeStamp.Month, timeStamp.Day, 0, 0, 0, DateTimeKind.Utc );
            var baseDate = new DateTime( 2000, 1, 1, 0, 0, 0, DateTimeKind.Utc );
            uint buildNumber = ((uint)(timeStamp - baseDate).Days) << 16;
            buildNumber += (ushort)((timeStamp - midnightTodayUtc).TotalSeconds / 2);
            return buildNumber.ToString( );
        }
    }
}
