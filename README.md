# Prism Library for Playnite

Playnite library plugin that imports [Prism Launcher](https://prismlauncher.org/) Minecraft instances.

## Features

- Import every Prism instance into Playnite
- Enable only the instances you want as **Installed** (playable); others stay **Uninstalled**
- Launch via Prism CLI (`--dir` + `--launch`)
- Dual path settings for Windows install vs data roots (and portable layouts)
- Icons, Minecraft version / loader tags, Prism groups as categories
- Playtime and last launch from Prism
- Optional default account profile
- Secondary actions: Open in Prism, Open instance folder, Open `.minecraft` folder
- Managed modpack detection (Modrinth / CurseForge / FTB) with platform tags and links
- Download Metadata fills description, cover, background, authors, and links from pack platforms
- Managed pack update checks (compare platform latest vs `ManagedPackVersionID`); **Update in Prism** opens the instance so Prism applies the update

Install / Uninstall in Playnite only toggles enablement. Prism instance files are never deleted.

## Requirements

- Windows
- [Playnite](https://playnite.link/) 9+
- Prism Launcher

## Install

The Playnite add-on browser does not list this plugin yet (new plugin submissions are on hold), so install from GitHub:

1. Download the latest `.pext` from [Releases](https://github.com/Loneranger419/playnite-prism-library/releases).
2. Open the file (double-click, or drag it onto Playnite). Playnite will install the extension and ask to restart.
3. After restart: **Add-on settings → Libraries → Prism Library** (or **Prism Launcher**), set **executable** and **data root** (or **Detect defaults**), enable the instances you want as Installed, then **Update Game Library**.

### Updating (manual)

Playnite will not auto-update this extension until it is accepted into the [add-on database](https://github.com/JosefNemec/PlayniteAddonDatabase). To update:

1. Download the newer `.pext` from [Releases](https://github.com/Loneranger419/playnite-prism-library/releases).
2. Open it with Playnite again (same as install). Restart when prompted.

### Development install

For local builds instead of a release `.pext`:

1. `.\build-and-test.ps1`
2. Playnite → **Settings → For developers → External extensions** → add `src\PrismLibrary\bin\Release\net462`
3. Restart Playnite and configure as above.

Do not load the same AddonId both as an installed `.pext` and as an external extension folder at once.

## Configuration

| Setting | Typical value |
|---------|----------------|
| Executable | `%LocalAppData%\Programs\PrismLauncher\prismlauncher.exe` |
| Data root | `%AppData%\PrismLauncher` (or the portable folder containing `instances/`) |
| CurseForge API key | Only needed for CurseForge pack metadata (see below) |

## Pack metadata (Modrinth / CurseForge / FTB)

Instances installed through Prism as managed packs expose `ManagedPack*` fields. On library update the plugin adds a platform tag and project link with no network calls.

For description, cover art, background, authors, and platform categories:

1. Optionally add a **CurseForge API key** in extension settings (Modrinth and FTB need no key).
2. In Playnite, run **Download Metadata** for the game(s), with the Prism / store source selected for those fields.

Tags from Download Metadata merge Prism data (Minecraft, MC version, loader, platform) with pack-page categories. Markdown descriptions (common for FTB) are converted to HTML for Playnite. YouTube embeds are converted to normal links (Playnite cannot play iframes).

Create a CurseForge key in the [CurseForge for Studios](https://console.curseforge.com/) developer console. Without a key, CurseForge packs still import and keep tag/link; rich CF metadata and CF update checks are skipped.

## Managed pack updates

Prism has no non-interactive “update modpack” CLI, so this plugin detects updates and hands install off to Prism:

1. On a managed pack’s game details page, use the **Check for update** / **Update in Prism** buttons (also under the game’s action menu and **Main menu → Prism Launcher**).
2. When a newer platform version id is found, Playnite shows a notification and the details status text updates. After **Update Game Library**, the game also gets an **Update Available** tag.
3. **Update in Prism** runs `prismlauncher --dir … --show "<instance>"`. Use Prism’s own update UI there, then refresh the Playnite library.

Modrinth and FTB need no API key. CurseForge update checks use the same API key as metadata.

**Theme / add-on compatibility:** the on-page **Check for update** / **Update in Prism** buttons are injected next to Playnite’s Play/More controls. Some themes or other extensions change that layout and can hide or break those buttons. Check/Update still work from the game context menu and **Main menu → Prism Launcher**. If the buttons are missing or the Play/More row looks wrong, please [open a bug report](https://github.com/Loneranger419/playnite-prism-library/issues/new?labels=bug) with your Playnite version, theme name, and any other extensions installed.

Themes can also host the control explicitly with `<ContentControl x:Name="PrismLibrary_PackUpdateButtons" />`.

## Distribution (Playnite add-on)

| File | Purpose |
|------|---------|
| `installer.yaml` | Playnite installer manifest (versions + `.pext` URLs) |
| `manifests/playnite-addon.yaml` | Draft entry for [PlayniteAddonDatabase](https://github.com/JosefNemec/PlayniteAddonDatabase) when plugin submissions reopen |
| `pack-release.ps1` | Build, pack `.pext`, optionally `gh release create` |

```powershell
# Build + pack only
.\pack-release.ps1

# Build, pack, and publish GitHub release v<version> from extension.yaml
.\pack-release.ps1 -Publish
```

See [Install](#install) for end-user `.pext` setup. AddonId: `PrismLibrary_b2aeb685-8587-490d-8b0f-5158b2df112f`

## License

MIT
