using PrismLibrary.Models;

namespace PrismLibrary.Services.Metadata
{
    public interface IPackMetadataClient
    {
        PackPlatformKind Platform { get; }
        PackMetadataInfo Fetch(PrismInstance instance, string curseForgeApiKey);
    }
}
