param(
    [string]$GameManagedDir = 'C:\Steam Games\steamapps\common\Sailwind\Sailwind_Data\Managed',
    [string]$ReferenceDir = ''
)

$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($ReferenceDir)) {
    $localReferences = Join-Path $PSScriptRoot '.local\references'
    if (Test-Path -LiteralPath (Join-Path $localReferences 'BepInEx.dll')) {
        $ReferenceDir = $localReferences
    } else {
        $ReferenceDir = Join-Path $env:LOCALAPPDATA 'SailwindModSynchronizer\packs\default\instance\BepInEx\core'
    }
}

foreach ($required in @(
    (Join-Path $GameManagedDir 'Assembly-CSharp.dll'),
    (Join-Path $GameManagedDir 'UnityEngine.CoreModule.dll'),
    (Join-Path $GameManagedDir 'UnityEngine.PhysicsModule.dll'),
    (Join-Path $ReferenceDir 'BepInEx.dll'),
    (Join-Path $ReferenceDir '0Harmony.dll')
)) {
    if (-not (Test-Path -LiteralPath $required -PathType Leaf)) {
        throw "Required build reference is missing: $required"
    }
}

& dotnet build (Join-Path $PSScriptRoot 'SailwindReceiptAlert.csproj') -c Release `
    "-p:GameManagedDir=$GameManagedDir" "-p:ReferenceDir=$ReferenceDir"
if ($LASTEXITCODE -ne 0) { throw "Receipt Alert build failed with exit code $LASTEXITCODE." }
