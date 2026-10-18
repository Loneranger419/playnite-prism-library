using PrismLibrary.Services;
using Xunit;

namespace PrismLibrary.Tests
{
    public class LaunchBuilderTests
    {
        [Fact]
        public void BuildLaunchArguments_QuotesPathsAndInstanceIdsWithSpaces()
        {
            var args = LaunchBuilder.BuildLaunchArguments(
                @"C:\Games\Prism Data",
                "Agrarian Skies",
                "Steve");

            Assert.Equal(
                "--dir \"C:\\Games\\Prism Data\" --launch \"Agrarian Skies\" --profile \"Steve\"",
                args);
        }

        [Fact]
        public void BuildLaunchArguments_OmitsProfileWhenEmpty()
        {
            var args = LaunchBuilder.BuildLaunchArguments(@"D:\Prism", "1.21.1", null);
            Assert.Equal("--dir \"D:\\Prism\" --launch \"1.21.1\"", args);
        }

        [Fact]
        public void BuildActions_PlayOnlyWhenEnabled()
        {
            var enabled = LaunchBuilder.BuildActions(@"C:\prismlauncher.exe", @"C:\data", "pack", @"C:\data\instances\pack", @"C:\data\instances\pack\.minecraft", null, true);
            var disabled = LaunchBuilder.BuildActions(@"C:\prismlauncher.exe", @"C:\data", "pack", @"C:\data\instances\pack", @"C:\data\instances\pack\.minecraft", null, false);

            Assert.Contains(enabled, a => a.IsPlayAction);
            Assert.DoesNotContain(disabled, a => a.IsPlayAction);
            Assert.Contains(disabled, a => a.Name == "Open in Prism");
            Assert.Contains(disabled, a => a.Name == "Open Instance Folder");
            Assert.DoesNotContain(enabled, a => a.Name == "Update in Prism");
        }

        [Fact]
        public void BuildActions_ManagedPackShowsCheckUntilUpdateAvailable()
        {
            var pluginId = System.Guid.Parse("b2aeb685-8587-490d-8b0f-5158b2df112f");
            var uncheckedActions = LaunchBuilder.BuildActions(
                @"C:\prismlauncher.exe",
                @"C:\data",
                "pack",
                @"C:\data\instances\pack",
                @"C:\data\instances\pack\.minecraft",
                null,
                true,
                isManagedPack: true,
                pluginId: pluginId,
                updateAvailable: false);

            Assert.Contains(uncheckedActions, a => a.Name == "Check for pack update");
            Assert.DoesNotContain(uncheckedActions, a => a.Name == "Update in Prism");

            var updateActions = LaunchBuilder.BuildActions(
                @"C:\prismlauncher.exe",
                @"C:\data",
                "pack",
                @"C:\data\instances\pack",
                @"C:\data\instances\pack\.minecraft",
                null,
                true,
                isManagedPack: true,
                pluginId: pluginId,
                updateAvailable: true);

            Assert.Contains(updateActions, a => a.Name == "Update in Prism");
            Assert.DoesNotContain(updateActions, a => a.Name == "Check for pack update");
        }
    }
}
