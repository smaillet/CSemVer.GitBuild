
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Ubiquity.NET.CSemVer;

namespace CSemVer.Build.Tasks
{
    public class ParseBuildVersionXml
        : Task
    {
        [Required]
        public string BuildVersionXml { get; set; }

        [Output]
        public string BuildMajor { get; private set; }

        [Output]
        public string BuildMinor { get; private set; }

        [Output]
        public string BuildPatch { get; private set; }

        [Output]
        public string PreReleaseName { get; private set; }

        [Output]
        public string PreReleaseNumber { get; private set; }

        [Output]
        public string PreReleaseFix { get; private set; }

        public override bool Execute( )
        {
            var parsedVersion = new ParsedBuildVersionXml(BuildVersionXml);
            BuildMajor = parsedVersion.BuildMajor.ToString( );
            BuildMinor = parsedVersion.BuildMinor.ToString( );
            BuildPatch = parsedVersion.BuildPatch.ToString( );
            PreReleaseName = parsedVersion.PreReleaseName;
            PreReleaseNumber = parsedVersion.PreReleaseNumber.ToString( );
            PreReleaseFix = parsedVersion.PreReleaseFix.ToString( );
            return true;
        }
    }
}
