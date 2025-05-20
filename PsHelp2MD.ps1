param($scriptFile)

function NormalizeLines($line, $replacement = [Environment]::NewLine)
{
    $retVal = $line.Trim() -replace "`r`n|`r|`n", $replacement
    return $retVal
}

function FindAllScriptFunctions
{
    [cmdletbinding()]
    param(
        [Parameter(Mandatory=$True)]
        [System.Management.Automation.Language.Ast] $ast
    )

    # Findall predicate to filter down to only function definitions.
    # In Powershsell 5+ (with classes) class methods are a derived type
    # of function definition, so skip those.
    $funcDefPredicate = [System.Func[System.Management.Automation.Language.Ast,bool]] {
        param([System.Management.Automation.Language.Ast] $ast)
        return $ast -is [System.Management.Automation.Language.FunctionDefinitionAst] -and ($PSVersionTable.PSVersion.Major -lt 5 -or $ast.Parent -isnot [System.Management.Automation.Language.FunctionMemberAst])
    }

    $ast.FindAll($funcDefPredicate, $true)
}

function GenerateMarkDownForHelpInfo
{
    param(
        [Parameter(Mandatory=$True,ValuefromPipeline=$True)]
        [System.Management.Automation.Language.CommentHelpInfo] $helpInfo,
        [int] $outlineDepth = 3
    )

    Begin
    {
        $sectionDepth = [string]::new([char]'#', $outlineDepth)
    }

    Process
    {
        try
        {
            $bldr = [System.Text.StringBuilder]::new()

            if($helpInfo)
            {
                if(![string]::IsNullOrWhitespace($helpInfo.SynOpsis))
                {
                    $bldr.AppendLine((NormalizeLines $HelpInfo.SynOpsis)) | Out-Null
                    $bldr.AppendLine() | Out-Null
                }

                if(![string]::IsNullOrWhitespace($helpInfo.Description))
                {
                    $bldr.AppendLine("$sectionDepth Description").AppendLine((NormalizeLines $helpInfo.Description)) | Out-Null
                }

                if(![string]::IsNullOrWhitespace($helpInfo.Examples))
                {
                    $bldr.AppendLine("$sectionDepth Examples").AppendLine((NormalizeLines $helpInfo.Examples)) | Out-Null
                }

                if(![string]::IsNullOrWhitespace($helpInfo.Notes))
                {
                    $bldr.AppendLine("$sectionDepth Notes").AppendLine((NormalizeLines $helpInfo.Notes)) | Out-Null
                }

                if($helpInfo.Parameters.Count -gt 0)
                {
                    $bldr.AppendLine("$sectionDepth Parameters") | Out-Null
                    $bldr.AppendLine('|Name|Description|') | Out-Null
                    $bldr.AppendLine('|----|-----------|') | Out-Null

                    # For reasons unknown PS won't iterate on the members
                    # of a dictionary without the use of GetEnumerator()...
                    foreach($kvp in $helpInfo.Parameters.GetEnumerator())
                    {
                        # trim leading and trailing whitepsace and convert embedded new lines into a space
                        $content = NormalizeLines $kvp.Value ' '
                        $bldr.AppendLine("|$($kvp.Key)|$content|") | Out-Null
                    }
                }
            }
            $bldr.ToString()
        }
        catch
        {
            throw
        }
    }
}

function GenerateMarkDownForDef
{
    param(
        [Parameter(Mandatory=$True,ValuefromPipeline=$True)]
        [System.Management.Automation.Language.FunctionDefinitionAst] $def
    )

    Process
    {
        "## $($def.Name)"
        $helpInfo = $def.GetHelpContent()
        if ($helpInfo)
        {
            GenerateMarkDownForHelpInfo $helpInfo
        }
    }
}

$tokens = $errors = $null
$ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptFile, [ref]$tokens, [ref]$errors)
if($errors)
{
    foreach($err in $errors)
    {
        Write-Error $err.ToString()
    }
}

if(!$ast)
{
    throw "Failed parsing the source '$scriptFile'"
}

"# $([System.IO.Path]::GetFileName($scriptFile))"
$astHelpInfo = $ast.GetHelpContent()
if($astHelpInfo)
{
    GenerateMarkDownForHelpInfo -HelpInfo $astHelpInfo -OutlineDepth 2
}
"------"
FindAllScriptFunctions $ast | GenerateMarkDownForDef
