using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Ubiquity.ArgValidators;

namespace CSemVer.GitBuild
{
    /// <summary>Build mode to use to determine the format of the version</summary>
    public enum BuildMode
    {
        /// <summary>Local Developer build</summary>
        /// <remarks>
        /// {BuildMajor}.{BuildMinor}.{BuildPatch}--ci-DEV-{UTCTIME of build in hex}
        /// </remarks>
        LocalDev,

        /// <summary>Automated validation build for a Pull Request</summary>
        /// <remarks>
        /// {BuildMajor}.{BuildMinor}.{BuildPatch}--ci-PRQ-{UTCTIME of PR Commit}+{COMMIT ID}
        /// <para>Pull request builds and artifacts are normally not published. A unique version
        /// pattern is used to ensure that the PR is built against any dependencies that share the
        /// same repository. (e.g. if project A depends on package B in the same repo then both
        /// are given a CSemVer with higher precedence than CI or official releases to ensure
        /// they are using what is built by the PR.</para>
        /// </remarks>
        PullRequest,

        /// <summary>Continuous integration build for a repository</summary>
        /// <remarks>
        /// {BuildMajor}.{BuildMinor}.{BuildPatch}{double dash}ci-REL-{UTCTIME of HEAD Commit}+{BuildMeta}
        /// <para> CI build versions are commonly released to the public via a dedicated gallery source for
        /// testing/early adopter feedback etc... Frequency of CI builds varies depending on project needs
        /// and can include a new build on every commit, rolling builds on a schedule, nightly builds, etc...
        /// </para>
        /// </remarks>
        ContinuousIntegration,

        /// <summary>Official release published build, may be a pre-release version but not a CI build</summary>
        /// <remarks>
        /// {BuildMajor}.{BuildMinor}.{BuildPatch}{single dash}{PreReleaseName}[.PreReleaseNumber][.PreReleaseFix]+{BuildMeta}
        /// or {BuildMajor}.{BuildMinor}.{BuildPatch}+{BuildMeta}
        /// </remarks>
        OfficialRelease
    }

    /// <summary>Version data for a build</summary>
    public class BuildVersionData
    {
        public UInt16 BuildMajor { get; private set; }

        public UInt16 BuildMinor { get; private set; }

        public UInt16 BuildPatch { get; private set; }

        public string PreReleaseName { get; private set; }

        public byte PreReleaseNumber { get; private set; }

        public byte PreReleaseFix { get; private set; }

        public string ReleaseBranch { get; private set; }

        public IReadOnlyDictionary<string, string> AdditionalProperties => ExtraPropertyMap;

        [CanBeNull]
        public string BuildVersionXmlFile { get; private set; }

        public CSemVer CreateSemVer( BuildMode buildMode, DateTime timeStamp, [CanBeNull] string buildmeta = null )
        {
            IPrereleaseVersion preReleaseInfo = null;
            switch( buildMode )
            {
            case BuildMode.LocalDev:
                // local dev builds are always newer than any other builds
                preReleaseInfo = new CIPreReleaseVersion( "DEV", GetBuildIndexFromUtc( timeStamp.ToUniversalTime() ) );
                break;

            case BuildMode.PullRequest:
                // PR builds should have a higher precedence than CI or release so that the
                // builds pull in the components built in previous stages of the current build
                // instead of the official CI or released builds.
                preReleaseInfo = new CIPreReleaseVersion( "PRQ", GetBuildIndexFromUtc( timeStamp.ToUniversalTime( ) ) );
                break;

            case BuildMode.ContinuousIntegration:
                preReleaseInfo = new CIPreReleaseVersion( "REL", GetBuildIndexFromUtc(timeStamp.ToUniversalTime() ) );
                break;

            case BuildMode.OfficialRelease:
                if( !string.IsNullOrWhiteSpace( PreReleaseName ) )
                {
                    preReleaseInfo = new OfficialPreRelease( PreReleaseName, PreReleaseNumber, PreReleaseFix );
                }

                break;

            default:
                throw new InvalidOperationException( "Unexpected/Unsupported repository state" );
            }

            return new CSemVer( BuildMajor, BuildMinor, BuildPatch, preReleaseInfo, buildmeta );
        }

        public static BuildVersionData Load( string path )
        {
            path.ValidateNotNullOrWhiteSpace( nameof( path ) );

            var retVal = new BuildVersionData( );
            using( var stream = File.OpenText( path ) )
            {
                var xdoc = System.Xml.Linq.XDocument.Load( stream, System.Xml.Linq.LoadOptions.None );
                var data = xdoc.Element( "BuildVersionData" );

                retVal.BuildVersionXmlFile = path;

                foreach( var attrib in data.Attributes() )
                {
                    switch( attrib.Name.LocalName )
                    {
                    case "BuildMajor":
                        retVal.BuildMajor = Convert.ToUInt16( attrib.Value );
                        break;

                    case "BuildMinor":
                        retVal.BuildMinor = Convert.ToUInt16( attrib.Value );
                        break;

                    case "BuildPatch":
                        retVal.BuildPatch = Convert.ToUInt16( attrib.Value );
                        break;

                    case "ReleaseBranch":
                        retVal.ReleaseBranch = attrib.Value;
                        break;

                    case "PreReleaseName":
                        retVal.PreReleaseName = attrib.Value;
                        break;

                    case "PreReleaseNumber":
                        retVal.PreReleaseNumber = Convert.ToByte( attrib.Value );
                        break;

                    case "PreReleaseFix":
                        retVal.PreReleaseFix = Convert.ToByte( attrib.Value );
                        break;

                    default:
                        retVal.ExtraPropertyMap.Add( attrib.Name.LocalName, attrib.Value );
                        break;
                    }
                }

                // correct malformed values
                if( string.IsNullOrWhiteSpace( retVal.PreReleaseName ) )
                {
                    retVal.PreReleaseNumber = 0;
                    retVal.PreReleaseFix = 0;
                }

                if( retVal.PreReleaseNumber == 0 )
                {
                    retVal.PreReleaseFix = 0;
                }
            }

            return retVal;
        }

        // For details on the general algorithm used for computing the numbers here see:
        // https://msdn.microsoft.com/en-us/library/system.reflection.assemblyversionattribute.assemblyversionattribute(v=vs.140).aspx
        // Only difference is this uses UTC as the basis to ensure the numbers consistently increase independent of locale.
        private static string GetBuildIndexFromUtc( DateTime timeStamp )
        {
            var midnightTodayUtc = new DateTime( timeStamp.Year, timeStamp.Month, timeStamp.Day, 0, 0, 0, DateTimeKind.Utc );
            var baseDate = new DateTime( 2000, 1, 1, 0, 0, 0, DateTimeKind.Utc );
            uint buildNumber = ( ( uint )( timeStamp - baseDate ).Days ) << 16;
            buildNumber += ( ushort )( ( timeStamp - midnightTodayUtc ).TotalSeconds / 2 );
            return buildNumber.ToString( "X08" );
        }

        private Dictionary<string,string> ExtraPropertyMap = new Dictionary<string, string>();
    }
}
