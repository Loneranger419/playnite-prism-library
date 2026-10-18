using PrismLibrary.Services.Metadata;
using Xunit;

namespace PrismLibrary.Tests
{
    public class DescriptionNormalizerTests
    {
        [Fact]
        public void Normalize_ConvertsMarkdownHeadingsAndBoldToHtml()
        {
            var markdown = "## **The Stone Remembers**\n\nWelcome to **StoneBlock 4**.";
            var html = DescriptionNormalizer.Normalize(markdown, out _);

            Assert.Contains("<h2", html);
            Assert.Contains("<strong>The Stone Remembers</strong>", html);
            Assert.Contains("<strong>StoneBlock 4</strong>", html);
            Assert.DoesNotContain("## ", html);
        }

        [Fact]
        public void Normalize_LeavesExistingHtmlAlone()
        {
            var existing = "<p>Full <strong>HTML</strong> description</p>";
            var html = DescriptionNormalizer.Normalize(existing, out _);

            Assert.Equal(existing, html);
        }
    }
}
