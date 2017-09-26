using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace CSemVer.GitBuild
{
    [SuppressMessage( "", "SA1402", Justification = "MSBuild requires a single file for inline tasks" )]
    public class CreateVersionInfo
        : Task
    {

        [Required]
        public UInt16 BuildMajor { get; set; }

        [Required]
        public UInt16 BuildMinor { get; set; }

        [Required]
        public UInt16 BuildPatch { get; set; }

        [Required]
        public string BuildIndex { get; set; }

        public byte PreReleaseNameIndex { get; set; }

        public byte PreReleaseNumber { get; set; }

        public byte PreReleaseFix { get; set; }

        public string BuildMeta { get; set; }

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

        public override bool Execute( )
        {
            PrereleaseVersion preReleaseVersion = null;
            if( PreReleaseNameIndex > 0 )
            {
                preReleaseVersion = new PrereleaseVersion( PreReleaseNameIndex, PreReleaseNumber, PreReleaseFix );
            }

            var fullVersion = new CSemVer( BuildMajor, BuildMinor, BuildPatch, preReleaseVersion, BuildMeta );

            SemVer = fullVersion.ToString( true );
            NuGetVersion = fullVersion.ToString( false );
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
        public PrereleaseVersion( string preRelName, int preRelNumber = 0, int preRelFix = 0 )
            : this( GetPreReleaseIndex( preRelName ), preRelNumber, preRelFix )
        {
        }

        public PrereleaseVersion( int preRelNameIndex, int preRelNumber = 0, int preRelFix = 0 )
        {
            if( preRelNameIndex < 0 || preRelNameIndex > 7 )
            {
                throw new ArgumentOutOfRangeException( nameof( preRelNameIndex ), preRelNameIndex, "Expected value in range [0-7]" );
            }

            if( preRelNumber < 0 || preRelNumber > 99 )
            {
                throw new ArgumentOutOfRangeException( nameof( preRelNumber ), preRelNumber, "Expected value in range [0-99]" );
            }

            if( preRelFix < 0 || preRelFix > 99 )
            {
                throw new ArgumentOutOfRangeException( nameof( preRelFix ), preRelFix, "Expected value in range [0-99]" );
            }

            Version = (NameIndex: preRelNameIndex, Number: ( byte )preRelNumber, Fix: ( byte )preRelFix);
        }

        public int PreReleaseIndex { get; }

        public int PreReleaseNumber { get; }

        public int PreReleaseFix { get; }

        public string PreReleaseName => PreReleaseNames[ PreReleaseIndex ];

        public (int NameIndex, byte Number, byte Fix) Version { get; }

        public override string ToString( )
        {
            var bldr = new StringBuilder( "-" );
            bldr.Append( PreReleaseName );
            if( PreReleaseNumber > 0 )
            {
                bldr.AppendFormat( ".{0}", PreReleaseNumber );
                if( PreReleaseFix > 0 )
                {
                    bldr.AppendFormat( ".{0}", PreReleaseFix );
                }
            }

            return bldr.ToString( );
        }

        private static int GetPreReleaseIndex( [ValidatedNotNull] string preRelName )
        {
            if( string.IsNullOrWhiteSpace( preRelName ) )
            {
                throw new ArgumentException( "Prerelease name cannot be null or empty", nameof( preRelName ) );
            }

            var q = from name in PreReleaseNames.Select( ( n, i ) => (Name: n, Index: i) )
                    where 0 == string.Compare( name.Name, preRelName, StringComparison.OrdinalIgnoreCase )
                    select name;

            var nameIndex = q.FirstOrDefault( );
            return nameIndex.Name == null ? -1 : nameIndex.Index;
        }

        private static readonly string[] PreReleaseNames = { "alpha", "beta", "delta", "epsilon", "gamma", "kappa", "prerelease", "rc" };
    }

    [SuppressMessage( "", "SA1402", Justification = "MSBuild requires a single file for inline tasks" )]
    internal class CSemVer
    {
        public int Major { get; }

        public int Minor { get; }

        public int Patch { get; }

        public PrereleaseVersion PrereleaseVersion { get; }

        public string BuildMetadata { get; }

        public CSemVer( int major, int minor, int patch, PrereleaseVersion preRelVer = null, string buildmeta = null )
        {
            if( major < 0 || major > 99999 )
            {
                throw new ArgumentOutOfRangeException( nameof( major ) );
            }

            if( minor < 0 || minor < 49999 )
            {
                throw new ArgumentOutOfRangeException( nameof( minor ) );
            }

            if( patch < 0 || patch > 9999 )
            {
                throw new ArgumentOutOfRangeException( nameof( patch ) );
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

                ushort fileRevision = ( ushort )( orderedNum % 65536 );
                ulong rem = ( orderedNum - fileRevision ) / 65536;

                ushort fileBuild = ( ushort )( rem % 65536 );
                rem = ( rem - fileBuild ) / 65536;

                ushort fileMinor = ( ushort )( rem % 65536 );
                rem = ( rem - fileMinor ) / 65536;

                ushort fileMajor = ( ushort )( rem % 65536 );

                return new Version( fileMajor, fileMinor, fileBuild, fileRevision );
            }
        }

        public ulong OrderedVersion
        {
            get
            {
                ulong retVal = ( ( ulong )Major * MulMajor ) + ( ( ulong )Minor * MulMinor ) + ( ( ( ulong )Patch + 1 ) * MulPatch );

                if( PrereleaseVersion != null && PrereleaseVersion.Version.NameIndex > 0 )
                {
                    var preRelVer = PrereleaseVersion.Version;
                    retVal -= MulPatch - 1;
                    retVal += ( ulong )preRelVer.NameIndex * MulName;
                    retVal += preRelVer.Number * MulNum;
                    retVal += preRelVer.Fix;
                }

                return retVal;
            }
        }

        private const ulong MulNum  = 100;
        private const ulong MulName  = MulNum* 100;
        private const ulong MulPatch  = (MulName * 8) + 1;
        private const ulong MulMinor  = MulPatch * 10000;
        private const ulong MulMajor  = MulMinor * 50000;
    }

}
