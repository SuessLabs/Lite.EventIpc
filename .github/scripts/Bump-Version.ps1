[CmdletBinding()]
param(
  [Parameter(Mandatory = $true)]
  [ValidateSet('develop', 'master')]
  [string]$Branch,

  [string]$RepositoryRoot = (Join-Path $PSScriptRoot '../..')
)

$ErrorActionPreference = 'Stop'
$projectPaths = @(
  'src/Lite.EventIpc/Lite.EventIpc.csproj'
  'src/Lite.EventIpc.Hosting/Lite.EventIpc.Hosting.csproj'
)

# Validate both projects before modifying either file.
$projects = foreach ($relativePath in $projectPaths)
{
  $path = Join-Path $RepositoryRoot $relativePath
  $content = [System.IO.File]::ReadAllText($path)
  $matches = [regex]::Matches($content, '<AssemblyVersion>(\d+)\.(\d+)\.(\d+)(?:\.(\d+))?</AssemblyVersion>')
  if ($matches.Count -ne 1)
  {
    throw "Expected exactly one major.minor.patch[.build] AssemblyVersion in $relativePath."
  }

  [pscustomobject]@{ Path = $path; Content = $content; Match = $matches[0] }
}

$currentVersion = $projects[0].Match.Value
if ($projects[1].Match.Value -ne $currentVersion)
{
  throw 'The library project versions must match before incrementing the version.'
}

$versionMatch = $projects[0].Match
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

foreach ($project in $projects)
{
  $content = $project.Content.Replace($project.Match.Value, "<AssemblyVersion>$nextVersion</AssemblyVersion>")
  [System.IO.File]::WriteAllText($project.Path, $content, [System.Text.UTF8Encoding]::new($false))
}

Write-Output $nextVersion
