param(
    [string]$GameManagedDir = 'C:\Steam Games\steamapps\common\Sailwind\Sailwind_Data\Managed',
    [string]$ReferenceDir = ''
)

$ErrorActionPreference = 'Stop'
& (Join-Path $PSScriptRoot 'Build.ps1') -GameManagedDir $GameManagedDir -ReferenceDir $ReferenceDir

$testArgs = @("-p:GameManagedDir=$GameManagedDir")
if (-not [string]::IsNullOrWhiteSpace($ReferenceDir)) { $testArgs += "-p:ReferenceDir=$ReferenceDir" }
& dotnet run --project (Join-Path $PSScriptRoot 'tests\ReceiptAlert.Tests.csproj') -c Release @testArgs
if ($LASTEXITCODE -ne 0) { throw "Receipt Alert checks failed with exit code $LASTEXITCODE." }
