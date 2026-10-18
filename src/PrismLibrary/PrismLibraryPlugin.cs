using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PrismLibrary.Controllers;
using PrismLibrary.Models;
using PrismLibrary.Services;

namespace PrismLibrary
{
    public class PrismLibraryPlugin : LibraryPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly PrismLibrarySettingsViewModel settings;

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
            Properties = new LibraryPluginProperties
            {
                HasSettings = true
            };
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
                        enabled)
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

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new PrismLibrarySettingsView();
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
    }
}
