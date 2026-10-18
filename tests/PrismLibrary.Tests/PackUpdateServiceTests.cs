using System.IO;
using PrismLibrary.Models;
using PrismLibrary.Services.Metadata;
using Xunit;

namespace PrismLibrary.Tests
{
    public class PackUpdateServiceTests
    {
        [Fact]
        public void HasCachedUpdate_IsFalseWhenInstalledMatchesLatest()
        {
            var dir = Path.Combine(Path.GetTempPath(), "PrismLibraryTests-" + Path.GetRandomFileName());
            Directory.CreateDirectory(dir);

            try
            {
                var cachePath = Path.Combine(dir, "pack-updates.json");
                File.WriteAllText(cachePath,
                    @"[{""InstanceId"":""pack"",""UpdateAvailable"":true,""CurrentVersionId"":""old"",""LatestVersionId"":""new""}]");

                using (var service = new PackUpdateService(dir))
                {
                    var outdated = new PrismInstance
                    {
                        Id = "pack",
                        ManagedPack = true,
                        ManagedPackType = "modrinth",
                        ManagedPackId = "abc",
                        ManagedPackVersionId = "old"
                    };
                    var updated = new PrismInstance
                    {
                        Id = "pack",
                        ManagedPack = true,
                        ManagedPackType = "modrinth",
                        ManagedPackId = "abc",
                        ManagedPackVersionId = "new"
                    };

                    Assert.True(service.HasCachedUpdate(outdated));
                    Assert.False(service.HasCachedUpdate(updated));
                }
            }
            finally
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }
            }
        }
    }
}
