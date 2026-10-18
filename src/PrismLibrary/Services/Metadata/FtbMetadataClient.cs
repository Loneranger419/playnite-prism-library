using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using PrismLibrary.Models;

namespace PrismLibrary.Services.Metadata
{
    public sealed class FtbMetadataClient : IPackMetadataClient
    {
        private readonly HttpJsonClient http;

        public FtbMetadataClient(HttpJsonClient http)
        {
            this.http = http;
        }

        public PackPlatformKind Platform => PackPlatformKind.Ftb;

        public PackMetadataInfo Fetch(PrismInstance instance, string curseForgeApiKey)
        {
            return Parse(
                http.GetString("https://api.feed-the-beast.com/v1/modpacks/public/modpack/" + instance.ManagedPackId),
                instance.ManagedPackId);
        }

        public static PackMetadataInfo Parse(string json, string packId)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            JObject root;
            try
            {
                root = JObject.Parse(json);
            }
            catch
            {
                return null;
            }

            var slug = root.Value<string>("slug");
            var description = FirstNonEmpty(root.Value<string>("description"), root.Value<string>("synopsis"));
            var info = new PackMetadataInfo
            {
                PlatformDisplayName = "FTB",
                Description = description,
                ProjectUrl = PackPlatform.GetProjectUrl(PackPlatformKind.Ftb, packId, slug)
            };

            var arts = root["art"] as JArray;
            if (arts != null)
            {
                foreach (var art in arts.OfType<JObject>())
                {
                    var type = art.Value<string>("type");
                    var url = art.Value<string>("url");
                    if (string.IsNullOrWhiteSpace(url))
                    {
                        continue;
                    }

                    if (string.Equals(type, "square", StringComparison.OrdinalIgnoreCase) &&
                        string.IsNullOrWhiteSpace(info.CoverUrl))
                    {
                        info.CoverUrl = url;
                    }
                    else if (string.Equals(type, "splash", StringComparison.OrdinalIgnoreCase) &&
                             string.IsNullOrWhiteSpace(info.BackgroundUrl))
                    {
                        info.BackgroundUrl = url;
                    }
                }
            }

            var authors = root["authors"] as JArray;
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

            var tags = root["tags"] as JArray;
            if (tags != null)
            {
                foreach (var tag in tags.OfType<JObject>())
                {
                    var name = tag.Value<string>("name");
                    if (!string.IsNullOrWhiteSpace(name) &&
                        !info.Tags.Any(t => string.Equals(t, name, StringComparison.OrdinalIgnoreCase)))
                    {
                        info.Tags.Add(name);
                    }
                }
            }

            var links = root["links"] as JArray;
            if (links != null)
            {
                foreach (var link in links.OfType<JObject>())
                {
                    var name = link.Value<string>("name") ?? "Link";
                    var url = link.Value<string>("link") ?? link.Value<string>("url");
                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        info.Links.Add(new PackMetadataLink { Name = name, Url = url });
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(info.ProjectUrl))
            {
                info.Links.Insert(0, new PackMetadataLink { Name = "FTB", Url = info.ProjectUrl });
            }

            return info;
        }

        private static string FirstNonEmpty(params string[] values)
        {
            return values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
        }
    }
}
