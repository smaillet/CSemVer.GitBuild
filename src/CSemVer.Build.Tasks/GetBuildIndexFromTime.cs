using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace CSemVer.Build.Tasks
{
    public class GetBuildIndexFromTime
        : Task
    {
        [Required]
        public DateTime TimeStamp { get; set; }

        [Output]
        public string BuildIndex { get; private set; }

        public override bool Execute( )
        {
            var timeStamp = TimeStamp.ToUniversalTime( );
            var midnightTodayUtc = new DateTime( timeStamp.Year, timeStamp.Month, timeStamp.Day, 0, 0, 0, DateTimeKind.Utc );
            var baseDate = new DateTime( 2000, 1, 1, 0, 0, 0, DateTimeKind.Utc );
            uint buildNumber = ( ( uint )( timeStamp - baseDate ).Days ) << 16;
            buildNumber += ( ushort )( ( timeStamp - midnightTodayUtc ).TotalSeconds / 2 );
            BuildIndex = buildNumber.ToString( );
            return true;
        }
    }
}
