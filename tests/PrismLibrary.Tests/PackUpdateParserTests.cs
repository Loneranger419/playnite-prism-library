using System.IO;
using PrismLibrary.Services.Metadata;
using Xunit;

namespace PrismLibrary.Tests
{
    public class PackUpdateParserTests
    {
        private static string Fixture(string name)
        {
            return File.ReadAllText(Path.Combine("fixtures", name));
        }

        [Fact]
        public void Modrinth_PrefersFirstReleaseOverNewerBeta()
        {
            var latest = ModrinthMetadataClient.ParseLatestVersion(Fixture("modrinth-versions.json"));

            Assert.NotNull(latest);
            Assert.Equal("verRelease2", latest.VersionId);
            Assert.Equal("1.9.1", latest.VersionName);
        }

        [Fact]
        public void CurseForge_PrefersNewestReleaseOverBeta()
        {
            var latest = CurseForgeMetadataClient.ParseLatestVersion(Fixture("curseforge-files.json"));

            Assert.NotNull(latest);
            Assert.Equal("7000001", latest.VersionId);
            Assert.Equal("Pack 7.4", latest.VersionName);
        }

        [Fact]
        public void Ftb_PrefersNewestReleaseIgnoringArchivedAndBeta()
        {
            var latest = FtbMetadataClient.ParseLatestVersion(Fixture("ftb-versions.json"));

            Assert.NotNull(latest);
            Assert.Equal("100144", latest.VersionId);
            Assert.Equal("1.0.4", latest.VersionName);
        }
    }
}
