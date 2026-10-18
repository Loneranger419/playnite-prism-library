using System.IO;
using PrismLibrary.Services;
using Xunit;

namespace PrismLibrary.Tests
{
    public class IconResolverTests
    {
        [Fact]
        public void Resolve_PrefersIconsDirectoryMatch()
        {
            var root = Path.Combine(Path.GetTempPath(), "PrismLibraryTests_" + Path.GetRandomFileName());
            var icons = Path.Combine(root, "icons");
            var instance = Path.Combine(root, "instances", "pack");
            Directory.CreateDirectory(icons);
            Directory.CreateDirectory(instance);

            var iconPath = Path.Combine(icons, "skyblock.png");
            File.WriteAllBytes(iconPath, new byte[] { 1, 2, 3 });

            try
            {
                var resolved = IconResolver.Resolve(icons, instance, "skyblock");
                Assert.Equal(iconPath, resolved);
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [Fact]
        public void Resolve_FallsBackToInstanceIcon()
        {
            var root = Path.Combine(Path.GetTempPath(), "PrismLibraryTests_" + Path.GetRandomFileName());
            var icons = Path.Combine(root, "icons");
            var instance = Path.Combine(root, "instances", "pack");
            Directory.CreateDirectory(icons);
            Directory.CreateDirectory(instance);

            var iconPath = Path.Combine(instance, "icon.png");
            File.WriteAllBytes(iconPath, new byte[] { 1, 2, 3 });

            try
            {
                var resolved = IconResolver.Resolve(icons, instance, "default");
                Assert.Equal(iconPath, resolved);
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }
    }
}
