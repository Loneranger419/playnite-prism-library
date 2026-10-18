using System.Text.RegularExpressions;
using Markdig;

namespace PrismLibrary.Services.Metadata
{
    public static class DescriptionNormalizer
    {
        private static readonly Regex HtmlTagRegex = new Regex(
            @"</?(p|div|br|h[1-6]|ul|ol|li|span|strong|em|b|i|a|img|table|tr|td|th|blockquote|pre|code|hr)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Use Playnite-hosted Markdig 0.18 API (ToHtml with pipeline only — no MarkdownParserContext overload).
        private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();

        /// <summary>
        /// Converts Markdown descriptions (common for FTB) to HTML for Playnite,
        /// then replaces YouTube embeds with clickable links.
        /// </summary>
        public static string Normalize(string description, out System.Collections.Generic.List<string> youtubeUrls)
        {
            youtubeUrls = new System.Collections.Generic.List<string>();
            if (string.IsNullOrWhiteSpace(description))
            {
                return description;
            }

            var html = LooksLikeHtml(description)
                ? description
                : Markdown.ToHtml(description, Pipeline);

            return DescriptionSanitizer.Sanitize(html, out youtubeUrls);
        }

        public static bool LooksLikeHtml(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && HtmlTagRegex.IsMatch(value);
        }
    }
}
