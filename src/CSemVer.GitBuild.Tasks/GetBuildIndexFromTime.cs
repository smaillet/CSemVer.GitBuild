using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace CSemVer.GitBuild
{
    public class GetBuildIndexFromTime
        : Task
    {
        [Required]
        public DateTime TimeStamp { get; set; }

        [Output]
        public string BuildIndex { get; private set; }

        // For details on the general algorithm used for computing the numbers here see:
        // https://msdn.microsoft.com/en-us/library/system.reflection.assemblyversionattribute.assemblyversionattribute(v=vs.140).aspx
        // Only difference is this uses UTC as the basis to ensure the numbers consistently increase independent of locale.
        public override bool Execute( )
        {
            var timeStamp = TimeStamp.ToUniversalTime( );
            var midnightTodayUtc = new DateTime( timeStamp.Year, timeStamp.Month, timeStamp.Day, 0, 0, 0, DateTimeKind.Utc );
            var baseDate = new DateTime( 2000, 1, 1, 0, 0, 0, DateTimeKind.Utc );
            uint buildNumber = ( ( uint )( timeStamp - baseDate ).Days ) << 16;
            buildNumber += ( ushort )( ( timeStamp - midnightTodayUtc ).TotalSeconds / 2 );
            BuildIndex = buildNumber.ToString( "X08" );
            return true;
        }
    }
}
