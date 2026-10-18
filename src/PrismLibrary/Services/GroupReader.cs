using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace PrismLibrary.Services
{
    public static class GroupReader
    {
        /// <summary>
        /// Returns a map of instance ID to group name.
        /// </summary>
        public static Dictionary<string, string> ReadFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            return Read(File.ReadAllText(path));
        }

        public static Dictionary<string, string> Read(string json)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
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

            var groups = root["groups"] as JObject;
            if (groups == null)
            {
                return result;
            }

            foreach (var groupProperty in groups.Properties())
            {
                var groupName = groupProperty.Name;
                if (string.IsNullOrWhiteSpace(groupName))
                {
                    continue;
                }

                var groupObj = groupProperty.Value as JObject;
                var instances = groupObj?["instances"] as JArray;
                if (instances == null)
                {
                    continue;
                }

                foreach (var instanceId in instances.Select(token => token?.ToString()).Where(id => !string.IsNullOrWhiteSpace(id)))
                {
                    result[instanceId] = groupName;
                }
            }

            return result;
        }
    }
}
