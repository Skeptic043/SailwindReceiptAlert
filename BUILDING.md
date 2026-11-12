# Building Sailwind Receipt Alert

## Requirements

- Windows with PowerShell 7 and the .NET 10 SDK (the tests target .NET 10).
- .NET Framework 4.7.1 reference assemblies.
- A local Sailwind installation with BepInEx 5.

Game and loader assemblies are build references only. They are not included in this repository or its packages.

## Build

From the repository folder:

```powershell
pwsh -File ./Build.ps1 -GameManagedDir 'C:\Games\Sailwind\Sailwind_Data\Managed' -ReferenceDir 'C:\Games\Sailwind\BepInEx\core'
```

Use the paths for your own installation or mod profile. Without `-ReferenceDir`, the build uses `.local/references/` when it holds `BepInEx.dll`, otherwise the Sailwind Mod Synchronizer default pack's `BepInEx/core` folder. The output is `bin/Release/net471/SailwindReceiptAlert.dll`.

## Test

```powershell
pwsh -File ./Test.ps1 -GameManagedDir 'C:\Games\Sailwind\Sailwind_Data\Managed' -ReferenceDir 'C:\Games\Sailwind\BepInEx\core'
```

This builds the mod, then checks which action leaving a desk takes for each setting combination and that the game members the mod hooks by name still exist. The checks read `Mono.Cecil.dll` from the BepInEx `core` folder. Any failure makes the command fail.

## Package

```powershell
pwsh -File ./Package.ps1 -GameManagedDir 'C:\Games\Sailwind\Sailwind_Data\Managed' -ReferenceDir 'C:\Games\Sailwind\BepInEx\core'
```

This builds the mod and writes `artifacts/SailwindReceiptAlert-<version>.zip` with `BepInEx/plugins/SailwindReceiptAlert/`, `manifest.json`, `icon.png`, `README.md`, `CHANGELOG.md` and `LICENSE`.

Run `./tools/Render-Icon.ps1` to regenerate the 256×256 icon from its editable shapes.
