# Sailwind Receipt Alert

Get a brief reminder or automatically collect a waiting trade receipt when you walk away from a port's trade and mission desk.

## Install

### Mod managers

Install Sailwind Receipt Alert through [Sailwind Mod Synchronizer](https://github.com/foxyv/SailwindModSynchronizer/releases/latest), r2modman or Thunderstore Mod Manager, then launch Sailwind through your manager. Dependencies are installed automatically.

### Manual installation

1. Install [BepInEx 5](https://github.com/BepInEx/BepInEx/releases) in your Sailwind game folder, following its installation instructions.
2. Download and extract the [Sailwind Receipt Alert ZIP](https://github.com/Skeptic043/SailwindReceiptAlert/releases/latest). Copy its `BepInEx/plugins/SailwindReceiptAlert` folder into `BepInEx/plugins` in your Sailwind folder.
3. Launch Sailwind normally.

## How it works

After buying or selling through a port's trade book, leaving the trade and mission desk area with a receipt waiting shows a brief reminder. If you return and leave again without collecting it, the reminder appears again. With automatic collection enabled, leaving the same area instead adds the receipt to your trade receipts and shows a brief confirmation. The desk areas include indoor and outdoor ports.

## Configuration

Settings are in `BepInEx/config/skeptic043.sailwind.receiptalert.cfg` after the first launch, or in game with the BepInEx 5 version of [BepInEx Configuration Manager](https://github.com/BepInEx/BepInEx.ConfigurationManager).

| Setting | Default | Options |
| --- | --- | --- |
| `Enabled` | `true` | Turns the reminder and automatic collection on or off. |
| `AutoCollectReceipt` | `false` | Collects the receipt when you leave the desk area. When `false`, the mod only reminds you. |

## Compatibility

No known incompatibilities.

## AI Use

AI was used to write all of the code in this project. The original concept, design direction, testing, debugging, and release decisions are my own. If you prefer not to use mods developed with AI assistance, I understand and respect that choice.

## Links

[Report an issue](https://github.com/Skeptic043/SailwindReceiptAlert/issues) · [Source code](https://github.com/Skeptic043/SailwindReceiptAlert) · [Release notes](https://github.com/Skeptic043/SailwindReceiptAlert/blob/main/CHANGELOG.md) · [Build instructions](https://github.com/Skeptic043/SailwindReceiptAlert/blob/main/BUILDING.md) · [License](https://github.com/Skeptic043/SailwindReceiptAlert/blob/main/LICENSE) · [Support on Ko-fi](https://ko-fi.com/skeptic043) · skeptic043
