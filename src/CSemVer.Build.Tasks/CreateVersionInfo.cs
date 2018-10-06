using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace CSemVer.Build.Tasks
{
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
        public string SemVer { get; set; }

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
            PrereleaseVersion preReleaseVersion = null;
            if( !string.IsNullOrWhiteSpace( CiBuildName ) )
            {
                preReleaseVersion = new PrereleaseVersion( PreReleaseName
                                                         , string.IsNullOrWhiteSpace( PreReleaseNumber ) ? 0 : Convert.ToInt32( PreReleaseNumber )
                                                         , string.IsNullOrWhiteSpace( PreReleaseFix ) ? 0 : Convert.ToInt32( PreReleaseFix )
                                                         , CiBuildName
                                                         , CiBuildIndex
                                                         );
            }

            var fullVersion = new CSemVer( Convert.ToInt32( BuildMajor )
                                         , Convert.ToInt32( BuildMinor )
                                         , Convert.ToInt32( BuildPatch )
                                         , preReleaseVersion
                                         , BuildMeta
                                         );

            SemVer = fullVersion.ToString( );
            FileVersionMajor = ( ushort )fullVersion.FileVersion.Major;
            FileVersionMinor = ( ushort )fullVersion.FileVersion.Minor;
            FileVersionBuild = ( ushort )fullVersion.FileVersion.Build;
            FileVersionRevision = ( ushort )fullVersion.FileVersion.Revision;
            return true;
        }
    }

    /// <summary>Marker Attribute to inform CodeAnalysis that a parameter is validated as non-null in a method</summary>
    [SuppressMessage( "", "SA1402", Justification = "MSBuild requires a single file for inline tasks" )]
    [AttributeUsage( AttributeTargets.Parameter, Inherited = true, AllowMultiple = false )]
    internal sealed class ValidatedNotNullAttribute
        : Attribute
    {
    }

    [SuppressMessage( "", "SA1402", Justification = "MSBuild requires a single file for inline tasks" )]
    internal class PrereleaseVersion
    {
        public PrereleaseVersion( string preRelName, int preRelNumber, int preRelFix, string ciBuildName, string ciBuildIndex )
        {
            if( !string.IsNullOrWhiteSpace( preRelName ) )
            {
                var q = from name in PreReleaseNames.Select( ( n, i ) => new { Name = n, Index = i } )
                        where 0 == string.Compare( name.Name, preRelName, StringComparison.OrdinalIgnoreCase )
                        select name;

                var nameIndex = q.FirstOrDefault( );
                PreReleaseNameIndex = nameIndex.Name == null ? -1 : nameIndex.Index;

                if( PreReleaseNameIndex < 0 || PreReleaseNameIndex > 7 )
                {
                    throw new ArgumentException( "Expected value in range [0-7]", "preRelName" );
                }

                PreReleaseNumber = preRelNumber;
                PreReleaseFix = preRelFix;

                if( PreReleaseNumber < 0 || PreReleaseNumber > 99 )
                {
                    throw new ArgumentOutOfRangeException( "preRelNumber", PreReleaseNumber, "Expected value in range [0-99]" );
                }

                if( PreReleaseFix < 0 || PreReleaseFix > 99 )
                {
                    throw new ArgumentOutOfRangeException( "preRelFix", PreReleaseFix, "Expected value in range [0-99]" );
                }
            }
            else
            {
                PreReleaseNameIndex = -1;
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

        [SuppressMessage( "", "IDE0025", Justification = "Inline task compiler doesn't support expression bodies" )]
        public string PreReleaseName
        {
            get
            {
                return PreReleaseNameIndex >= 0 ? PreReleaseNames[ PreReleaseNameIndex ] : null;
            }
        }

        public int PreReleaseNameIndex { get; private set; }

        public int PreReleaseNumber { get; private set; }

        public int PreReleaseFix { get; private set; }

        public string CiBuildName { get; set; }

        public string CiBuildIndex { get; set; }

        public override string ToString( )
        {
            bool hasCIBuild = !string.IsNullOrEmpty( CiBuildName );
            bool hasPreRel = PreReleaseNameIndex >= 0;
            var bldr = new StringBuilder();

            if( hasPreRel )
            {
                bldr.Append( '-' );
                bldr.Append( PreReleaseName );

                if( PreReleaseNumber > 0 || hasCIBuild )
                {
                    bldr.AppendFormat( ".{0}", PreReleaseNumber );
                    if( PreReleaseFix > 0 || hasCIBuild )
                    {
                        bldr.AppendFormat( ".{0}", PreReleaseFix );
                    }
                }
            }

            if( hasCIBuild )
            {
                bldr.Append( hasPreRel ? "." : "--" );
                bldr.AppendFormat( "ci-{0}.{1}", CiBuildName, CiBuildIndex );
            }

            return bldr.ToString( );
        }

        private static readonly Regex CiBuildIdRegEx = new Regex(@"[a-zA-z0-9\-]+");
        private static readonly string[] PreReleaseNames = { "alpha", "beta", "delta", "epsilon", "gamma", "kappa", "prerelease", "rc" };
    }

    [SuppressMessage( "", "SA1402", Justification = "MSBuild requires a single file for inline tasks" )]
    internal class CSemVer
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

            if( buildmeta != null && buildmeta.Length > 20 )
            {
                throw new ArgumentException( "Build meta size must be less than 20 characters" );
            }

            Major = major;
            Minor = minor;
            Patch = patch;
            PrereleaseVersion = preRelVer;
            BuildMetadata = buildmeta;
        }

        public override string ToString( )
        {
            var bldr = new System.Text.StringBuilder( );
            bldr.AppendFormat( "{0}.{1}.{2}", Major, Minor, Patch );

            if( PrereleaseVersion != null )
            {
                bldr.Append( PrereleaseVersion.ToString( ) );
            }

            if( BuildMetadata != null )
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

                if( PrereleaseVersion != null && PrereleaseVersion.PreReleaseNameIndex > 0 )
                {
                    retVal -= MulPatch - 1;
                    retVal += ( ulong )PrereleaseVersion.PreReleaseNameIndex * MulName;
                    retVal += ( ulong )PrereleaseVersion.PreReleaseNumber * MulNum;
                    retVal += ( ulong )PrereleaseVersion.PreReleaseFix;
                }

                return retVal;
            }
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
