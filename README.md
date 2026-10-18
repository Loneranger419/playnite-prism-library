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

## Development install

1. Build:

   ```powershell
   .\build-and-test.ps1
   ```

2. In Playnite: **Settings → For developers → External extensions** → add:

   `src\PrismLibrary\bin\Release\net462`

3. Restart Playnite, open **Add-on settings → Libraries → Prism Launcher**, set paths (or **Detect defaults**), enable instances, then **Update Game Library**.

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

Themes can also host the control explicitly with `<ContentControl x:Name="PrismLibrary_PackUpdateButtons" />`.

## License

MIT
