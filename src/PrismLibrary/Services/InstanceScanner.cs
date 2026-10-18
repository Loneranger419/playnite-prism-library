using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PrismLibrary.Models;

namespace PrismLibrary.Services
{
    public static class InstanceScanner
    {
        public static IReadOnlyList<PrismInstance> Scan(PrismPathInfo paths)
        {
            var results = new List<PrismInstance>();
            if (paths == null || string.IsNullOrWhiteSpace(paths.InstancesDirectory) || !Directory.Exists(paths.InstancesDirectory))
            {
                return results;
            }

            var groups = GroupReader.ReadFile(Path.Combine(paths.InstancesDirectory, "instgroups.json"));

            foreach (var directory in Directory.EnumerateDirectories(paths.InstancesDirectory))
            {
                var cfgPath = Path.Combine(directory, "instance.cfg");
                if (!File.Exists(cfgPath))
                {
                    continue;
                }

                var id = Path.GetFileName(directory);
                if (string.IsNullOrWhiteSpace(id))
                {
                    continue;
                }

                var cfg = InstanceCfgReader.ReadFile(cfgPath);
                var pack = MmcPackReader.ReadFile(Path.Combine(directory, "mmc-pack.json"));
                var minecraftDir = ResolveMinecraftDirectory(directory);

                groups.TryGetValue(id, out var group);

                results.Add(new PrismInstance
                {
                    Id = id,
                    Name = string.IsNullOrWhiteSpace(cfg.Name) ? id : cfg.Name,
                    InstanceDirectory = directory,
                    MinecraftDirectory = minecraftDir,
                    IconKey = cfg.IconKey,
                    IconPath = IconResolver.Resolve(paths.IconsDirectory, directory, cfg.IconKey),
                    MinecraftVersion = pack.MinecraftVersion,
                    Loaders = pack.Loaders?.ToList() ?? new List<string>(),
                    Group = group,
                    TotalTimePlayedSeconds = cfg.TotalTimePlayedSeconds,
                    LastLaunch = cfg.LastLaunch,
                    ManagedPack = cfg.ManagedPack,
                    ManagedPackType = cfg.ManagedPackType,
                    ManagedPackId = cfg.ManagedPackId,
                    ManagedPackName = cfg.ManagedPackName,
                    ManagedPackVersionId = cfg.ManagedPackVersionId,
                    ManagedPackVersionName = cfg.ManagedPackVersionName,
                    ManagedPackUrl = cfg.ManagedPackUrl
                });
            }

            return results.OrderBy(i => i.Name, StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static string ResolveMinecraftDirectory(string instanceDirectory)
        {
            var hidden = Path.Combine(instanceDirectory, ".minecraft");
            if (Directory.Exists(hidden))
            {
                return hidden;
            }

            var visible = Path.Combine(instanceDirectory, "minecraft");
            return Directory.Exists(visible) ? visible : null;
        }
    }
}
