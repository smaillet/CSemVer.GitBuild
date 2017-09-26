using System;
using System.Collections.Generic;
using System.IO;

namespace CSemVer.GitBuild
{
    /// <summary>Version data for a build</summary>
    internal class BuildVersionData
    {
        /// <summary>Major portion of the build</summary>
        public UInt16 BuildMajor { get; private set; }

        /// <summary>Minor portion of the build</summary>
        public UInt16 BuildMinor { get; private set; }

        /// <summary>Patch portion of the build</summary>
        public UInt16 BuildPatch { get; private set; }

        public string PreReleaseName { get; private set; }

        public byte PreReleaseNumber { get; private set; }

        public byte PreReleaseFix { get; private set; }

        public string ReleaseBranch { get; private set; }

        public IReadOnlyDictionary<string, string> AdditionalProperties => ExtraPropertyMap;

        public string BuildVersionXmlFile { get; private set; }

        public static BuildVersionData Load( string path )
        {
            var retVal = new BuildVersionData( );
            using( var stream = File.OpenText( path ) )
            {
                var xdoc = System.Xml.Linq.XDocument.Load( stream, System.Xml.Linq.LoadOptions.None );
                var data = xdoc.Element( "BuildVersionData" );

                retVal.BuildVersionXmlFile = path;

                foreach( var attrib in data.Attributes( ) )
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

        private readonly Dictionary<string, string> ExtraPropertyMap = new Dictionary<string, string>();
    }
}
