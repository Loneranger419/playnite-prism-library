using System;

namespace PrismLibrary.Services
{
    public enum PackPlatformKind
    {
        None,
        Modrinth,
        CurseForge,
        Ftb,
        Unknown
    }

    public static class PackPlatform
    {
        public static PackPlatformKind Parse(string managedPackType)
        {
            if (string.IsNullOrWhiteSpace(managedPackType))
            {
                return PackPlatformKind.None;
            }

            if (string.Equals(managedPackType, "modrinth", StringComparison.OrdinalIgnoreCase))
            {
                return PackPlatformKind.Modrinth;
            }

            if (string.Equals(managedPackType, "flame", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(managedPackType, "curseforge", StringComparison.OrdinalIgnoreCase))
            {
                return PackPlatformKind.CurseForge;
            }

            if (string.Equals(managedPackType, "ftb", StringComparison.OrdinalIgnoreCase))
            {
                return PackPlatformKind.Ftb;
            }

            return PackPlatformKind.Unknown;
        }

        public static string GetDisplayName(PackPlatformKind kind)
        {
            switch (kind)
            {
                case PackPlatformKind.Modrinth:
                    return "Modrinth";
                case PackPlatformKind.CurseForge:
                    return "CurseForge";
                case PackPlatformKind.Ftb:
                    return "FTB";
                default:
                    return null;
            }
        }

        public static string GetProjectUrl(PackPlatformKind kind, string packId, string slug = null)
        {
            if (string.IsNullOrWhiteSpace(packId) && string.IsNullOrWhiteSpace(slug))
            {
                return null;
            }

            switch (kind)
            {
                case PackPlatformKind.Modrinth:
                    return "https://modrinth.com/modpack/" + Uri.EscapeDataString(slug ?? packId);
                case PackPlatformKind.CurseForge:
                    return "https://www.curseforge.com/projects/" + Uri.EscapeDataString(packId);
                case PackPlatformKind.Ftb:
                    if (!string.IsNullOrWhiteSpace(slug))
                    {
                        return "https://www.feed-the-beast.com/modpacks/" + Uri.EscapeDataString(slug);
                    }

                    return "https://www.feed-the-beast.com/modpack/" + Uri.EscapeDataString(packId);
                default:
                    return null;
            }
        }
    }
}
