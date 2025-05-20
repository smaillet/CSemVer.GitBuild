using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Ubiquity.NET.Versioning.Build.Tasks
{
    public class CreateVersionInfo
        : Task
    {
        [Required]
        public string? BuildMajor { get; set; }

        [Required]
        public string? BuildMinor { get; set; }

        [Required]
        public string? BuildPatch { get; set; }

        public string? PreReleaseName { get; set; }

        public string? PreReleaseNumber { get; set; }

        public string? PreReleaseFix { get; set; }

        public string? CiBuildName { get; set; }

        public string? CiBuildIndex { get; set; }

        public string? BuildMeta { get; set; }

        [Output]
        public string? CSemVer { get; set; }

        [Output]
        public string? ShortCSemVer { get; set; }

        [Output]
        public ushort? FileVersionMajor { get; set; }

        [Output]
        public ushort? FileVersionMinor { get; set; }

        [Output]
        public ushort? FileVersionBuild { get; set; }

        [Output]
        public ushort? FileVersionRevision { get; set; }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "External API invoker doesn't process exceptions")]
        public override bool Execute( )
        {
            try
            {
                if(PreReleaseName is null)
                {
                    Log.LogError("PreReleaseName is required");
                }

                bool hasCiBuildIndex = CiBuildIndex is not null && string.IsNullOrWhiteSpace(CiBuildIndex);
                bool hasCiBuildName = CiBuildIndex is not null && string.IsNullOrWhiteSpace(CiBuildIndex);
                if (hasCiBuildIndex && !hasCiBuildName)
                {
                    Log.LogError("CiBuildName must be provided when CiBuildIndex is");
                }

                if(!hasCiBuildIndex && hasCiBuildName)
                {
                    Log.LogError("Both CiBuildIndex must be provided if CiBuildName is");
                }
                // having both or NOT having both is perfectly OK.

                // Stop if any errors detected in this task
                if(Log.HasLoggedErrors)
                {
                    return false;
                }

                var fmtProvider = CultureInfo.InvariantCulture;
                var ciInfo = new CiBuildInfo(CiBuildIndex ?? string.Empty, CiBuildName ??string.Empty);

                PrereleaseVersion preReleaseVersion = default;
                if(!string.IsNullOrWhiteSpace(PreReleaseName))
                {
                    preReleaseVersion = new PrereleaseVersion( PreReleaseName!
                                                             , string.IsNullOrWhiteSpace( PreReleaseNumber ) ? 0 : Convert.ToInt32( PreReleaseNumber, fmtProvider)
                                                             , string.IsNullOrWhiteSpace( PreReleaseFix ) ? 0 : Convert.ToInt32( PreReleaseFix, fmtProvider)
                                                             );
                }

                var fullVersion = new CSemVer( Convert.ToInt32( BuildMajor, fmtProvider)
                                             , Convert.ToInt32( BuildMinor, fmtProvider)
                                             , Convert.ToInt32( BuildPatch, fmtProvider)
                                             , preReleaseVersion
                                             , ciInfo
                                             , BuildMeta ?? string.Empty
                                             );

                CSemVer = fullVersion.ToString( );
                ShortCSemVer = fullVersion.ToString("MS", null);
                FileVersionMajor = fullVersion.FileVersion.Major;
                FileVersionMinor = fullVersion.FileVersion.Minor;
                FileVersionBuild = fullVersion.FileVersion.Build;
                FileVersionRevision = fullVersion.FileVersion.Revision;
                return true;
            }
            catch(Exception ex)
            {
                Log.LogErrorFromException(ex, showStackTrace: true);
                return false;
            }
        }
    }
}
