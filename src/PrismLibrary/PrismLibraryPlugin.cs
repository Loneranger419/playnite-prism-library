using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PrismLibrary.Controllers;
using PrismLibrary.Models;
using PrismLibrary.Services;
using PrismLibrary.Services.Metadata;
using PrismLibrary.Views;

namespace PrismLibrary
{
    public class PrismLibraryPlugin : LibraryPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly PrismLibrarySettingsViewModel settings;
        private readonly PackUpdateService packUpdates;
        private readonly GameDetailsButtonHost detailsButtonHost;

        public override Guid Id { get; } = Guid.Parse("b2aeb685-8587-490d-8b0f-5158b2df112f");

        public override string Name => "Prism Launcher";

        public override string LibraryIcon => Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
            "icon.png");

        public override LibraryClient Client { get; }

        public PrismLibraryPlugin(IPlayniteAPI api) : base(api)
        {
            settings = new PrismLibrarySettingsViewModel(this);
            Client = new PrismLibraryClient(this);
            packUpdates = new PackUpdateService(GetPluginUserDataPath());
            detailsButtonHost = new GameDetailsButtonHost(this);
            Properties = new LibraryPluginProperties
            {
                HasSettings = true
            };

            AddCustomElementSupport(new AddCustomElementSupportArgs
            {
                SourceName = "PrismLibrary",
                ElementList = new List<string> { "PackUpdateButtons" }
            });
        }

        public PrismPathInfo GetResolvedPaths()
        {
            return PrismPaths.Resolve(settings.Settings.ExecutablePath, settings.Settings.DataRoot);
        }

        public string GetCurseForgeApiKey()
        {
            return settings.Settings.CurseForgeApiKey;
        }

        public PrismInstance FindInstance(string gameId)
        {
            var paths = GetResolvedPaths();
            if (!paths.IsValid || string.IsNullOrWhiteSpace(gameId))
            {
                return null;
            }

            return InstanceScanner.Scan(paths)
                .FirstOrDefault(i => string.Equals(i.Id, gameId, StringComparison.OrdinalIgnoreCase));
        }

        public override LibraryMetadataProvider GetMetadataDownloader()
        {
            return new PrismMetadataProvider(this);
        }

        public override IEnumerable<GameMetadata> GetGames(LibraryGetGamesArgs args)
        {
            var paths = GetResolvedPaths();
            if (!paths.IsValid)
            {
                logger.Warn("Prism Library paths are invalid. Configure executable and data root in extension settings.");
                PlayniteApi.Notifications.Add(new NotificationMessage(
                    "prism-library-paths",
                    "Prism Library: configure Prism executable and data root in Add-on settings.",
                    NotificationType.Error));
                return Enumerable.Empty<GameMetadata>();
            }

            var games = new List<GameMetadata>();
            foreach (var instance in InstanceScanner.Scan(paths))
            {
                var enabled = settings.Settings.IsInstanceEnabled(instance.Id);
                var tags = new HashSet<MetadataProperty> { new MetadataNameProperty("Minecraft") };
                if (!string.IsNullOrWhiteSpace(instance.MinecraftVersion))
                {
                    tags.Add(new MetadataNameProperty("MC " + instance.MinecraftVersion));
                }

                foreach (var loader in instance.Loaders)
                {
                    tags.Add(new MetadataNameProperty(loader));
                }

                var platformName = PackPlatform.GetDisplayName(instance.PlatformKind);
                if (!string.IsNullOrWhiteSpace(platformName))
                {
                    tags.Add(new MetadataNameProperty(platformName));
                }

                var isManagedPack = instance.HasManagedPackId &&
                                    instance.PlatformKind != PackPlatformKind.None &&
                                    instance.PlatformKind != PackPlatformKind.Unknown;
                var updateAvailable = packUpdates.HasCachedUpdate(instance);
                if (updateAvailable)
                {
                    tags.Add(new MetadataNameProperty(PackUpdateService.UpdateAvailableTag));
                }

                var categories = new HashSet<MetadataProperty>();
                if (!string.IsNullOrWhiteSpace(instance.Group))
                {
                    categories.Add(new MetadataNameProperty(instance.Group));
                }

                var links = new List<Link>();
                var projectUrl = PackPlatform.GetProjectUrl(instance.PlatformKind, instance.ManagedPackId);
                if (!string.IsNullOrWhiteSpace(projectUrl))
                {
                    links.Add(new Link(platformName ?? "Pack Page", projectUrl));
                }
                else if (!string.IsNullOrWhiteSpace(instance.ManagedPackUrl))
                {
                    links.Add(new Link("Pack URL", instance.ManagedPackUrl));
                }

                var version = !string.IsNullOrWhiteSpace(instance.ManagedPackVersionName)
                    ? instance.ManagedPackVersionName
                    : instance.MinecraftVersion;

                var metadata = new GameMetadata
                {
                    Name = instance.Name,
                    GameId = instance.Id,
                    IsInstalled = enabled,
                    InstallDirectory = instance.InstanceDirectory,
                    Playtime = instance.TotalTimePlayedSeconds,
                    LastActivity = instance.LastLaunch,
                    Version = version,
                    Platforms = new HashSet<MetadataProperty> { new MetadataNameProperty("PC") },
                    Tags = tags,
                    Categories = categories,
                    Links = links.Count > 0 ? links : null,
                    GameActions = LaunchBuilder.BuildActions(
                        paths.ExecutablePath,
                        paths.DataRoot,
                        instance.Id,
                        instance.InstanceDirectory,
                        instance.MinecraftDirectory,
                        settings.Settings.DefaultProfile,
                        enabled,
                        isManagedPack,
                        Id,
                        updateAvailable)
                };

                if (!string.IsNullOrWhiteSpace(instance.IconPath) && File.Exists(instance.IconPath))
                {
                    metadata.Icon = new MetadataFile(instance.IconPath);
                }

                games.Add(metadata);
            }

            return games;
        }

        public override IEnumerable<InstallController> GetInstallActions(GetInstallActionsArgs args)
        {
            if (args?.Game == null || args.Game.PluginId != Id)
            {
                yield break;
            }

            yield return new PrismInstallController(args.Game, EnableInstance, ResolveInstanceDirectory);
        }

        public override IEnumerable<UninstallController> GetUninstallActions(GetUninstallActionsArgs args)
        {
            if (args?.Game == null || args.Game.PluginId != Id)
            {
                yield break;
            }

            yield return new PrismUninstallController(args.Game, DisableInstance);
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            yield return new MainMenuItem
            {
                Description = "Check managed packs for updates",
                MenuSection = "@Prism Launcher",
                Action = _ => CheckAllManagedPackUpdates()
            };
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            var games = args?.Games?.Where(g => g != null && g.PluginId == Id).ToList();
            if (games == null || games.Count == 0)
            {
                yield break;
            }

            yield return new GameMenuItem
            {
                Description = "Check for pack update",
                MenuSection = "Prism Launcher",
                Action = a => CheckManagedPackUpdates(a.Games)
            };

            if (games.Count == 1)
            {
                var instance = FindInstance(games[0].GameId);
                if (instance != null && packUpdates.HasCachedUpdate(instance))
                {
                    yield return new GameMenuItem
                    {
                        Description = "Update in Prism",
                        MenuSection = "Prism Launcher",
                        Action = _ => OpenInstanceInPrism(instance.Id)
                    };
                }
            }
        }

        public override Control GetGameViewControl(GetGameViewControlArgs args)
        {
            if (args != null &&
                string.Equals(args.Name, "PackUpdateButtons", StringComparison.OrdinalIgnoreCase))
            {
                return new PackUpdateButtonsView(this);
            }

            return null;
        }

        public override void OnGameSelected(OnGameSelectedEventArgs args)
        {
            var game = args?.NewValue?.FirstOrDefault();
            detailsButtonHost.Sync(game);
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new PrismLibrarySettingsView();
        }

        public PackUpdateUiState GetPackUpdateUiState(Game game)
        {
            var state = new PackUpdateUiState();
            if (game == null || game.PluginId != Id)
            {
                return state;
            }

            var instance = FindInstance(game.GameId);
            if (!IsManagedPack(instance))
            {
                return state;
            }

            state.IsVisible = true;
            state.CanUpdate = packUpdates.HasCachedUpdate(instance);
            state.CanCheck = !state.CanUpdate;
            state.UpdateButtonLabel = "Update in Prism";

            var status = packUpdates.GetCachedStatus(instance.Id);
            if (state.CanUpdate)
            {
                state.StatusText = "Update available: " + (status?.Summary ?? "newer version found");
            }
            else if (status != null && !string.IsNullOrWhiteSpace(status.Error))
            {
                state.StatusText = status.Error;
            }
            else if (status != null)
            {
                state.StatusText = status.Summary;
            }
            else
            {
                state.StatusText = "Not checked yet";
            }

            return state;
        }

        public void CheckPackUpdateFromUi(string gameId)
        {
            var instance = FindInstance(gameId);
            if (!IsManagedPack(instance))
            {
                PlayniteApi.Dialogs.ShowMessage(
                    "This game is not a managed Modrinth, CurseForge, or FTB pack.",
                    "Prism Launcher");
                return;
            }

            PackUpdateStatus status = null;
            PlayniteApi.Dialogs.ActivateGlobalProgress(
                _ => { status = packUpdates.Check(instance, GetCurseForgeApiKey()); },
                new GlobalProgressOptions("Checking for pack update…", true)
                {
                    IsIndeterminate = true
                });

            if (status != null)
            {
                NotifyUpdateResults(new List<PackUpdateStatus> { status });
            }

            detailsButtonHost.Sync(PlayniteApi.MainView?.SelectedGames?.FirstOrDefault());
        }

        public void OpenUpdateInPrismFromUi(string gameId)
        {
            OpenInstanceInPrism(gameId);
        }

        public void CheckSelectedPackUpdateFromUi()
        {
            var game = PlayniteApi.MainView?.SelectedGames?.FirstOrDefault();
            if (game != null)
            {
                CheckPackUpdateFromUi(game.GameId);
            }
        }

        public void OpenSelectedUpdateInPrismFromUi()
        {
            var game = PlayniteApi.MainView?.SelectedGames?.FirstOrDefault();
            if (game != null)
            {
                OpenUpdateInPrismFromUi(game.GameId);
            }
        }

        private static bool IsManagedPack(PrismInstance instance)
        {
            return instance != null &&
                   instance.HasManagedPackId &&
                   instance.PlatformKind != PackPlatformKind.None &&
                   instance.PlatformKind != PackPlatformKind.Unknown;
        }

        private void EnableInstance(string instanceId)
        {
            settings.Settings.SetInstanceEnabled(instanceId, true);
            SavePluginSettings(settings.Settings);
        }

        private void DisableInstance(string instanceId)
        {
            settings.Settings.SetInstanceEnabled(instanceId, false);
            SavePluginSettings(settings.Settings);
        }

        private string ResolveInstanceDirectory(string instanceId)
        {
            return FindInstance(instanceId)?.InstanceDirectory;
        }

        private void CheckAllManagedPackUpdates()
        {
            var paths = GetResolvedPaths();
            if (!paths.IsValid)
            {
                PlayniteApi.Dialogs.ShowErrorMessage(
                    "Configure Prism executable and data root in Add-on settings first.",
                    "Prism Launcher");
                return;
            }

            var instances = InstanceScanner.Scan(paths)
                .Where(i => i.HasManagedPackId &&
                            i.PlatformKind != PackPlatformKind.None &&
                            i.PlatformKind != PackPlatformKind.Unknown)
                .ToList();

            if (instances.Count == 0)
            {
                PlayniteApi.Dialogs.ShowMessage("No managed Modrinth, CurseForge, or FTB packs were found.", "Prism Launcher");
                return;
            }

            NotifyUpdateResults(packUpdates.CheckAll(instances, GetCurseForgeApiKey()));
        }

        private void CheckManagedPackUpdates(IEnumerable<Game> games)
        {
            var instances = new List<PrismInstance>();
            foreach (var game in games ?? Enumerable.Empty<Game>())
            {
                if (game == null || game.PluginId != Id)
                {
                    continue;
                }

                var instance = FindInstance(game.GameId);
                if (instance != null && instance.HasManagedPackId)
                {
                    instances.Add(instance);
                }
            }

            if (instances.Count == 0)
            {
                PlayniteApi.Dialogs.ShowMessage(
                    "Selected games are not managed Modrinth, CurseForge, or FTB packs.",
                    "Prism Launcher");
                return;
            }

            NotifyUpdateResults(packUpdates.CheckAll(instances, GetCurseForgeApiKey()));
        }

        private void NotifyUpdateResults(IList<PackUpdateStatus> results)
        {
            if (results == null || results.Count == 0)
            {
                return;
            }

            var updates = results.Where(r => r.UpdateAvailable).ToList();
            var errors = results.Where(r => !string.IsNullOrWhiteSpace(r.Error)).ToList();

            if (updates.Count > 0)
            {
                var lines = updates.Select(u =>
                    (u.InstanceName ?? u.InstanceId) + ": " + u.Summary);
                PlayniteApi.Notifications.Add(new NotificationMessage(
                    "prism-library-pack-updates",
                    "Prism Library: " + updates.Count + " pack update(s) available.\n" +
                    string.Join("\n", lines.Take(8)) +
                    (updates.Count > 8 ? "\n…" : string.Empty) +
                    "\nUse Update in Prism, then Update Game Library.",
                    NotificationType.Info));
            }
            else if (errors.Count == results.Count)
            {
                PlayniteApi.Notifications.Add(new NotificationMessage(
                    "prism-library-pack-updates",
                    "Prism Library: pack update check failed (" + errors[0].Error + ").",
                    NotificationType.Error));
            }
            else
            {
                PlayniteApi.Notifications.Add(new NotificationMessage(
                    "prism-library-pack-updates",
                    "Prism Library: managed packs are up to date (" + results.Count + " checked).",
                    NotificationType.Info));
            }

            if (errors.Count > 0 && updates.Count > 0)
            {
                logger.Warn("Pack update check completed with " + errors.Count + " error(s).");
            }
        }

        private void OpenInstanceInPrism(string instanceId)
        {
            var paths = GetResolvedPaths();
            if (!paths.IsValid || string.IsNullOrWhiteSpace(instanceId))
            {
                PlayniteApi.Dialogs.ShowErrorMessage(
                    "Configure Prism executable and data root in Add-on settings first.",
                    "Prism Launcher");
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = paths.ExecutablePath,
                Arguments = LaunchBuilder.BuildShowArguments(paths.DataRoot, instanceId),
                UseShellExecute = true
            });
        }
    }
}
