using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace CSemVer.GitBuild
{
    /// <summary>Marker Attribute to inform CodeAnalysis that a parameter is validated as non-null in a method</summary>
    [SuppressMessage( "", "SA1402", Justification = "Too small to bother" )]
    [AttributeUsage( AttributeTargets.Parameter, Inherited = true, AllowMultiple = false )]
    public sealed class ValidatedNotNullAttribute
        : Attribute
    {
    }

    [SuppressMessage( "", "SA1402", Justification = "Too small to bother" )]
    internal class PrereleaseVersion
    {
        public PrereleaseVersion( string preRelName, int preRelNumber = 0, int preRelFix = 0 )
            : this( GetPreReleaseIndex( preRelName ), preRelNumber, preRelFix )
        {
        }

        public PrereleaseVersion( int preRelNameIndex, int preRelNumber = 0, int preRelFix = 0 )
        {
            if( preRelNameIndex < 0 || preRelNameIndex > 7)
            {
                throw new ArgumentOutOfRangeException(nameof(preRelNameIndex), preRelNameIndex, "Expected value in range [0-7]");
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
}
