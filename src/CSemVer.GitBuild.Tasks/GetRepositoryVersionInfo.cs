using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace CSemVer.GitBuild
{
    public class GetRepositoryVersionInfo
        : Task
    {
        [Required]
        public string BuildVersionXmlFile { get; set; }

        [Required]
        public bool IsReleaseBuild { get; set; }

        [Required]
        public bool IsPullRequestBuild { get; set; }

        [Required]
        public bool IsAutomatedBuild { get; set; }

        [Required]
        public string BuildMeta { get; set; }

        [Required]
        public string BuildIndex { get; set; }

        [Output]
        public string SemVer { get; set; }

        [Output]
        public string NuGetVersion { get; set; }

        [Output]
        public ushort FileVersionMajor { get; set; }

        [Output]
        public ushort FileVersionMinor { get; set; }

        [Output]
        public ushort FileVersionBuild { get; set; }

        [Output]
        public ushort FileVersionRevision { get; set; }

        [Output]
        public ITaskItem[ ] ExtraProperties { get; set; }

        public override bool Execute( )
        {
            var buildMode = BuildMode.LocalDev;
            if( IsAutomatedBuild )
            {
                if( IsPullRequestBuild )
                {
                    buildMode = BuildMode.PullRequest;
                }
                else if( IsReleaseBuild )
                {
                    buildMode = BuildMode.OfficialRelease;
                }
                else
                {
                    buildMode = BuildMode.ContinuousIntegration;
                }
            }

            var baseBuildVersionData = BuildVersionData.Load( BuildVersionXmlFile );
            CSemVer fullVersion = baseBuildVersionData.CreateSemVer( buildMode, BuildIndex, BuildMeta );

            SemVer = fullVersion.ToString( true );
            NuGetVersion = fullVersion.ToString( false );
            FileVersionMajor = ( ushort )fullVersion.FileVersion.Major;
            FileVersionMinor = ( ushort )fullVersion.FileVersion.Minor;
            FileVersionBuild = ( ushort )fullVersion.FileVersion.Build;
            FileVersionRevision = ( ushort )fullVersion.FileVersion.Revision;

            ExtraProperties = ( from kvp in baseBuildVersionData.AdditionalProperties
                                select new TaskItem( "ExtraProperties", new Dictionary<string, string> { { "Name", kvp.Key }, { "Value", kvp.Value } } )
                              ).ToArray( );

            return true;
        }
    }
}
