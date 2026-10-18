using System;
using System.Collections.Generic;
using System.Linq;
using Playnite.SDK.Models;
using PrismLibrary.Models;
using PrismLibrary.Services;

namespace PrismLibrary.Services.Metadata
{
    public static class PackMetadataMapper
    {
        public static GameMetadata ToGameMetadata(PackMetadataInfo info, PrismInstance instance = null)
        {
            if (info == null)
            {
                return null;
            }

            var description = DescriptionNormalizer.Normalize(info.Description, out var youtubeUrls);
            var metadata = new GameMetadata
            {
                Description = description,
                Tags = BuildTags(info, instance)
            };

            if (!string.IsNullOrWhiteSpace(info.CoverUrl))
            {
                metadata.CoverImage = new MetadataFile(info.CoverUrl);
            }

            if (!string.IsNullOrWhiteSpace(info.BackgroundUrl))
            {
                metadata.BackgroundImage = new MetadataFile(info.BackgroundUrl);
            }

            if (info.Authors != null && info.Authors.Count > 0)
            {
                metadata.Developers = info.Authors
                    .Where(a => !string.IsNullOrWhiteSpace(a))
                    .Select(a => (MetadataProperty)new MetadataNameProperty(a))
                    .ToHashSet();
            }

            var links = new List<Link>();
            if (info.Links != null)
            {
                links.AddRange(info.Links
                    .Where(l => !string.IsNullOrWhiteSpace(l.Url))
                    .Select(l => new Link(l.Name ?? "Link", l.Url)));
            }

            for (var i = 0; i < youtubeUrls.Count; i++)
            {
                var name = youtubeUrls.Count == 1 ? "YouTube" : "YouTube " + (i + 1);
                links.Add(new Link(name, youtubeUrls[i]));
            }

            metadata.Links = links
                .GroupBy(l => l.Url, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();

            if (metadata.Links.Count == 0)
            {
                metadata.Links = null;
            }

            return metadata;
        }

        public static HashSet<MetadataProperty> BuildTags(PackMetadataInfo info, PrismInstance instance)
        {
            var tags = new HashSet<MetadataProperty>();
            void Add(string value)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    tags.Add(new MetadataNameProperty(value.Trim()));
                }
            }

            Add("Minecraft");

            if (instance != null)
            {
                if (!string.IsNullOrWhiteSpace(instance.MinecraftVersion))
                {
                    Add("MC " + instance.MinecraftVersion);
                }

                if (instance.Loaders != null)
                {
                    foreach (var loader in instance.Loaders)
                    {
                        Add(loader);
                    }
                }

                var platform = PackPlatform.GetDisplayName(instance.PlatformKind);
                Add(platform);
            }

            if (!string.IsNullOrWhiteSpace(info?.PlatformDisplayName))
            {
                Add(info.PlatformDisplayName);
            }

            if (info?.Tags != null)
            {
                foreach (var tag in info.Tags)
                {
                    Add(tag);
                }
            }

            return tags;
        }
    }
}
