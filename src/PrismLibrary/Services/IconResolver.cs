using System;
using System.IO;

namespace PrismLibrary.Services
{
    public static class IconResolver
    {
        public static string Resolve(string iconsDirectory, string instanceDirectory, string iconKey)
        {
            if (!string.IsNullOrWhiteSpace(iconKey) &&
                !string.Equals(iconKey, "default", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(iconsDirectory) &&
                Directory.Exists(iconsDirectory))
            {
                foreach (var extension in new[] { ".png", ".ico", ".jpg", ".jpeg", ".webp" })
                {
                    var candidate = Path.Combine(iconsDirectory, iconKey + extension);
                    if (File.Exists(candidate))
                    {
                        return candidate;
                    }
                }

                // Some icons are stored without needing the key as filename with spaces intact.
                var wildcardMatches = Directory.GetFiles(iconsDirectory, iconKey + ".*");
                if (wildcardMatches.Length > 0)
                {
                    return wildcardMatches[0];
                }
            }

            if (!string.IsNullOrWhiteSpace(instanceDirectory) && Directory.Exists(instanceDirectory))
            {
                foreach (var relative in new[]
                {
                    "icon.png",
                    Path.Combine(".minecraft", "icon.png"),
                    Path.Combine("minecraft", "icon.png")
                })
                {
                    var candidate = Path.Combine(instanceDirectory, relative);
                    if (File.Exists(candidate))
                    {
                        return candidate;
                    }
                }
            }

            return null;
        }
    }
}
