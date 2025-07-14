// -----------------------------------------------------------------------
// <copyright file="GeneratedHeader.cs" company="Ubiquity.NET Contributors">
// Copyright (c) Ubiquity.NET Contributors. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Ubiquity.NET.Versioning.Build.Tasks.Templates
{
    internal partial class GeneratedHeader
    {
        public string ToolName => GetType( ).Assembly.FullName;

        public string ToolVersion => GetType( ).Assembly.GetAssemblyInformationalVersion( );

        public string FileVersionMajor { get; init; } = string.Empty;

        public string FileVersionMinor { get; init; } = string.Empty;

        public string FileVersionBuild { get; init; } = string.Empty;

        public string FileVerionsRevision { get; init; } = string.Empty;

        public string FileVersion { get; init; } = string.Empty;

        public string FullBuildNumber { get; init; } = string.Empty;
    }
}
