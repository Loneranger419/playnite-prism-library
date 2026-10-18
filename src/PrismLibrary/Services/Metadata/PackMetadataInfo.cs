using System.Collections.Generic;

namespace PrismLibrary.Services.Metadata
{
    public sealed class PackMetadataInfo
    {
        public string PlatformDisplayName { get; set; }
        public string Description { get; set; }
        public string CoverUrl { get; set; }
        public string BackgroundUrl { get; set; }
        public string ProjectUrl { get; set; }
        public List<string> Authors { get; set; } = new List<string>();
        public List<string> Tags { get; set; } = new List<string>();
        public List<PackMetadataLink> Links { get; set; } = new List<PackMetadataLink>();
    }

    public sealed class PackMetadataLink
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }
}
