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

## License

MIT
