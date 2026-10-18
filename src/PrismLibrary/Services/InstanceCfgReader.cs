using System;
using System.Globalization;

namespace PrismLibrary.Services
{
    public sealed class InstanceCfgData
    {
        public string Name { get; set; }
        public string IconKey { get; set; }
        public ulong TotalTimePlayedSeconds { get; set; }
        public DateTime? LastLaunch { get; set; }
        public bool ManagedPack { get; set; }
        public string ManagedPackType { get; set; }
        public string ManagedPackId { get; set; }
        public string ManagedPackName { get; set; }
        public string ManagedPackVersionId { get; set; }
        public string ManagedPackVersionName { get; set; }
        public string ManagedPackUrl { get; set; }
    }

    public static class InstanceCfgReader
    {
        public static InstanceCfgData ReadFile(string path)
        {
            return Read(IniKeyValueParser.ParseFile(path));
        }

        public static InstanceCfgData Read(string content)
        {
            return Read(IniKeyValueParser.Parse(content));
        }

        public static InstanceCfgData Read(System.Collections.Generic.Dictionary<string, string> values)
        {
            var data = new InstanceCfgData
            {
                Name = Get(values, "name"),
                IconKey = Get(values, "iconKey"),
                ManagedPackType = Get(values, "ManagedPackType"),
                ManagedPackId = Get(values, "ManagedPackID"),
                ManagedPackName = Get(values, "ManagedPackName"),
                ManagedPackVersionId = Get(values, "ManagedPackVersionID"),
                ManagedPackVersionName = Get(values, "ManagedPackVersionName"),
                ManagedPackUrl = Get(values, "ManagedPackURL")
            };

            data.ManagedPack = ParseBool(Get(values, "ManagedPack"));

            if (TryParseInt64(Get(values, "totalTimePlayed"), out var played) && played > 0)
            {
                data.TotalTimePlayedSeconds = (ulong)played;
            }

            if (TryParseInt64(Get(values, "lastLaunchTime"), out var lastLaunchMs) && lastLaunchMs > 0)
            {
                data.LastLaunch = DateTimeOffset.FromUnixTimeMilliseconds(lastLaunchMs).LocalDateTime;
            }

            return data;
        }

        private static string Get(System.Collections.Generic.Dictionary<string, string> values, string key)
        {
            return values != null && values.TryGetValue(key, out var value) ? value : null;
        }

        private static bool ParseBool(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "1", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryParseInt64(string value, out long result)
        {
            return long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
        }
    }
}
