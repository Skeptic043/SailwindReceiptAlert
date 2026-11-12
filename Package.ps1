param(
    [string]$GameManagedDir = 'C:\Steam Games\steamapps\common\Sailwind\Sailwind_Data\Managed',
    [string]$ReferenceDir = ''
)

$ErrorActionPreference = 'Stop'
& (Join-Path $PSScriptRoot 'Build.ps1') -GameManagedDir $GameManagedDir -ReferenceDir $ReferenceDir

Add-Type -AssemblyName System.IO.Compression.FileSystem
Add-Type -AssemblyName System.Drawing

$manifest = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'manifest.json') -Raw | ConvertFrom-Json
if ($manifest.name -cnotmatch '^[A-Za-z0-9_]{1,128}$') { throw 'Invalid package name.' }
if ($manifest.version_number -notmatch '^\d+\.\d+\.\d+$') { throw 'Invalid package version.' }
if ([string]::IsNullOrWhiteSpace($manifest.description) -or $manifest.description.Length -gt 250) {
    throw 'Package description must contain 1-250 characters.'
}
if ($manifest.website_url -notmatch '^https?://') { throw 'Invalid website URL.' }

$icon = [Drawing.Image]::FromFile((Join-Path $PSScriptRoot 'icon.png'))
try {
    if ($icon.Width -ne 256 -or $icon.Height -ne 256 -or
        $icon.RawFormat.Guid -ne [Drawing.Imaging.ImageFormat]::Png.Guid) {
        throw 'Icon must be a 256x256 PNG.'
    }
}
finally { $icon.Dispose() }

$files = [ordered]@{
    'manifest.json' = (Join-Path $PSScriptRoot 'manifest.json')
    'README.md' = (Join-Path $PSScriptRoot 'README.md')
    'CHANGELOG.md' = (Join-Path $PSScriptRoot 'CHANGELOG.md')
    'LICENSE' = (Join-Path $PSScriptRoot 'LICENSE')
    'icon.png' = (Join-Path $PSScriptRoot 'icon.png')
    'BepInEx/plugins/SailwindReceiptAlert/SailwindReceiptAlert.dll' = (Join-Path $PSScriptRoot 'bin/Release/net471/SailwindReceiptAlert.dll')
}
foreach ($source in $files.Values) {
    if (-not (Test-Path -LiteralPath $source -PathType Leaf)) { throw "Missing package file: $source" }
}

$outputDirectory = Join-Path $PSScriptRoot 'artifacts'
New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
$zipPath = Join-Path $outputDirectory "SailwindReceiptAlert-$($manifest.version_number).zip"
if (Test-Path -LiteralPath $zipPath) { Remove-Item -LiteralPath $zipPath }

$archive = [IO.Compression.ZipFile]::Open($zipPath, [IO.Compression.ZipArchiveMode]::Create)
try {
    foreach ($entry in $files.GetEnumerator()) {
        $null = [IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
            $archive, $entry.Value, $entry.Key, [IO.Compression.CompressionLevel]::Optimal)
    }
}
finally { $archive.Dispose() }

Write-Host "Packaged $($files.Count) files in $zipPath"
