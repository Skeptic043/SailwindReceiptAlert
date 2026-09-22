# Sailwind Receipt Alert

Sailwind Receipt Alert shows Sailwind's normal brief notification when you walk away from a port's trade and mission desk with a trade receipt still waiting. It does not print the receipt or interrupt movement. If you return to the desk and leave again while the receipt is still available, it reminds you again.

The warning area follows each port's native `PortDude`. It covers the desk area, including outdoor desks, rather than trying to detect a building doorway.

## Install

Install BepInEx 5 for Sailwind, then copy `SailwindReceiptAlert.dll` into `Sailwind/BepInEx/plugins/`. Restart the game. The mod does not require NANDTweaks.

## Build

Run `./Build.ps1 -GameDirectory 'C:\path\to\Sailwind' -BepInExCoreDirectory 'C:\path\to\BepInEx\core'` from this repository. The build reads game and Unity assemblies from the Sailwind installation and copies the BepInEx and Harmony reference DLLs into an ignored local folder. The game installation is not changed.

## Check in game

After trading, close the trade book and walk away from the desk before collecting the receipt. A brief native notification should appear once as you leave the area. Re-enter and leave to see it again. Collect or print the receipt and repeat to confirm no warning appears. Check both an indoor office and an outdoor desk. The trigger size has only been checked against game files, so in-game distance and collider behavior still need confirmation.

## License

MIT. See [LICENSE](LICENSE).
