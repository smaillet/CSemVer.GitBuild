
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Ubiquity.NET.Versioning.Build.Tasks
{
    public class ParseBuildVersionXml
        : Task
    {
        [Required]
        public string? BuildVersionXml { get; set; }

        [Output]
        public string? BuildMajor { get; private set; }

        [Output]
        public string? BuildMinor { get; private set; }

        [Output]
        public string? BuildPatch { get; private set; }

        [Output]
        public string? PreReleaseName { get; private set; }

        [Output]
        public string? PreReleaseNumber { get; private set; }

        [Output]
        public string? PreReleaseFix { get; private set; }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "External API invoker doesn't process exceptions")]
        public override bool Execute( )
        {
            try
            {
                if(string.IsNullOrWhiteSpace(BuildVersionXml))
                {
                    Log.LogError("PreReleaseName is required, and cannot be empty or all whitespace");
                    return false;
                }

                var parsedVersion = ParsedBuildVersionXml.ParseFile(BuildVersionXml!);
                BuildMajor = parsedVersion.BuildMajor.ToString(CultureInfo.InvariantCulture);
                BuildMinor = parsedVersion.BuildMinor.ToString(CultureInfo.InvariantCulture);
                BuildPatch = parsedVersion.BuildPatch.ToString(CultureInfo.InvariantCulture);
                PreReleaseName = parsedVersion.PreReleaseName;
                PreReleaseNumber = parsedVersion.PreReleaseNumber.ToString(CultureInfo.InvariantCulture);
                PreReleaseFix = parsedVersion.PreReleaseFix.ToString(CultureInfo.InvariantCulture);
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
