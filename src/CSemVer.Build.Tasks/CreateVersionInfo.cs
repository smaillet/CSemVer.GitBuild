using System;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Ubiquity.NET.CSemVer;

namespace CSemVer.Build.Tasks
{
    public class CreateVersionInfo
        : Task
    {
        [Required]
        public string BuildMajor { get; set; }

        [Required]
        public string BuildMinor { get; set; }

        [Required]
        public string BuildPatch { get; set; }

        public string PreReleaseName { get; set; }

        public string PreReleaseNumber { get; set; }

        public string PreReleaseFix { get; set; }

        public string CiBuildName { get; set; }

        public string CiBuildIndex { get; set; }

        public string BuildMeta { get; set; }

        [Output]
        public string CSemVer { get; set; }

        [Output]
        public string ShortCSemVer { get; set; }

        [Output]
        public ushort FileVersionMajor { get; set; }

        [Output]
        public ushort FileVersionMinor { get; set; }

        [Output]
        public ushort FileVersionBuild { get; set; }

        [Output]
        public ushort FileVersionRevision { get; set; }

        public override bool Execute( )
        {
            var preReleaseVersion = new PrereleaseVersion( PreReleaseName
                                                         , string.IsNullOrWhiteSpace( PreReleaseNumber ) ? 0 : Convert.ToInt32( PreReleaseNumber )
                                                         , string.IsNullOrWhiteSpace( PreReleaseFix ) ? 0 : Convert.ToInt32( PreReleaseFix )
                                                         , CiBuildName
                                                         , CiBuildIndex
                                                         );

            var fullVersion = new ConstrainedSemanticVersion( Convert.ToInt32( BuildMajor )
                                                            , Convert.ToInt32( BuildMinor )
                                                            , Convert.ToInt32( BuildPatch )
                                                            , preReleaseVersion
                                                            , BuildMeta
                                                            );

            CSemVer = fullVersion.ToString( );
            ShortCSemVer = fullVersion.ToString( false, true );
            FileVersionMajor = ( ushort )fullVersion.FileVersion.Major;
            FileVersionMinor = ( ushort )fullVersion.FileVersion.Minor;
            FileVersionBuild = ( ushort )fullVersion.FileVersion.Build;
            FileVersionRevision = ( ushort )fullVersion.FileVersion.Revision;
            return true;
        }
    }
}
