using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Playnite.SDK;
using PrismLibrary.Models;

namespace PrismLibrary.Services.Metadata
{
    public sealed class PackUpdateService : IDisposable
    {
        public const string UpdateAvailableTag = "Update Available";

        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly HttpJsonClient http = new HttpJsonClient();
        private readonly ModrinthMetadataClient modrinth;
        private readonly CurseForgeMetadataClient curseForge;
        private readonly FtbMetadataClient ftb;
        private readonly string cachePath;
        private readonly object gate = new object();
        private Dictionary<string, PackUpdateStatus> cache =
            new Dictionary<string, PackUpdateStatus>(StringComparer.OrdinalIgnoreCase);

        public PackUpdateService(string cacheDirectory)
        {
            modrinth = new ModrinthMetadataClient(http);
            curseForge = new CurseForgeMetadataClient(http);
            ftb = new FtbMetadataClient(http);
            cachePath = Path.Combine(cacheDirectory ?? Path.GetTempPath(), "pack-updates.json");
            LoadCache();
        }

        public PackUpdateStatus GetCachedStatus(string instanceId)
        {
            if (string.IsNullOrWhiteSpace(instanceId))
            {
                return null;
            }

            lock (gate)
            {
                return cache.TryGetValue(instanceId, out var status) ? status : null;
            }
        }

        public bool HasCachedUpdate(PrismInstance instance)
        {
            if (instance == null || !instance.HasManagedPackId)
            {
                return false;
            }

            var status = GetCachedStatus(instance.Id);
            if (status == null || !status.UpdateAvailable)
            {
                return false;
            }

            // Drop stale "update available" once Prism's installed version id matches the cached latest.
            if (!string.IsNullOrWhiteSpace(instance.ManagedPackVersionId) &&
                !string.IsNullOrWhiteSpace(status.LatestVersionId) &&
                string.Equals(instance.ManagedPackVersionId, status.LatestVersionId, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        public PackUpdateStatus Check(PrismInstance instance, string curseForgeApiKey)
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

            var status = new PackUpdateStatus
            {
                InstanceId = instance.Id,
                InstanceName = instance.Name,
                PackId = instance.ManagedPackId,
                Platform = PackPlatform.GetDisplayName(kind),
                CurrentVersionId = instance.ManagedPackVersionId,
                CurrentVersionName = instance.ManagedPackVersionName,
                CheckedAtUtc = DateTime.UtcNow
            };

            try
            {
                PackVersionInfo latest = null;
                switch (kind)
                {
                    case PackPlatformKind.Modrinth:
                        latest = modrinth.FetchLatestVersion(instance);
                        break;
                    case PackPlatformKind.CurseForge:
                        latest = curseForge.FetchLatestVersion(instance, curseForgeApiKey);
                        break;
                    case PackPlatformKind.Ftb:
                        latest = ftb.FetchLatestVersion(instance);
                        break;
                }

                if (latest == null || string.IsNullOrWhiteSpace(latest.VersionId))
                {
                    status.Error = kind == PackPlatformKind.CurseForge && string.IsNullOrWhiteSpace(curseForgeApiKey)
                        ? "CurseForge API key required"
                        : "Could not determine latest pack version";
                }
                else
                {
                    status.LatestVersionId = latest.VersionId;
                    status.LatestVersionName = latest.VersionName;
                    status.UpdateAvailable =
                        !string.IsNullOrWhiteSpace(status.CurrentVersionId) &&
                        !string.Equals(status.CurrentVersionId, status.LatestVersionId, StringComparison.OrdinalIgnoreCase);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Pack update check failed for " + instance.Id);
                status.Error = "Update check failed";
            }

            lock (gate)
            {
                cache[instance.Id] = status;
            }

            SaveCache();
            return status;
        }

        public List<PackUpdateStatus> CheckAll(IEnumerable<PrismInstance> instances, string curseForgeApiKey)
        {
            var results = new List<PackUpdateStatus>();
            if (instances == null)
            {
                return results;
            }

            foreach (var instance in instances.Where(i => i != null && i.HasManagedPackId))
            {
                var kind = instance.PlatformKind;
                if (kind == PackPlatformKind.None || kind == PackPlatformKind.Unknown)
                {
                    continue;
                }

                var status = Check(instance, curseForgeApiKey);
                if (status != null)
                {
                    results.Add(status);
                }
            }

            return results;
        }

        public void Dispose()
        {
            http.Dispose();
        }

        private void LoadCache()
        {
            try
            {
                if (!File.Exists(cachePath))
                {
                    return;
                }

                var json = File.ReadAllText(cachePath);
                var entries = JsonConvert.DeserializeObject<List<PackUpdateStatus>>(json);
                if (entries == null)
                {
                    return;
                }

                lock (gate)
                {
                    cache = entries
                        .Where(e => e != null && !string.IsNullOrWhiteSpace(e.InstanceId))
                        .GroupBy(e => e.InstanceId, StringComparer.OrdinalIgnoreCase)
                        .ToDictionary(g => g.Key, g => g.Last(), StringComparer.OrdinalIgnoreCase);
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "Failed loading pack update cache from " + cachePath);
            }
        }

        private void SaveCache()
        {
            try
            {
                var directory = Path.GetDirectoryName(cachePath);
                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                List<PackUpdateStatus> entries;
                lock (gate)
                {
                    entries = cache.Values.ToList();
                }

                File.WriteAllText(cachePath, JsonConvert.SerializeObject(entries, Formatting.Indented));
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "Failed saving pack update cache to " + cachePath);
            }
        }
    }
}
