param(
    [string]$GameDirectory = $env:SAILWIND_GAME_DIR,
    [string]$BepInExCoreDirectory = $env:BEPINEX_CORE_DIR
)

$ErrorActionPreference = 'Stop'
$projectDirectory = $PSScriptRoot

if (-not $GameDirectory) {
    $usualSteamDirectory = 'C:\Steam Games\steamapps\common\Sailwind'
    if (Test-Path -LiteralPath $usualSteamDirectory -PathType Container) {
        $GameDirectory = $usualSteamDirectory
    }
}

if (-not $GameDirectory) {
    throw 'Set -GameDirectory or SAILWIND_GAME_DIR to your installed Sailwind directory.'
}

$managedDirectory = Join-Path $GameDirectory 'Sailwind_Data\Managed'
foreach ($name in @('Assembly-CSharp.dll', 'UnityEngine.dll', 'UnityEngine.CoreModule.dll', 'UnityEngine.PhysicsModule.dll')) {
    if (-not (Test-Path -LiteralPath (Join-Path $managedDirectory $name) -PathType Leaf)) {
        throw "Missing game reference: $name in $managedDirectory"
    }
}

$referenceDirectory = Join-Path $projectDirectory '.local\references'
if ($BepInExCoreDirectory) {
    New-Item -ItemType Directory -Path $referenceDirectory -Force | Out-Null
    foreach ($name in @('BepInEx.dll', '0Harmony.dll')) {
        $source = Join-Path $BepInExCoreDirectory $name
        if (-not (Test-Path -LiteralPath $source -PathType Leaf)) {
            throw "Missing loader reference: $source"
        }
        Copy-Item -LiteralPath $source -Destination (Join-Path $referenceDirectory $name) -Force
    }
}

foreach ($name in @('BepInEx.dll', '0Harmony.dll')) {
    if (-not (Test-Path -LiteralPath (Join-Path $referenceDirectory $name) -PathType Leaf)) {
        throw "Missing loader reference: $name. Set -BepInExCoreDirectory or BEPINEX_CORE_DIR."
    }
}

& dotnet build (Join-Path $projectDirectory 'SailwindReceiptAlert.csproj') -c Release -p:GameManagedDir="$managedDirectory"
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed with exit code $LASTEXITCODE"
}
