using System;
using System.IO;
using PrismLibrary.Services;
using Xunit;

namespace PrismLibrary.Tests
{
    public class GroupReaderTests
    {
        [Fact]
        public void Read_MapsInstanceIdsToGroups()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fixtures", "instgroups.json");
            var groups = GroupReader.ReadFile(path);

            Assert.Equal("Modded", groups["Agrarian Skies"]);
            Assert.Equal("Modded", groups["Create-Pack"]);
            Assert.Equal("Vanilla", groups["1.21.1"]);
        }
    }
}
