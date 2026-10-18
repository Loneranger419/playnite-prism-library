using System;
using System.IO;
using PrismLibrary.Services;
using Xunit;

namespace PrismLibrary.Tests
{
    public class ManagedPackReaderTests
    {
        [Fact]
        public void Read_ParsesCurseForgeManagedPackFields()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fixtures", "managed-flame.cfg");
            var data = InstanceCfgReader.ReadFile(path);

            Assert.True(data.ManagedPack);
            Assert.Equal("flame", data.ManagedPackType);
            Assert.Equal("925200", data.ManagedPackId);
            Assert.Equal("All the Mods 10", data.ManagedPackName);
            Assert.Equal("8558519", data.ManagedPackVersionId);
            Assert.Equal("7.3", data.ManagedPackVersionName);
            Assert.Equal(PackPlatformKind.CurseForge, PackPlatform.Parse(data.ManagedPackType));
            Assert.Equal("CurseForge", PackPlatform.GetDisplayName(PackPlatformKind.CurseForge));
        }
    }
}
