using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace PrismLibrary.Services
{
    public sealed class MmcPackData
    {
        public string MinecraftVersion { get; set; }
        public List<string> Loaders { get; set; } = new List<string>();
    }

    public static class MmcPackReader
    {
        private static readonly Dictionary<string, string> LoaderNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["net.minecraftforge"] = "Forge",
            ["net.neoforged"] = "NeoForge",
            ["net.fabricmc.fabric-loader"] = "Fabric",
            ["org.quiltmc.quilt-loader"] = "Quilt",
            ["com.mumfrey.liteloader"] = "LiteLoader"
        };

        public static MmcPackData ReadFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return new MmcPackData();
            }

            return Read(File.ReadAllText(path));
        }

        public static MmcPackData Read(string json)
        {
            var result = new MmcPackData();
            if (string.IsNullOrWhiteSpace(json))
            {
                return result;
            }

            JObject root;
            try
            {
                root = JObject.Parse(json);
            }
            catch
            {
                return result;
            }

            var components = root["components"] as JArray;
            if (components == null)
            {
                return result;
            }

            foreach (var component in components.OfType<JObject>())
            {
                var uid = component.Value<string>("uid") ?? string.Empty;
                var version = component.Value<string>("version");

                if (string.Equals(uid, "net.minecraft", StringComparison.OrdinalIgnoreCase))
                {
                    result.MinecraftVersion = version;
                    continue;
                }

                if (LoaderNames.TryGetValue(uid, out var loaderName))
                {
                    if (!result.Loaders.Contains(loaderName))
                    {
                        result.Loaders.Add(loaderName);
                    }
                }
            }

            return result;
        }
    }
}
