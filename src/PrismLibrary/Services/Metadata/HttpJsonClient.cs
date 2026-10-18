using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using Playnite.SDK;

namespace PrismLibrary.Services.Metadata
{
    public sealed class HttpJsonClient : IDisposable
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly HttpClient httpClient;

        public HttpJsonClient()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
            httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("PrismLibrary-Playnite/" + version);
            httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        }

        public string GetString(string url, IDictionary<string, string> headers = null)
        {
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                        }
                    }

                    var response = httpClient.SendAsync(request).GetAwaiter().GetResult();
                    var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    if (!response.IsSuccessStatusCode)
                    {
                        logger.Warn($"HTTP {(int)response.StatusCode} for {SanitizeUrl(url)}");
                        return null;
                    }

                    return body;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "HTTP request failed for " + SanitizeUrl(url));
                return null;
            }
        }

        public void Dispose()
        {
            httpClient.Dispose();
        }

        private static string SanitizeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return url;
            }

            // Avoid leaking query tokens if any are ever added.
            var hash = url.IndexOf('#');
            if (hash >= 0)
            {
                url = url.Substring(0, hash);
            }

            return url;
        }
    }
}
