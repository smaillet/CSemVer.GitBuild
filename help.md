# CommonBuild.psm1
Repository neutral common build support utilities

## Description
This library is intended for use across multiple repositories
and therefore, should only contain functionality that is independent
of the particulars of any given repository.

------
## Ensure-PathExists
Ensures a path exists on disk (Silently, Creates it if it doesn't exist)


## Get-DefaultBuildPaths
Gets the default set of paths for a build

### Description
This function initializes a hash table with the default paths for a build. This
allows for standardization of build output locations etc... across builds and repositories
in the organization. The values set are as follows:

| Name                | Description                          |
|---------------------|--------------------------------------|
| RepoRootPath        | Root of the repository for the build |
| BuildOutputPath     | Base directory for all build output during the build |
| NuGetRepositoryPath | NuGet 'packages' directory for C++ projects using packages.config |
| NuGetOutputPath     | Location where NuGet packages created during the build are placed |
| SrcRootPath         | Root of the source code for this repository |
| DocsOutputPath      | Root path for the generated documentation for the project |
| BinLogsPath         | Path to where the binlogs are generated for PR builds to allow diagnosing failures in the automated builds |
| TestResultsPath     | Path to where test results are placed. |
| DownloadsPath       | Location where any downloaded files, used by the build are placed |
| ToolsPath           | Location of any executable tools downloaded for the build (Typically expanded from a compressed download) |

## Assert-OfficialGitRemote
Verifies the current git remote is the official repository

### Description
Some operations like, Release tags, and docs updates must only be pushed from a repository with the
official GitHub repository as the origin remote. This, among other things, ensures that the links
to source in the generated docs will have the correct URLs (e.g. docs pushed to the official repository
MUST not have links to source in some private fork). This function is used, primarily in OneFlow release
management scripts to ensure operations are done using the correct remote.
### Parameters
|Name|Description|
|----|-----------|
|OFFICIALURL|URL of the official remote to Verify|
|ACTIVITY|String describing the activity. This is used in the exception message stating that the activity is only valid with the correct remote URL|

## Update-Submodules
Updates Git submodules for this repository


## Find-OnPath
Searches for an executable on the current environment search path

### Notes
This is a simple wrapper around the command line 'where' utility to select the first location found
### Parameters
|Name|Description|
|----|-----------|
|EXENAME|The executable to search for|

## ConvertTo-NormalizedPath
Converts a potentially relative folder path to an absolute one with a trailing delimiter

### Notes
The delimiters in the path are converted to the native system preferred form during conversion
### Parameters
|Name|Description|
|----|-----------|
|PATH|Path to convert|

## ConvertTo-PropertyList
Converts a hash table into a semi-colon delimited property list


## Invoke-TimedBlock
Invokes a script block with a timer

### Description
This will print a start (via Write-Information), start the timer, run the script block stop the timer
then print a finish message indicating the total time the script block took to run.
### Parameters
|Name|Description|
|----|-----------|
|ACTIVITY|Name of the activity to output as part of Write-Information messages for the timer|
|BLOCK|Script block to execute with the timer|

## Expand-ArchiveStream
Expands an archive stream

### Parameters
|Name|Description|
|----|-----------|
|SRC|Input stream containing compressed ZIP archive data to expand|
|OUTPUTPATH|Out put destination for the decompressed data|

## Expand-StreamFromUri
Downloads and expands a ZIP file to the specified destination

### Parameters
|Name|Description|
|----|-----------|
|URI|URI of the ZIP file to download and expand|
|OUTPUTPATH|Output folder to expand the ZIP contents into|

## Invoke-NuGet
Invokes NuGet with any arguments provided

### Description
This will attempt to find Nuget.exe on the current path, and if not found will download the latest
version from NuGet.org before running the command.

## Find-VSInstance
Finds an installed VS instance

### Description
Uses the official MS provided PowerShell module to find a VS instance. If the VSSetup
module is not loaded it is loaded first. If it isn't installed, then the module is installed.
### Parameters
|Name|Description|
|----|-----------|
|PRERELEASE|Indicates if the search should include pre-release versions of Visual Studio|
|VERSION|The version range to search for. [Default is '[15.0, 18.0)']|
|REQUIREDCOMPONENTS|The set of required components to search for. [Default is an empty array]|

## Find-MSBuild
Locates MSBuild if not already in the environment path

### Description
Attempts to find MSBuild on the current environment path, if not found uses Find-VSInstance
to locate a Visual Studio instance that can provide an MSBuild.
### Parameters
|Name|Description|
|----|-----------|
|ALLOWVSPRERELEASES|Switch to indicate if the search for a VS Instance should include pre-release versions.|

## Invoke-MSBuild
## Find-7Zip
Attempts to find 7zip console command

### Description
This will try to find 7z.exe on the current path, and if not found tries to find the registered install location

## Expand-7zArchive
Expands an archive packed with 7-Zip


## Get-CurrentBuildKind
Determines the kind of build for the current environment

### Description
This function retrieves environment values set by various automated builds
to determine the kind of build the environment is for. The return is one of
the [BuildKind] enumeration values:

| Name             | Description |
|------------------|-------------|
| LocalBuild       | This is a local developer build (e.g. not an automated build)
| PullRequestBuild | This is a build from a PullRequest with untrusted changes, so build should limit the steps appropriately |
| CiBuild          | This build is from a Continuous Integration (CI) process, usually after a PR is accepted and merged to the branch |
| ReleaseBuild     | This is an official release build, the output is ready for publication (Automated builds may use this to automatically publish) |

## Get-GitHubReleases
Gets a collection of the GitHub releases for a project

### Description
This function retrieves a collection of releases from a given GitHub organization and project.
The result is a collection of GitHub releases as JSON data.
### Parameters
|Name|Description|
|----|-----------|
|ORG|GitHub organization name that owns the project|
|PROJECT|GitHub project to retrieve releases from|

## Get-GitHubTaggedRelease
Gets a specific tagged release for a GitHub project

### Description
This function retrieves a single tagged release from a given GitHub organization and project.
The result is a GitHub release as JSON data.
### Parameters
|Name|Description|
|----|-----------|
|ORG|GitHub organization name that owns the project|
|PROJECT|GitHub project to retrieve releases from|
|TAG|Tag to find the specific release for|

## Invoke-DotNetTest
Invokes specified .NET tests for a project

### Description
This invokes 'dotnet.exe test ...' for the relative project path. The absolute path for the test to
run is derived from the buildInfo parameter.
### Parameters
|Name|Description|
|----|-----------|
|BUILDINFO|Hashtable of properties for the build. This function only requires two properties: "RepoRootPath", which is the Root of the repository this build is for and "SrcRootPath" that refers to the root of the source code of the repository. The relative project path is combined with the "RepoRootPath" to get the absolute path of the project to test. Additionally, there must be an 'x64.runsettings' file in the "SrcRootPath" to configure the proper settings for an x64 run.|
|PROJECTRELATIVEPATH|Relative path to the project to test. The absolute path is computed by combining $buildInfo['RepoRootPath'] with the relative path provided.|

## Get-BuildVersionXML
Retrieves the contents of the BuildVersion.XML file in the RepoRootPath

### Description
Reads the contents of the BuildVersion.xml file and returns it as XML
for additional processing.
### Parameters
|Name|Description|
|----|-----------|
|BUILDINFO|Hashtable containing Information about the repository and build. This function requires the presence of a 'RepoRootPath' property to indicate the root of the repository containing the BuildVersion.xml file.|
|XMLPATH|Path of the XML file to retrieve. This is mutually exclusive with the BuildInfo parameter, it is generally only used when developing these build scripts to explicitly retrieve the version from a path without needing the hashtable.|

## Get-ParsedBuildVersionXML
Retrieves the contents of the BuildVersion.XML file in the RepoRootPath and parses it to a hashtable

### Description
Reads the contents of the BuildVersion.xml file and returns it as parsed hashtable
where each attribute of the root element is a key in the table.
### Parameters
|Name|Description|
|----|-----------|
|BUILDINFO|Hashtable containing Information about the repository and build. This function requires the presence of a 'RepoRootPath' property to indicate the root of the repository containing the BuildVersion.xml file.|
|XMLPATH|Path of the XML file to retrieve. This is mutually exclusive with the BuildInfo parameter, it is generally only used when developing these build scripts to explicitly retrieve the version from a path without needing the hashtable.|

## Get-BuildVersionTag
Retrieves the git tag name to apply for this build.

### Description
Reads the contents of the BuildVersion.xml file and generates a git
release tag name for the current build.

This is a standalone function instead of a property on the build
information Hashtable so that it is always dynamically evaluated
based on the current contents of the BuildVersion.XML file as that
is generally updated when this is needed.
### Parameters
|Name|Description|
|----|-----------|
|BUILDINFO|Hashtable containing Information about the repository and build. This function requires the table include an entry for 'RepoRootPath' to indicate the root of the repository containing the BuildVersion.xml file.|
|RELEASENAMEONLY|Returns only the final release name of the current version dropping any pre-release if present. This is mostly used in cases that need to determine the release Name before the XML file is updated. Including creation of a release branch where the change to the XML will occur.|

## Initialize-CommonBuildEnvironment
Initializes the build environment for the build scripts

### Description
This script is used to initialize the build environment in a central place, it returns the
build info Hashtable with properties determined for the build. Script code should use these
properties instead of any environment variables. While this script does setup some environment
variables for non-script tools (i.e., MSBuild) script code should not rely on those as the goal
is to remove the Environment variables going forward (Passing them as parameters/properties etc...)

This script will setup the PATH environment variable to contain the path to MSBuild so it is
readily available for all subsequent script code.

Environment variables set for non-script tools:

| Name               | Description |
|--------------------|-------------|
| IsAutomatedBuild   | "true" if in an automated build environment "false" for local developer builds |
| IsPullRequestBuild | "true" if this is a build from an untrusted pull request (limited build, no publish etc...) |
| IsReleaseBuild     | "true" if this is an official release build |
| CiBuildName        | Name of the build for Constrained Semantic Version construction |
| BuildTime          | ISO-8601 formatted time stamp for the build (local builds are based on current time, automated builds use the time from the HEAD commit)

This function returns a Hashtable containing properties for the current build with the following properties:

| Name                | Description                          |
|---------------------|--------------------------------------|
| RepoRootPath        | Root of the repository for the build |
| BuildOutputPath     | Base directory for all build output during the build |
| NuGetRepositoryPath | NuGet 'packages' directory for C++ projects using packages.config |
| NuGetOutputPath     | Location where NuGet packages created during the build are placed |
| SrcRootPath         | Root of the source code for this repository |
| DocsOutputPath      | Root path for the generated documentation for the project |
| BinLogsPath         | Path to where the binlogs are generated for PR builds to allow diagnosing failures in the automated builds |
| TestResultsPath     | Path to where test results are placed. |
| DownloadsPath       | Location where any downloaded files, used by the build are placed |
| ToolsPath           | Location of any executable tools downloaded for the build (Typically expanded from a compressed download) |
| CurrentBuildKind    | Results of a call to Get-CurrentBuildKind |
| MsBuildLoggerArgs   | Array of MSBuild arguments, normally this only contains the Logger parameters for the console logger verbosity |
| MSBuildInfo         | Information about the found version of MSBuild (from a call to Find-MSBuild) |
| VersionTag          | Git tag name for this build if released |
| CiBuildName         | Name of the build for Constrained Semantic Version construction |
| BuildTime           | ISO-8601 formatted time stamp for the build (local builds are based on current time, automated builds use the time from the HEAD commit)
### Parameters
|Name|Description|
|----|-----------|
|FULLINIT|Performs a full initialization. A full initialization includes forcing a re-capture of the time stamp for local builds as well as writes details of the initialization to the information and verbose streams.|
|ALLOWVSPRERELEASES|Switch to enable use of Visual Studio Pre-Release versions. This is NEVER enabled for official production builds, however it is useful when adding support for new versions during the pre-release stages.|
|DEFAULTMSBUILDVERBOSITY|Default MSBuild verbosity for the console logger output, default value for this is 'Minimal'.|

## Show-FullBuildInfo
Displays details of the build information and environment to the information and verbose streams

### Description
This function displays all the properties of the buildinfo to the information stream. Additionally,
details of the current PATH, the .NET SDKs and runtimes installed is logged to the Verbose stream.
### Parameters
|Name|Description|
|----|-----------|
|BUILDINFO|The build information Hashtable for the build. This normally contains the standard and repo specific properties so that the full details are available in logs.|

## Invoke-Git
Invokes GIT commands and throws an exception if the command failed


## Assert-CmakeInfo
Asserts that CMAKE is available and at least matches the minimum version specified

### Parameters
|Name|Description|
|----|-----------|
|MINVERSION|Minimum version of CMAKE allowed. Older versions trigger an exception|

