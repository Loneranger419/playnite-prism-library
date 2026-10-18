using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PrismLibrary.Services.Metadata
{
    /// <summary>
    /// Playnite's description HTML renderer cannot play YouTube iframes.
    /// Convert embeds to normal links so users can open them in a browser.
    /// </summary>
    public static class DescriptionSanitizer
    {
        private static readonly Regex IframeRegex = new Regex(
            @"<iframe\b[^>]*\bsrc\s*=\s*[""']([^""']+)[""'][^>]*>\s*</iframe>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex ObjectEmbedRegex = new Regex(
            @"<(?:object|embed)\b[^>]*(?:data|src)\s*=\s*[""']([^""']*youtube[^""']*)[""'][^>]*(?:/>|>.*?</(?:object|embed)>)",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        public static string Sanitize(string html, out List<string> youtubeUrls)
        {
            var urls = new List<string>();
            youtubeUrls = urls;

            if (string.IsNullOrWhiteSpace(html))
            {
                return html;
            }

            var result = IframeRegex.Replace(html, match => ReplaceMedia(match.Groups[1].Value, urls));
            result = ObjectEmbedRegex.Replace(result, match => ReplaceMedia(match.Groups[1].Value, urls));
            return result;
        }

        private static string ReplaceMedia(string src, List<string> youtubeUrls)
        {
            var watchUrl = ToYouTubeWatchUrl(src);
            if (watchUrl == null)
            {
                // Non-YouTube iframe: drop it; Playnite cannot host arbitrary embeds reliably.
                return string.Empty;
            }

            if (!youtubeUrls.Any(u => string.Equals(u, watchUrl, StringComparison.OrdinalIgnoreCase)))
            {
                youtubeUrls.Add(watchUrl);
            }

            return "<p><a href=\"" + watchUrl + "\">Watch on YouTube</a></p>";
        }

        public static string ToYouTubeWatchUrl(string src)
        {
            if (string.IsNullOrWhiteSpace(src))
            {
                return null;
            }

            if (!Uri.TryCreate(src, UriKind.Absolute, out var uri))
            {
                return null;
            }

            var host = uri.Host.Replace("www.", string.Empty);
            if (host.Equals("youtu.be", StringComparison.OrdinalIgnoreCase))
            {
                var id = uri.AbsolutePath.Trim('/');
                return string.IsNullOrWhiteSpace(id) ? null : "https://www.youtube.com/watch?v=" + id;
            }

            if (!host.EndsWith("youtube.com", StringComparison.OrdinalIgnoreCase) &&
                !host.EndsWith("youtube-nocookie.com", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (uri.AbsolutePath.StartsWith("/embed/", StringComparison.OrdinalIgnoreCase) ||
                uri.AbsolutePath.StartsWith("/v/", StringComparison.OrdinalIgnoreCase) ||
                uri.AbsolutePath.StartsWith("/shorts/", StringComparison.OrdinalIgnoreCase))
            {
                var id = uri.AbsolutePath.Trim('/').Split('/').LastOrDefault();
                return string.IsNullOrWhiteSpace(id) ? null : "https://www.youtube.com/watch?v=" + id;
            }

            var query = ParseQuery(uri.Query);
            if (query.TryGetValue("v", out var videoId) && !string.IsNullOrWhiteSpace(videoId))
            {
                return "https://www.youtube.com/watch?v=" + videoId;
            }

            return null;
        }

        private static Dictionary<string, string> ParseQuery(string query)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(query))
            {
                return result;
            }

            var trimmed = query.TrimStart('?');
            foreach (var part in trimmed.Split('&'))
            {
                var pair = part.Split(new[] { '=' }, 2);
                if (pair.Length == 2)
                {
                    result[Uri.UnescapeDataString(pair[0])] = Uri.UnescapeDataString(pair[1]);
                }
            }

            return result;
        }
    }
}
