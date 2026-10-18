using PrismLibrary;
using Xunit;

namespace PrismLibrary.Tests
{
    public class SettingsEnablementTests
    {
        [Fact]
        public void SetInstanceEnabled_TogglesMembership()
        {
            var settings = new PrismLibrarySettings();
            Assert.False(settings.IsInstanceEnabled("pack"));

            settings.SetInstanceEnabled("pack", true);
            Assert.True(settings.IsInstanceEnabled("pack"));
            Assert.True(settings.IsInstanceEnabled("Pack"));

            settings.SetInstanceEnabled("pack", false);
            Assert.False(settings.IsInstanceEnabled("pack"));
        }
    }
}
