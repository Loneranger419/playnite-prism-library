using System;
using System.Collections.Generic;
using Playnite.SDK;
using PrismLibrary.Models;

namespace PrismLibrary.Services.Metadata
{
    public sealed class PackMetadataService : IDisposable
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly HttpJsonClient http = new HttpJsonClient();
        private readonly Dictionary<string, PackMetadataInfo> cache =
            new Dictionary<string, PackMetadataInfo>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<PackPlatformKind, IPackMetadataClient> clients;
        private bool notifiedMissingCurseForgeKey;

        public PackMetadataService()
        {
            clients = new Dictionary<PackPlatformKind, IPackMetadataClient>
            {
                [PackPlatformKind.Modrinth] = new ModrinthMetadataClient(http),
                [PackPlatformKind.CurseForge] = new CurseForgeMetadataClient(http),
                [PackPlatformKind.Ftb] = new FtbMetadataClient(http)
            };
        }

        public PackMetadataInfo GetMetadata(PrismInstance instance, string curseForgeApiKey, Action notifyMissingCurseForgeKey = null)
        {
            if (instance == null || !instance.HasManagedPackId)
            {
                return null;
            }

            var kind = instance.PlatformKind;
            if (kind == PackPlatformKind.None || kind == PackPlatformKind.Unknown)
            {
                return null;
            }

            if (kind == PackPlatformKind.CurseForge && string.IsNullOrWhiteSpace(curseForgeApiKey))
            {
                if (!notifiedMissingCurseForgeKey)
                {
                    notifiedMissingCurseForgeKey = true;
                    notifyMissingCurseForgeKey?.Invoke();
                    logger.Warn("CurseForge API key missing; rich metadata skipped for CurseForge packs.");
                }

                return null;
            }

            var cacheKey = kind + ":" + instance.ManagedPackId;
            if (cache.TryGetValue(cacheKey, out var cached))
            {
                return cached;
            }

            if (!clients.TryGetValue(kind, out var client))
            {
                return null;
            }

            try
            {
                var info = client.Fetch(instance, curseForgeApiKey);
                cache[cacheKey] = info;
                return info;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed fetching pack metadata for " + cacheKey);
                cache[cacheKey] = null;
                return null;
            }
        }

        public void Dispose()
        {
            http.Dispose();
        }
    }
}
