using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using System.Linq;

namespace TralaleroTralala
{
    /// <summary>
    /// Handles favicon loading for browser tabs
    /// </summary>
    public class FaviconLoader
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly string _faviconCacheDir;

        static FaviconLoader()
        {
            // Create favicon cache directory
            _faviconCacheDir = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Data",
                "faviconCache"
            );

            if (!Directory.Exists(_faviconCacheDir))
            {
                Directory.CreateDirectory(_faviconCacheDir);
            }
        }

        /// <summary>
        /// Loads favicon for a WebView page
        /// </summary>
        public static async Task LoadFaviconAsync(CoreWebView2 webView, Action<Image> onFaviconLoaded)
        {
            if (webView == null) return;

            try
            {
                string url = webView.Source;
                if (string.IsNullOrEmpty(url)) return;

                // Try to get cached favicon first
                string faviconHash = GetHashFromUrl(url);
                string cachedPath = Path.Combine(_faviconCacheDir, $"{faviconHash}.ico");

                if (File.Exists(cachedPath))
                {
                    try
                    {
                        using (var stream = File.OpenRead(cachedPath))
                        {
                            var image = Image.FromStream(stream);
                            onFaviconLoaded?.Invoke(image);
                            return;
                        }
                    }
                    catch
                    {
                        // If loading cached favicon fails, continue to fetch a new one
                        File.Delete(cachedPath);
                    }
                }

                // Get favicon URL from the page content or use default location
                Uri uri = new Uri(url);
                string baseUrl = $"{uri.Scheme}://{uri.Host}";

                // Execute JavaScript to find favicon links in the page
                string faviconScript = @"
                    (function() {
                        let icons = [];
                        // Check for regular link icons
                        let links = document.querySelectorAll('link[rel*=""icon""]');
                        for (let i = 0; i < links.length; i++) {
                            icons.push({
                                href: links[i].href,
                                sizes: links[i].sizes?.value || '',
                                rel: links[i].rel
                            });
                        }
                        // Check for Apple touch icons
                        let appleLinks = document.querySelectorAll('link[rel=""apple-touch-icon""]');
                        for (let i = 0; i < appleLinks.length; i++) {
                            icons.push({
                                href: appleLinks[i].href,
                                sizes: appleLinks[i].sizes?.value || '',
                                rel: appleLinks[i].rel
                            });
                        }
                        return JSON.stringify(icons);
                    })();
                ";

                string iconJson = await webView.ExecuteScriptAsync(faviconScript);

                // Parse the JSON-escaped string (double unescape needed)
                iconJson = iconJson.Trim('"');
                iconJson = Regex.Unescape(iconJson);

                // Try to find the best favicon URL
                string faviconUrl = null;

                // If we found any icons in the page
                if (!string.IsNullOrEmpty(iconJson) && iconJson != "[]")
                {
                    // Very simple parsing (in a real implementation, use JSON parsing)
                    var matches = Regex.Matches(iconJson, @"""href""\s*:\s*""([^""]+)""");
                    if (matches.Count > 0)
                    {
                        // Prefer PNG or larger icons when available
                        var pngMatch = matches.Cast<Match>()
                            .FirstOrDefault(m => m.Groups[1].Value.EndsWith(".png"));

                        if (pngMatch != null)
                        {
                            faviconUrl = pngMatch.Groups[1].Value;
                        }
                        else
                        {
                            // Take the first one
                            faviconUrl = matches[0].Groups[1].Value;
                        }
                    }
                }

                // If we still don't have a favicon URL, use the default location
                if (string.IsNullOrEmpty(faviconUrl))
                {
                    faviconUrl = $"{baseUrl}/favicon.ico";
                }

                // Download the favicon
                var response = await _httpClient.GetAsync(faviconUrl);
                if (response.IsSuccessStatusCode)
                {
                    byte[] data = await response.Content.ReadAsByteArrayAsync();
                    using (var ms = new MemoryStream(data))
                    {
                        try
                        {
                            var image = Image.FromStream(ms);

                            // Create a new bitmap from the image to avoid disposal issues
                            var bitmap = new Bitmap(image);
                            image.Dispose(); // Dispose the original image

                            // Cache the favicon
                            try
                            {
                                bitmap.Save(cachedPath);
                            }
                            catch
                            {
                                // If saving fails, continue with the bitmap we have
                            }

                            onFaviconLoaded?.Invoke(bitmap);
                        }
                        catch
                        {
                            // Use a cloned default icon to avoid disposal issues
                            var defaultIcon = (Image)SystemIcons.Application.ToBitmap().Clone();
                            onFaviconLoaded?.Invoke(defaultIcon);
                        }
                    }
                }
                else
                {
                    // If we can't download, use default icon
                    onFaviconLoaded?.Invoke(SystemIcons.Application.ToBitmap());
                }
            }
            catch (Exception ex)
            {
                DebugLogger.LogError($"Error loading favicon: {ex.Message}");
                // Use default icon on error
                onFaviconLoaded?.Invoke(SystemIcons.Application.ToBitmap());
            }
        }

        public static bool IsImageValid(Image img)
        {
            try
            {
                return img.Width > 0 && img.Height > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a simple hash from a URL for caching
        /// </summary>
        private static string GetHashFromUrl(string url)
        {
            try
            {
                Uri uri = new Uri(url);
                return uri.Host.Replace(".", "_");
            }
            catch
            {
                // If parsing fails, just use a simple hash
                return url.GetHashCode().ToString().Replace("-", "m");
            }
        }
    }
}