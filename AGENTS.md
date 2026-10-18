# Prism Library — agent notes

Project-specific guidance for this Playnite plugin. Follow global user rules for general process.

## Stack constraints

- Target `net462` only. Do not retarget to modern .NET; Playnite plugins require .NET Framework 4.6.2.
- Entry point: `PrismLibraryPlugin` (`LibraryPlugin`) with GUID `b2aeb685-8587-490d-8b0f-5158b2df112f`. Never change the GUID after release.
- Always launch instances through `prismlauncher.exe` with separate executable vs data-root settings.
- Playnite already ships Markdig ~0.18. Compile against that API and do **not** copy a newer Markdig.dll into the plugin output (causes `MissingMethodException` at runtime).

## Prism data

- Treat Prism files as read-only in v1. Do not write `instance.cfg`, `instgroups.json`, or delete instance folders.
- Playnite Install/Uninstall must only toggle `EnabledInstanceIds` in plugin settings.
- Managed pack updates: detect via platform APIs vs `ManagedPackVersionID`; install only by opening Prism (`--show`). Do not reimplement pack install/overwrite in the plugin.

## Verification

- Prefer fixture unit tests under `tests/PrismLibrary.Tests` for parsers and launch args.
- Use `.\build-and-test.ps1` as the single local verification entry point.
- Manual Playnite checks are still required for library import and launch UX.
