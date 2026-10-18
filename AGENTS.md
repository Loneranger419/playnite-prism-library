# Prism Library — agent notes

Project-specific guidance for this Playnite plugin. Follow global user rules for general process.

## Stack constraints

- Target `net462` only. Do not retarget to modern .NET; Playnite plugins require .NET Framework 4.6.2.
- Entry point: `PrismLibraryPlugin` (`LibraryPlugin`) with GUID `b2aeb685-8587-490d-8b0f-5158b2df112f`. Never change the GUID after release.
- Always launch instances through `prismlauncher.exe` with separate executable vs data-root settings.

## Prism data

- Treat Prism files as read-only in v1. Do not write `instance.cfg`, `instgroups.json`, or delete instance folders.
- Playnite Install/Uninstall must only toggle `EnabledInstanceIds` in plugin settings.

## Verification

- Prefer fixture unit tests under `tests/PrismLibrary.Tests` for parsers and launch args.
- Use `.\build-and-test.ps1` as the single local verification entry point.
- Manual Playnite checks are still required for library import and launch UX.
