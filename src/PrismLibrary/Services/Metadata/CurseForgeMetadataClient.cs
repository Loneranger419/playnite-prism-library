using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Playnite.SDK;
using PrismLibrary.Models;

namespace PrismLibrary.Services.Metadata
{
    public sealed class CurseForgeMetadataClient : IPackMetadataClient
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly HttpJsonClient http;
        private bool missingKeyNotified;

        public CurseForgeMetadataClient(HttpJsonClient http)
        {
            this.http = http;
        }

        public PackPlatformKind Platform => PackPlatformKind.CurseForge;

        public PackMetadataInfo Fetch(PrismInstance instance, string curseForgeApiKey)
        {
            if (string.IsNullOrWhiteSpace(curseForgeApiKey))
            {
                if (!missingKeyNotified)
                {
                    missingKeyNotified = true;
                    logger.Warn("CurseForge API key is not configured; skipping rich CurseForge metadata.");
                }

                return null;
            }

            var headers = new Dictionary<string, string>
            {
                ["x-api-key"] = curseForgeApiKey.Trim()
            };

            var modJson = http.GetString("https://api.curseforge.com/v1/mods/" + instance.ManagedPackId, headers);
            var descriptionJson = http.GetString(
                "https://api.curseforge.com/v1/mods/" + instance.ManagedPackId + "/description",
                headers);

            return Parse(modJson, descriptionJson, instance.ManagedPackId);
        }

        public PackVersionInfo FetchLatestVersion(PrismInstance instance, string curseForgeApiKey)
        {
            if (instance == null || string.IsNullOrWhiteSpace(instance.ManagedPackId))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(curseForgeApiKey))
            {
                if (!missingKeyNotified)
                {
                    missingKeyNotified = true;
                    logger.Warn("CurseForge API key is not configured; skipping CurseForge pack update checks.");
                }

                return null;
            }

            var headers = new Dictionary<string, string>
            {
                ["x-api-key"] = curseForgeApiKey.Trim()
            };

            var url = "https://api.curseforge.com/v1/mods/" + Uri.EscapeDataString(instance.ManagedPackId) +
                      "/files?pageSize=50";
            if (!string.IsNullOrWhiteSpace(instance.MinecraftVersion))
            {
                url += "&gameVersion=" + Uri.EscapeDataString(instance.MinecraftVersion.Trim());
            }

            var latest = ParseLatestVersion(http.GetString(url, headers));
            if (latest != null || string.IsNullOrWhiteSpace(instance.MinecraftVersion))
            {
                return latest;
            }

            // Fallback without MC filter when the filtered page is empty.
            url = "https://api.curseforge.com/v1/mods/" + Uri.EscapeDataString(instance.ManagedPackId) +
                  "/files?pageSize=50";
            return ParseLatestVersion(http.GetString(url, headers));
        }

        public static PackVersionInfo ParseLatestVersion(string filesJson)
        {
            if (string.IsNullOrWhiteSpace(filesJson))
            {
                return null;
            }

            JObject root;
            try
            {
                root = JObject.Parse(filesJson);
            }
            catch
            {
                return null;
            }

            var data = root["data"] as JArray;
            if (data == null || data.Count == 0)
            {
                return null;
            }

            var files = data.OfType<JObject>()
                .Where(f => f.Value<bool?>("isAvailable") != false)
                .ToList();
            if (files.Count == 0)
            {
                return null;
            }

            // releaseType: 1 = Release, 2 = Beta, 3 = Alpha
            var preferred = files.Where(f => f.Value<int?>("releaseType") == 1).ToList();
            var pool = preferred.Count > 0 ? preferred : files;

            var chosen = pool
                .OrderByDescending(f =>
                {
                    if (DateTime.TryParse(f.Value<string>("fileDate"), null,
                            System.Globalization.DateTimeStyles.RoundtripKind, out var date))
                    {
                        return date.ToUniversalTime();
                    }

                    return DateTime.MinValue;
                })
                .ThenByDescending(f => f.Value<long?>("id") ?? 0)
                .FirstOrDefault();

            if (chosen == null)
            {
                return null;
            }

            var id = chosen.Value<long?>("id");
            if (id == null)
            {
                return null;
            }

            DateTime? published = null;
            if (DateTime.TryParse(chosen.Value<string>("fileDate"), null,
                    System.Globalization.DateTimeStyles.RoundtripKind, out var parsed))
            {
                published = parsed.ToUniversalTime();
            }

            return new PackVersionInfo
            {
                VersionId = id.Value.ToString(),
                VersionName = FirstNonEmpty(chosen.Value<string>("displayName"), chosen.Value<string>("fileName")),
                PublishedUtc = published
            };
        }

        private static string FirstNonEmpty(params string[] values)
        {
            return values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
        }

        public static PackMetadataInfo Parse(string modJson, string descriptionJson, string packId)
        {
            if (string.IsNullOrWhiteSpace(modJson))
            {
                return null;
            }

            JObject root;
            try
            {
                root = JObject.Parse(modJson);
            }
            catch
            {
                return null;
            }

            var data = root["data"] as JObject ?? root;
            var links = data["links"] as JObject;
            var website = links?.Value<string>("websiteUrl")
                          ?? PackPlatform.GetProjectUrl(PackPlatformKind.CurseForge, packId);

            var description = ExtractDescription(descriptionJson);
            if (string.IsNullOrWhiteSpace(description))
            {
                description = data.Value<string>("summary");
            }

            var info = new PackMetadataInfo
            {
                PlatformDisplayName = "CurseForge",
                Description = description,
                CoverUrl = data["logo"]?.Value<string>("thumbnailUrl") ?? data["logo"]?.Value<string>("url"),
                ProjectUrl = website
            };

            var screenshots = data["screenshots"] as JArray;
            var firstShot = screenshots?.OfType<JObject>().FirstOrDefault();
            info.BackgroundUrl = firstShot?.Value<string>("url") ?? firstShot?.Value<string>("thumbnailUrl");

            var authors = data["authors"] as JArray;
            if (authors != null)
            {
                foreach (var author in authors.OfType<JObject>())
                {
                    var name = author.Value<string>("name");
                    if (!string.IsNullOrWhiteSpace(name) && !info.Authors.Contains(name))
                    {
                        info.Authors.Add(name);
                    }
                }
            }

            var categories = data["categories"] as JArray;
            if (categories != null)
            {
                foreach (var category in categories.OfType<JObject>())
                {
                    var name = category.Value<string>("name");
                    if (string.IsNullOrWhiteSpace(name) ||
                        string.Equals(name, "Modpacks", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (!info.Tags.Any(t => string.Equals(t, name, StringComparison.OrdinalIgnoreCase)))
                    {
                        info.Tags.Add(name);
                    }
                }
            }

            AddLink(info, "Website", website);
            AddLink(info, "Wiki", links?.Value<string>("wikiUrl"));
            AddLink(info, "Issues", links?.Value<string>("issuesUrl"));
            AddLink(info, "Source", links?.Value<string>("sourceUrl"));

            return info;
        }

        private static string ExtractDescription(string descriptionJson)
        {
            if (string.IsNullOrWhiteSpace(descriptionJson))
            {
                return null;
            }

            try
            {
                var root = JObject.Parse(descriptionJson);
                var data = root["data"];
                if (data == null)
                {
                    return null;
                }

                if (data.Type == JTokenType.String)
                {
                    return data.Value<string>();
                }

                return data.Value<string>("description") ?? data.ToString();
            }
            catch
            {
                return null;
            }
        }

        private static void AddLink(PackMetadataInfo info, string name, string url)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                info.Links.Add(new PackMetadataLink { Name = name, Url = url });
            }
        }
    }
}
