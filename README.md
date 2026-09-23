# Sailwind Receipt Alert

Get a brief in-game reminder when you walk away from a port's trade and mission desk with a trade receipt still waiting.

## Install

### Mod managers

Install Sailwind Receipt Alert through **r2modman** or **Thunderstore Mod Manager**, then launch Sailwind through your manager. Dependencies are installed automatically.

### Manual installation

1. Install [BepInExPack](https://thunderstore.io/c/sailwind/p/BepInEx/BepInExPack/) in your Sailwind game folder, following its installation instructions.
2. Download and extract the mod ZIP. Copy its `plugins/SailwindReceiptAlert` folder into `BepInEx/plugins` in your Sailwind folder.
3. Launch Sailwind normally.

## During play

After buying or selling through a port's trade book, leave the trade and mission desk area without collecting the receipt. Sailwind briefly reminds you to pick it up. The reminder works at indoor and outdoor desks. If you return and leave again while the receipt is still waiting, it appears again.

Collecting the receipt stops the reminder. Sailwind clears an uncollected receipt when you reload a save.

## Configuration

The mod is enabled by default. To turn it off, set `General.Enabled` to `false` in `BepInEx/config/skeptic043.sailwind.receiptalert.cfg` after the first launch.

## AI Use

AI was used to write all of the code in this project. The original concept, design direction, testing, debugging, and release decisions are my own. If you prefer not to use mods developed with AI assistance, I understand and respect that choice.

## Issues and links

[Report an issue](https://github.com/Skeptic043/SailwindReceiptAlert/issues) with your `BepInEx/LogOutput.log`.

[Source code](https://github.com/Skeptic043/SailwindReceiptAlert) · [Build from source](https://github.com/Skeptic043/SailwindReceiptAlert/blob/main/BUILDING.md) · [MIT License](https://github.com/Skeptic043/SailwindReceiptAlert/blob/main/LICENSE) · [Support on Ko-fi](https://ko-fi.com/skeptic043) · skeptic043
