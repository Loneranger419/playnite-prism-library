using System;
using System.IO;
using PrismLibrary.Services;
using Xunit;

namespace PrismLibrary.Tests
{
    public class InstanceCfgReaderTests
    {
        [Fact]
        public void Read_ParsesNameIconPlaytimeAndLastLaunch()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fixtures", "instance.cfg");
            var data = InstanceCfgReader.ReadFile(path);

            Assert.Equal("Agrarian Skies", data.Name);
            Assert.Equal("skyblock", data.IconKey);
            Assert.Equal(3661UL, data.TotalTimePlayedSeconds);
            Assert.NotNull(data.LastLaunch);
            Assert.Equal(2024, data.LastLaunch.Value.ToUniversalTime().Year);
        }
    }
}
