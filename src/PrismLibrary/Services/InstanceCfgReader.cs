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
                IconKey = Get(values, "iconKey")
            };

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

        private static bool TryParseInt64(string value, out long result)
        {
            return long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
        }
    }
}
