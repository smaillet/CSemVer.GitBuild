using System;
using System.Diagnostics.CodeAnalysis;

namespace Ubiquity.NET.CSemVer
{
    public class ConstrainedSemanticVersion
    {
        public int Major { get; private set; }

        public int Minor { get; private set; }

        public int Patch { get; private set; }

        public PrereleaseVersion? PrereleaseVersion { get; private set; }

        public string BuildMetadata { get; private set; }

        public ConstrainedSemanticVersion( int major
                                         , int minor
                                         , int patch
                                         , PrereleaseVersion? preRelVer = null
                                         , string buildmeta = ""
                                         )
        {
            if( major < 0 || major > 99999 )
            {
                throw new ArgumentOutOfRangeException( nameof( major ) );
            }

            if( minor < 0 || minor > 49999 )
            {
                throw new ArgumentOutOfRangeException( nameof( minor ) );
            }

            if( patch < 0 || patch > 9999 )
            {
                throw new ArgumentOutOfRangeException( nameof( patch ) );
            }

            if( !string.IsNullOrWhiteSpace( buildmeta ) && buildmeta.Length > 20 )
            {
                throw new ArgumentException( "Build meta size must be less than or equal to 20 characters" );
            }

            Major = major;
            Minor = minor;
            Patch = patch;
            PrereleaseVersion = preRelVer;
            BuildMetadata = buildmeta;
        }

        public override string ToString( ) => ToString( true, false );

        public string ToString( bool includeMetadata, bool useShortNames )
        {
            var bldr = new System.Text.StringBuilder( );
            if( !useShortNames )
            {
                bldr.Append( 'v' );
            }

            bldr.AppendFormat( "{0}.{1}.{2}", Major, Minor, Patch );

            if( PrereleaseVersion != null )
            {
                bldr.Append( PrereleaseVersion.ToString( useShortNames ) );
            }

            if( !string.IsNullOrWhiteSpace(BuildMetadata) && includeMetadata )
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
                ulong retVal = ((ulong)Major * MulMajor) + ((ulong)Minor * MulMinor) + (((ulong)Patch + 1) * MulPatch);

                if( PrereleaseVersion != null && PrereleaseVersion.Index > 0 )
                {
                    retVal -= MulPatch - 1;
                    retVal += (ulong)PrereleaseVersion.Index * MulName;
                    retVal += (ulong)PrereleaseVersion.Number * MulNum;
                    retVal += (ulong)PrereleaseVersion.Fix;
                }

                return retVal;
            }
        }

        public static ConstrainedSemanticVersion CreateFrom( string buildVersionXmlPath, DateTime timeStamp, string ciBuildName, string buildMeta )
        {
            string ciBuildIndex = timeStamp.ToBuildIndex( );
            var parsedBuildVersionXml = new ParsedBuildVersionXml( buildVersionXmlPath );

            var preReleaseVersion = new PrereleaseVersion( parsedBuildVersionXml.PreReleaseName
                                                         , parsedBuildVersionXml.PreReleaseNumber
                                                         , parsedBuildVersionXml.PreReleaseFix
                                                         , ciBuildName
                                                         , ciBuildIndex
                                                         );

            return new ConstrainedSemanticVersion( parsedBuildVersionXml.BuildMajor
                                                 , parsedBuildVersionXml.BuildMinor
                                                 , parsedBuildVersionXml.BuildPatch
                                                 , preReleaseVersion
                                                 , buildMeta
                                                 );
        }

        private static Version MakeVersion( ulong value )
        {
            ushort revision = (ushort)(value % 65536);
            ulong rem = (value - revision) / 65536;

            ushort build = (ushort)(rem % 65536);
            rem = (rem - build) / 65536;

            ushort minor = (ushort)(rem % 65536);
            rem = (rem - minor) / 65536;

            ushort major = (ushort)(rem % 65536);

            return new Version( major, minor, build, revision );
        }

        private const ulong MulNum = 100;
        private const ulong MulName = MulNum * 100;
        private const ulong MulPatch = (MulName * 8) + 1;
        private const ulong MulMinor = MulPatch * 10000;
        private const ulong MulMajor = MulMinor * 50000;
    }
}
