# Sailwind Receipt Alert

Get a brief reminder or automatically collect a waiting trade receipt when you walk away from a port's trade and mission desk.

## Install

### Mod managers

Install [Sailwind Receipt Alert](https://thunderstore.io/c/sailwind/p/Skeptic043/Sailwind_Receipt_Alert/) through **r2modman** or **Thunderstore Mod Manager**, then launch Sailwind through your manager. Dependencies are installed automatically.

### Manual installation

1. Install [BepInEx 5](https://github.com/BepInEx/BepInEx/releases) in your Sailwind game folder, following its installation instructions.
2. Download and extract the mod ZIP. Copy its `plugins/SailwindReceiptAlert` folder into `BepInEx/plugins` in your Sailwind folder.
3. Launch Sailwind normally.

## How it works

After buying or selling through a port's trade book, leaving the trade and mission desk area with a receipt waiting shows a brief reminder. If you return and leave again without collecting it, the reminder appears again. With automatic collection enabled, leaving the same area instead adds the receipt to your trade receipts and shows a brief confirmation. The desk areas include indoor and outdoor ports.

## Configuration

After the first launch, edit `BepInEx/config/skeptic043.sailwind.receiptalert.cfg` to change these settings:

| Setting | Default | Values | Effect |
| --- | --- | --- | --- |
| `General.Enabled` | `true` | `true`, `false` | Enables or disables the mod. |
| `General.AutoCollectReceipt` | `false` | `true`, `false` | Collects the receipt when you leave the desk area. When `false`, the mod only reminds you. |

## AI Use

AI was used to write all of the code in this project. The original concept, design direction, testing, debugging, and release decisions are my own. If you prefer not to use mods developed with AI assistance, I understand and respect that choice.

## Issues and links

[Report an issue](https://github.com/Skeptic043/SailwindReceiptAlert/issues) with your `BepInEx/LogOutput.log`.

[Source code](https://github.com/Skeptic043/SailwindReceiptAlert) · [Build from source](https://github.com/Skeptic043/SailwindReceiptAlert/blob/main/BUILDING.md) · [MIT License](https://github.com/Skeptic043/SailwindReceiptAlert/blob/main/LICENSE) · [Support on Ko-fi](https://ko-fi.com/skeptic043) · skeptic043
