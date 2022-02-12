<#
.SYNOPSIS
    Finalizes a release buy updating the master branch in the upstream repository

.DESCRIPTION
    Generally, this function will reset the master branch to point to the released tag name.

    pushing the tag should trigger the start the official build via a GitHub action, once that successfully
    completes, this script is used to update master to point to the release tag.
#>

Param([Parameter(Mandatory=$True)]$commit)
$repoRoot = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($PSScriptRoot, ".."))
. (join-path $repoRoot repo-buildutils.ps1)
$buildInfo = Initialize-BuildEnvironment

# create script scoped alias for git that throws a PowerShell exception if the command fails
Set-Alias git Invoke-git -scope Script -option Private

# pushing the tag to develop branch on the official repository triggers the official build and release of the Nuget Packages
$tagName = Get-BuildVersionTag $buildInfo

git checkout master
git merge --ff-only $tagName
git push upstream master
