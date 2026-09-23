# Building from source

Install the .NET SDK, Sailwind, and BepInEx 5. Run the following from this repository in PowerShell, using the directories for your installations:

```powershell
./Build.ps1 -GameDirectory 'C:\path\to\Sailwind' -BepInExCoreDirectory 'C:\path\to\BepInEx\core'
```

The DLL is written to `bin/Release/net471/SailwindReceiptAlert.dll`. The build copies BepInEx and Harmony reference DLLs into the ignored `.local/references` directory. Game assemblies are read directly from the Sailwind installation. The build does not change the game installation.

To build the DLL and create a Thunderstore package ZIP:

```powershell
./Package.ps1 -GameDirectory 'C:\path\to\Sailwind' -BepInExCoreDirectory 'C:\path\to\BepInEx\core'
```

The package and its SHA-256 checksum are written to `artifacts/release/`. The ZIP contains only the package metadata, public documentation, license, icon, and mod DLL. It does not contain game or BepInEx assemblies.
