using PrismLibrary.Services.Metadata;
using Xunit;

namespace PrismLibrary.Tests
{
    public class DescriptionSanitizerTests
    {
        [Fact]
        public void Sanitize_ReplacesYouTubeIframeWithLink()
        {
            var html = "<p>Intro</p><iframe src=\"https://www.youtube.com/embed/dQw4w9WgXcQ\" width=\"560\" height=\"315\"></iframe><p>More</p>";
            var result = DescriptionSanitizer.Sanitize(html, out var urls);

            Assert.Contains("Watch on YouTube", result);
            Assert.DoesNotContain("<iframe", result);
            Assert.Single(urls);
            Assert.Equal("https://www.youtube.com/watch?v=dQw4w9WgXcQ", urls[0]);
        }
    }
}
