using System;
using Playnite.SDK;
using Playnite.SDK.Models;
using PrismLibrary.Services;
using PrismLibrary.Services.Metadata;

namespace PrismLibrary
{
    public sealed class PrismMetadataProvider : LibraryMetadataProvider
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly PrismLibraryPlugin plugin;
        private readonly PackMetadataService metadataService = new PackMetadataService();

        public PrismMetadataProvider(PrismLibraryPlugin plugin)
        {
            this.plugin = plugin;
        }

        public override GameMetadata GetMetadata(Game game)
        {
            try
            {
                if (game == null || game.PluginId != plugin.Id)
                {
                    return null;
                }

                var instance = plugin.FindInstance(game.GameId);
                if (instance == null || !instance.HasManagedPackId)
                {
                    return null;
                }

                var info = metadataService.GetMetadata(
                    instance,
                    plugin.GetCurseForgeApiKey(),
                    () => plugin.PlayniteApi.Notifications.Add(new NotificationMessage(
                        "prism-library-cf-key",
                        "Prism Library: add a CurseForge API key in extension settings to download CurseForge pack metadata. Modrinth and FTB do not need a key.",
                        NotificationType.Info)));

                return PackMetadataMapper.ToGameMetadata(info, instance);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Prism metadata provider failed for " + game?.GameId);
                return null;
            }
        }

        public override void Dispose()
        {
            metadataService.Dispose();
            base.Dispose();
        }
    }
}
