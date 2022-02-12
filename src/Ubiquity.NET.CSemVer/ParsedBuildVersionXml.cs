using System;
using System.IO;

namespace Ubiquity.NET.CSemVer
{
    public class ParsedBuildVersionXml
    {
        public ParsedBuildVersionXml( string path )
        {
            using var stream = File.OpenText( path );
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

        public int BuildMajor { get; } = 0;

        public int BuildMinor { get; } = 0;

        public int BuildPatch { get; } = 0;

        public string PreReleaseName { get; } = string.Empty;

        public int PreReleaseNumber { get; } = 0;

        public int PreReleaseFix { get; } = 0;
    }
}
