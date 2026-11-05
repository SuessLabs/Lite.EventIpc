[CmdletBinding()]
param(
  [Parameter(Mandatory = $true)]
  [ValidateSet('develop', 'master')]
  [string]$Branch,

  [string]$RepositoryRoot = (Join-Path $PSScriptRoot '../..')
)

$ErrorActionPreference = 'Stop'
$path = Join-Path $RepositoryRoot 'src/Directory.Build.props'
$content = [System.IO.File]::ReadAllText($path)
$versionMatches = [regex]::Matches($content, '<AssemblyVersion>(\d+)\.(\d+)\.(\d+)\.(\d+)</AssemblyVersion>')
if ($versionMatches.Count -ne 1)
{
  throw 'Expected exactly one major.minor.patch.build AssemblyVersion in src/Directory.Build.props.'
}

$versionMatch = $versionMatches[0]
$major = [int]$versionMatch.Groups[1].Value
$minor = [int]$versionMatch.Groups[2].Value
$patch = [int]$versionMatch.Groups[3].Value
$build = if ($versionMatch.Groups[4].Success) { [int]$versionMatch.Groups[4].Value } else { 0 }

if ($Branch -eq 'develop')
{
  $build++
}
else
{
  $minor++
  $patch = 0
  $build = 0
}

# AssemblyVersion components must be below 65535.
foreach ($component in @($major, $minor, $patch, $build))
{
  if ($component -ge 65535)
  {
    throw 'The incremented version exceeds the AssemblyVersion component limit (65534).'
  }
}
$nextVersion = '{0}.{1}.{2}.{3}' -f $major, $minor, $patch, $build

$content = $content.Replace($versionMatch.Value, "<AssemblyVersion>$nextVersion</AssemblyVersion>")
[System.IO.File]::WriteAllText($path, $content, [System.Text.UTF8Encoding]::new($false))

Write-Output $nextVersion
