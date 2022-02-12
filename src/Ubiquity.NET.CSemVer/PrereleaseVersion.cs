using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Ubiquity.NET.CSemVer
{
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
                    throw new ArgumentException( "Expected value in range [0-7]", nameof( preRelName ) );
                }

                Number = preRelNumber;
                Fix = preRelFix;

                if( Number < 0 || Number > 99 )
                {
                    throw new ArgumentOutOfRangeException( nameof( preRelNumber ), Number, "Expected value in range [0-99]" );
                }

                if( Fix < 0 || Fix > 99 )
                {
                    throw new ArgumentOutOfRangeException( nameof( preRelFix ), Fix, "Expected value in range [0-99]" );
                }
            }
            else
            {
                Index = -1;
            }

            if( !string.IsNullOrEmpty( ciBuildName ) && !CiBuildIdRegEx.IsMatch( ciBuildName ) )
            {
                throw new ArgumentException( string.Format( "Invalid build name '{0}'", ciBuildName ), nameof( ciBuildName ) );
            }

            if( !string.IsNullOrEmpty( ciBuildIndex ) && !CiBuildIdRegEx.IsMatch( ciBuildIndex ) )
            {
                throw new ArgumentException( string.Format( "Invalid build index '{0}'", ciBuildIndex ), nameof( ciBuildIndex ) );
            }

            if( !string.IsNullOrEmpty( ciBuildName ) && string.IsNullOrEmpty( ciBuildIndex ) )
            {
                throw new ArgumentException( "CiBuildIndex is required if CiBuildName is provided" );
            }

            CiBuildName = ciBuildName;
            CiBuildIndex = ciBuildIndex;
        }

        public string Name => Index >= 0 ? PreReleaseNames[ Index ] : string.Empty;

        public string ShortName => Index >= 0 ? PreReleaseShortNames[ Index ] : string.Empty;

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
            var bldr = new StringBuilder( );

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

        private static readonly Regex CiBuildIdRegEx = new Regex( @"[a-zA-z0-9\-]+" );
        private static readonly string[] PreReleaseNames = { "alpha", "beta", "delta", "epsilon", "gamma", "kappa", "prerelease", "rc" };
        private static readonly string[] PreReleaseShortNames = { "a", "b", "d", "e", "g", "k", "p", "r" };
    }
}
