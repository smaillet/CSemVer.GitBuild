using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace CSemVer.Build.Tasks
{
    // The simplest way of selfhosting this code to create the version info for the package that contains it
    // is through a code task factory based on the source. Unfortunately, that means everything must be in a
    // single source file, The amount of code is small and the value of direct self-hosting is too great to
    // warrant establishing some alternate method of taging this package version.
    #region MSBuild tasks
    [SuppressMessage( "", "SA1402", Justification = "MSBuild requires a single file for inline tasks" )]
    public class GetBuildIndexFromTime
        : Task
    {
        [Required]
        public DateTime TimeStamp { get; set; }

        [Output]
        public string BuildIndex { get; private set; }

        public override bool Execute( )
        {
            BuildIndex = GetBuildIndex( TimeStamp );
            return true;
        }

        internal static string GetBuildIndex( DateTime timeStamp )
        {
            // establish an increasing build index based on the number of seconds from a common UTC date
            timeStamp = timeStamp.ToUniversalTime( );
            var midnightTodayUtc = new DateTime( timeStamp.Year, timeStamp.Month, timeStamp.Day, 0, 0, 0, DateTimeKind.Utc );
            var baseDate = new DateTime( 2000, 1, 1, 0, 0, 0, DateTimeKind.Utc );
            uint buildNumber = ( ( uint )( timeStamp - baseDate ).Days ) << 16;
            buildNumber += ( ushort )( ( timeStamp - midnightTodayUtc ).TotalSeconds / 2 );
            return buildNumber.ToString( );
        }
    }

    [SuppressMessage( "", "SA1402", Justification = "MSBuild requires a single file for inline tasks" )]
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

    [SuppressMessage( "", "SA1402", Justification = "MSBuild requires a single file for inline tasks" )]
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

            var fullVersion = new CSemVer( Convert.ToInt32( BuildMajor )
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
    #endregion

    [SuppressMessage( "", "SA1402", Justification = "MSBuild requires a single file for inline tasks" )]
    public class ParsedBuildVersionXml
    {
        public ParsedBuildVersionXml( string path )
        {
            using( var stream = File.OpenText( path ) )
            {
                var xdoc = System.Xml.Linq.XDocument.Load( stream, System.Xml.Linq.LoadOptions.None );
                var data = xdoc.Element( "BuildVersionData" );

                foreach( var attrib in data.Attributes( ) )
                {
                    switch( attrib.Name.LocalName )
                    {
                    case "BuildMajor":
                        BuildMajor = Convert.ToInt32( attrib.Value );
                        break;

                    case "BuildMinor":
                        BuildMinor = Convert.ToInt32( attrib.Value );
                        break;

                    case "BuildPatch":
                        BuildPatch = Convert.ToInt32( attrib.Value );
                        break;

                    case "PreReleaseName":
                        PreReleaseName = attrib.Value;
                        break;

                    case "PreReleaseNumber":
                        PreReleaseNumber = Convert.ToInt32( attrib.Value );
                        break;

                    case "PreReleaseFix":
                        PreReleaseFix = Convert.ToInt32( attrib.Value );
                        break;

                    default:
                        throw new InvalidDataException( $"Unexpected attribute {attrib.Name.LocalName}" );
                    }
                }

                // correct malformed values
                if( string.IsNullOrWhiteSpace( PreReleaseName ) )
                {
                    PreReleaseNumber = 0;
                    PreReleaseFix = 0;
                }
            }
        }
        public int BuildMajor { get; } = 0;

        public int BuildMinor { get; } = 0;

        public int BuildPatch { get; } = 0;

        public string PreReleaseName { get; } = "";

        public int PreReleaseNumber { get; } = 0;

        public int PreReleaseFix { get; } = 0;
    }

    [SuppressMessage( "", "SA1402", Justification = "MSBuild requires a single file for inline tasks" )]
    public class PrereleaseVersion
    {
        public PrereleaseVersion( string preRelName, int preRelNumber, int preRelFix, string ciBuildName, string ciBuildIndex )
        {
            if( !string.IsNullOrWhiteSpace( preRelName ) )
            {
                var q = from name in PreReleaseNames.Select( ( n, i ) => new { Name = n, Index = i } )
                        where 0 == string.Compare( name.Name, preRelName, StringComparison.OrdinalIgnoreCase )
                        select name;

                var nameIndex = q.FirstOrDefault( );
                Index = nameIndex.Name == null ? -1 : nameIndex.Index;

                if( Index < 0 || Index > 7 )
                {
                    throw new ArgumentException( "Expected value in range [0-7]", "preRelName" );
                }

                Number = preRelNumber;
                Fix = preRelFix;

                if( Number < 0 || Number > 99 )
                {
                    throw new ArgumentOutOfRangeException( "preRelNumber", Number, "Expected value in range [0-99]" );
                }

                if( Fix < 0 || Fix > 99 )
                {
                    throw new ArgumentOutOfRangeException( "preRelFix", Fix, "Expected value in range [0-99]" );
                }
            }
            else
            {
                Index = -1;
            }

            if( !string.IsNullOrEmpty( ciBuildName ) && !CiBuildIdRegEx.IsMatch( ciBuildName ) )
            {
                throw new ArgumentException( string.Format( "Invalid build name '{0}'", ciBuildName ), "ciBuildName" );
            }

            if( !string.IsNullOrEmpty( ciBuildIndex ) && !CiBuildIdRegEx.IsMatch( ciBuildIndex ) )
            {
                throw new ArgumentException( string.Format( "Invalid build index '{0}'", ciBuildIndex ), "ciBuildIndex" );
            }

            if( !string.IsNullOrEmpty( ciBuildName ) && string.IsNullOrEmpty( ciBuildIndex ) )
            {
                throw new ArgumentException( "CiBuildIndex is required if CiBuildName is provided" );
            }

            CiBuildName = ciBuildName;
            CiBuildIndex = ciBuildIndex;
        }

        public string Name => Index >= 0 ? PreReleaseNames[ Index ] : null;

        public string ShortName => Index >= 0 ? PreReleaseShortNames[ Index ] : null;

        public int Index { get; private set; }

        public int Number { get; private set; }

        public int Fix { get; private set; }

        public string CiBuildName { get; set; }

        public string CiBuildIndex { get; set; }

        public override string ToString( ) => ToString( false );

        [SuppressMessage( "Style", "IDE0058:Expression value is never used", Justification = "Fluent API returns self" )]
        public string ToString( bool useShortForm )
        {
            bool hasCIBuild = !string.IsNullOrEmpty( CiBuildName );
            bool hasPreRel = Index >= 0;
            var bldr = new StringBuilder();

            if( hasPreRel )
            {
                bldr.Append( '-' )
                    .Append( useShortForm ? ShortName : Name );

                string delimFormat = useShortForm ? "-{0:D02}" : ".{0}";
                if( Number > 0 || hasCIBuild )
                {
                    bldr.AppendFormat( delimFormat, Number );
                    if( Fix > 0 || hasCIBuild )
                    {
                        bldr.AppendFormat( delimFormat, Fix );
                    }
                }
            }

            if( hasCIBuild )
            {
                bldr.Append( hasPreRel ? "." : "--" );
                bldr.AppendFormat( "ci.{0}.{1}", CiBuildIndex, CiBuildName );
            }

            return bldr.ToString( );
        }

        private static readonly Regex CiBuildIdRegEx = new Regex(@"[a-zA-z0-9\-]+");
        private static readonly string[] PreReleaseNames = { "alpha", "beta", "delta", "epsilon", "gamma", "kappa", "prerelease", "rc" };
        private static readonly string[] PreReleaseShortNames = { "a", "b", "d", "e", "g", "k", "p", "r" };
    }

    [SuppressMessage( "", "SA1402", Justification = "MSBuild requires a single file for inline tasks" )]
    public class CSemVer
    {
        public int Major { get; private set; }

        public int Minor { get; private set; }

        public int Patch { get; private set; }

        public PrereleaseVersion PrereleaseVersion { get; private set; }

        public string BuildMetadata { get; private set; }

        public CSemVer( int major
                      , int minor
                      , int patch
                      , PrereleaseVersion preRelVer = null
                      , string buildmeta = null
                      )
        {
            if( major < 0 || major > 99999 )
            {
                throw new ArgumentOutOfRangeException( "major" );
            }

            if( minor < 0 || minor > 49999 )
            {
                throw new ArgumentOutOfRangeException( "minor" );
            }

            if( patch < 0 || patch > 9999 )
            {
                throw new ArgumentOutOfRangeException( "patch" );
            }

            if( !string.IsNullOrWhiteSpace( buildmeta ) && buildmeta.Length > 20 )
            {
                throw new ArgumentException( "Build meta size must be less than 20 characters" );
            }

            Major = major;
            Minor = minor;
            Patch = patch;
            PrereleaseVersion = preRelVer;
            BuildMetadata = buildmeta;
        }

        public override string ToString( ) => ToString( true, false );

        [SuppressMessage( "Style", "IDE0058:Expression value is never used", Justification = "Fluent API returns self" )]
        public string ToString( bool includeMetadata, bool useShortNames )
        {
            var bldr = new System.Text.StringBuilder( );
            if(!useShortNames)
            {
                bldr.Append( 'v' );
            }
            bldr.AppendFormat( "{0}.{1}.{2}", Major, Minor, Patch );

            if( PrereleaseVersion != null )
            {
                bldr.Append( PrereleaseVersion.ToString( useShortNames ) );
            }

            if( BuildMetadata != null && includeMetadata )
            {
                bldr.AppendFormat( "+{0}", BuildMetadata );
            }

            return bldr.ToString( );
        }

        public Version FileVersion
        {
            get
            {
                ulong orderedNum = OrderedVersion << 1;
                return MakeVersion( orderedNum );
            }
        }

        public ulong OrderedVersion
        {
            get
            {
                ulong retVal = ( ( ulong )Major * MulMajor ) + ( ( ulong )Minor * MulMinor ) + ( ( ( ulong )Patch + 1 ) * MulPatch );

                if( PrereleaseVersion != null && PrereleaseVersion.Index > 0 )
                {
                    retVal -= MulPatch - 1;
                    retVal += ( ulong )PrereleaseVersion.Index * MulName;
                    retVal += ( ulong )PrereleaseVersion.Number * MulNum;
                    retVal += ( ulong )PrereleaseVersion.Fix;
                }

                return retVal;
            }
        }

        public static CSemVer CreateFrom( string buildVersionXmlPath, DateTime timeStamp, string ciBuildName, string buildMeta )
        {
            string ciBuildIndex = GetBuildIndexFromTime.GetBuildIndex(timeStamp);
            var parsedBuildVersionXml = new ParsedBuildVersionXml( buildVersionXmlPath );

            var preReleaseVersion = new PrereleaseVersion( parsedBuildVersionXml.PreReleaseName
                                                         , parsedBuildVersionXml.PreReleaseNumber
                                                         , parsedBuildVersionXml.PreReleaseFix
                                                         , ciBuildName
                                                         , ciBuildIndex
                                                         );

            return new CSemVer( parsedBuildVersionXml.BuildMajor
                              , parsedBuildVersionXml.BuildMinor
                              , parsedBuildVersionXml.BuildPatch
                              , preReleaseVersion
                              , buildMeta
                              );
        }

        private static Version MakeVersion( ulong value )
        {
            ushort revision = ( ushort )( value % 65536 );
            ulong rem = ( value - revision ) / 65536;

            ushort build = ( ushort )( rem % 65536 );
            rem = ( rem - build ) / 65536;

            ushort minor = ( ushort )( rem % 65536 );
            rem = ( rem - minor ) / 65536;

            ushort major = ( ushort )( rem % 65536 );

            return new Version( major, minor, build, revision );
        }

        private const ulong MulNum  = 100;
        private const ulong MulName  = MulNum* 100;
        private const ulong MulPatch  = (MulName * 8) + 1;
        private const ulong MulMinor  = MulPatch * 10000;
        private const ulong MulMajor  = MulMinor * 50000;
    }
}
