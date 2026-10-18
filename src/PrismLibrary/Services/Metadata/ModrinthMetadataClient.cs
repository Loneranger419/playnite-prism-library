using System.Linq;
using Newtonsoft.Json.Linq;
using PrismLibrary.Models;

namespace PrismLibrary.Services.Metadata
{
    public sealed class ModrinthMetadataClient : IPackMetadataClient
    {
        private readonly HttpJsonClient http;

        public ModrinthMetadataClient(HttpJsonClient http)
        {
            this.http = http;
        }

        public PackPlatformKind Platform => PackPlatformKind.Modrinth;

        public PackMetadataInfo Fetch(PrismInstance instance, string curseForgeApiKey)
        {
            return Parse(http.GetString("https://api.modrinth.com/v2/project/" + instance.ManagedPackId), instance.ManagedPackId);
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
            var description = FirstNonEmpty(root.Value<string>("body"), root.Value<string>("description"));
            var info = new PackMetadataInfo
            {
                PlatformDisplayName = "Modrinth",
                Description = description,
                CoverUrl = root.Value<string>("icon_url"),
                ProjectUrl = PackPlatform.GetProjectUrl(PackPlatformKind.Modrinth, packId, slug)
            };

            var gallery = root["gallery"] as JArray;
            var firstGallery = gallery?.OfType<JObject>().FirstOrDefault();
            info.BackgroundUrl = firstGallery?.Value<string>("url");

            var author = root.Value<string>("author");
            if (!string.IsNullOrWhiteSpace(author))
            {
                info.Authors.Add(author);
            }

            var categories = root["categories"] as JArray;
            if (categories != null)
            {
                foreach (var category in categories)
                {
                    var name = category?.Type == JTokenType.String
                        ? category.Value<string>()
                        : category?.Value<string>("name");
                    AddTag(info, FormatCategory(name));
                }
            }

            AddLink(info, "Source", root.Value<string>("source_url"));
            AddLink(info, "Issues", root.Value<string>("issues_url"));
            AddLink(info, "Wiki", root.Value<string>("wiki_url"));
            AddLink(info, "Discord", root.Value<string>("discord_url"));
            AddLink(info, "Modrinth", info.ProjectUrl);

            return info;
        }

        private static void AddTag(PackMetadataInfo info, string tag)
        {
            if (!string.IsNullOrWhiteSpace(tag) &&
                !info.Tags.Any(t => string.Equals(t, tag, System.StringComparison.OrdinalIgnoreCase)))
            {
                info.Tags.Add(tag);
            }
        }

        private static string FormatCategory(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            value = value.Replace('-', ' ').Replace('_', ' ').Trim();
            if (value.Length == 0)
            {
                return null;
            }

            return char.ToUpperInvariant(value[0]) + value.Substring(1);
        }

        private static void AddLink(PackMetadataInfo info, string name, string url)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                info.Links.Add(new PackMetadataLink { Name = name, Url = url });
            }
        }

        private static string FirstNonEmpty(params string[] values)
        {
            return values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
        }
    }
}
