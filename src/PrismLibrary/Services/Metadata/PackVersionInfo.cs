using System;

namespace PrismLibrary.Services.Metadata
{
    public sealed class PackVersionInfo
    {
        public string VersionId { get; set; }
        public string VersionName { get; set; }
        public DateTime? PublishedUtc { get; set; }
    }
}
