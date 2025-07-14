// -----------------------------------------------------------------------
// <copyright file="GenerateHeader.cs" company="Ubiquity.NET Contributors">
// Copyright (c) Ubiquity.NET Contributors. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Ubiquity.NET.Versioning.Build.Tasks
{
    public class GenerateHeader
        : Task
    {
        [Required]
        public UInt16 FileVersionMajor { get; set; }

        [Required]
        public UInt16 FileVersionMinor { get; set; }

        [Required]
        public UInt16 FileVersionBuild { get; set; }

        [Required]
        public UInt16 FileVerionsRevision { get; set; }

        [Required]
        public string FileVersion { get; set; } = string.Empty;

        [Required]
        public string FullBuildNumber { get; set; } = string.Empty;

        [Required]
        public string GeneratedHeaderFilePath { get; set; } = string.Empty;

        [SuppressMessage( "Design", "CA1031:Do not catch general exception types", Justification = "Caught exceptions are logged as errors" )]
        public override bool Execute( )
        {
            try
            {
                // RequiredAttribute on the members means MSBuild will guarantee the
                // values are not null or empty.
                var template = new Templates.GeneratedHeader()
                {
                    FileVersionMajor = FileVersionMajor.ToString(CultureInfo.InvariantCulture),
                    FileVersionMinor = FileVersionMinor.ToString(CultureInfo.InvariantCulture),
                    FileVersionBuild = FileVersionBuild.ToString(CultureInfo.InvariantCulture),
                    FileVerionsRevision = FileVerionsRevision.ToString(CultureInfo.InvariantCulture),
                    FileVersion = FileVersion,
                    FullBuildNumber = FullBuildNumber
                };

                File.WriteAllText(GeneratedHeaderFilePath, template.TransformText());
                return true;
            }
            catch(Exception ex)
            {
                Log.LogErrorFromException(ex, showStackTrace: true);
                return false;
            }
        }
    }
}
