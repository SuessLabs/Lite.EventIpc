[CmdletBinding()]
param(
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
  $matches = [regex]::Matches($content, '<AssemblyVersion>(\d+)\.(\d+)\.(\d+)</AssemblyVersion>')
  if ($matches.Count -ne 1)
  {
    throw "Expected exactly one major.minor.patch AssemblyVersion in $relativePath."
  }

  [pscustomobject]@{ Path = $path; Content = $content; Match = $matches[0] }
}

$currentVersion = $projects[0].Match.Value
if ($projects[1].Match.Value -ne $currentVersion)
{
  throw 'The library project versions must match before incrementing the patch version.'
}

$versionMatch = $projects[0].Match
$patch = [int]$versionMatch.Groups[3].Value + 1
$nextVersion = '{0}.{1}.{2}' -f $versionMatch.Groups[1].Value, $versionMatch.Groups[2].Value, $patch

foreach ($project in $projects)
{
  $content = $project.Content.Replace($project.Match.Value, "<AssemblyVersion>$nextVersion</AssemblyVersion>")
  [System.IO.File]::WriteAllText($project.Path, $content, [System.Text.UTF8Encoding]::new($false))
}

Write-Output $nextVersion
