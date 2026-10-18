using System;
using System.IO;
using PrismLibrary.Services;
using Xunit;

namespace PrismLibrary.Tests
{
    public class MmcPackReaderTests
    {
        [Fact]
        public void Read_ExtractsMinecraftVersionAndLoaders()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fixtures", "mmc-pack.json");
            var data = MmcPackReader.ReadFile(path);

            Assert.Equal("1.20.1", data.MinecraftVersion);
            Assert.Contains("Fabric", data.Loaders);
            Assert.Contains("Forge", data.Loaders);
        }
    }
}
