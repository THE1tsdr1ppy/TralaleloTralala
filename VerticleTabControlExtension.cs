using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using TralaleloTralala;

namespace TralaleroTralala
{
    public static class VerticalTabControlExtensions
    {
        // Event delegate for tab creation
        public delegate void TabCreatedEventHandler(object sender, TabCreatedEventArgs e);

        // Event arguments for tab creation
        public class TabCreatedEventArgs : EventArgs
        {
            public WebView2 WebView { get; }
            public TabPage TabPage { get; }

            public TabCreatedEventArgs(WebView2 webView, TabPage tabPage)
            {
                WebView = webView;
                TabPage = tabPage;
            }
        }

        // Dictionary to store tab created event handlers for each control
        private static readonly Dictionary<VerticalTabControl, List<TabCreatedEventHandler>> _tabCreatedHandlers
            = new Dictionary<VerticalTabControl, List<TabCreatedEventHandler>>();

        // Add a handler for the TabCreated event
        public static void AddTabCreatedHandler(this VerticalTabControl tabControl, TabCreatedEventHandler handler)
        {
            if (!_tabCreatedHandlers.ContainsKey(tabControl))
            {
                _tabCreatedHandlers[tabControl] = new List<TabCreatedEventHandler>();
            }

            _tabCreatedHandlers[tabControl].Add(handler);
        }

        // Raise the TabCreated event
        public static void RaiseTabCreated(this VerticalTabControl tabControl, WebView2 webView, TabPage tabPage)
        {
            if (_tabCreatedHandlers.ContainsKey(tabControl))
            {
                var args = new TabCreatedEventArgs(webView, tabPage);
                foreach (var handler in _tabCreatedHandlers[tabControl])
                {
                    handler(tabControl, args);
                }
            }
        }

        // Add redirection handling to the tab control
        public static void AddRedirectionHandling(this VerticalTabControl tabControl)
        {
            // Make sure we're handling tab creations
            tabControl.AddTabCreatedHandler((sender, e) => {
                if (e.WebView?.CoreWebView2 != null)
                {
                    // Handle navigation starting event to detect redirections
                    e.WebView.CoreWebView2.NavigationStarting += (s, args) => {
                        // Your redirection handling logic here
                        System.Diagnostics.Debug.WriteLine($"Navigation starting to: {args.Uri}");
                    };
                }
            });
        }

        // Add favicon support to the tab control
        public static void AddFaviconSupport(this VerticalTabControl tabControl)
        {
            // Make sure we're handling tab creations
            tabControl.AddTabCreatedHandler((sender, e) => {
                if (e.WebView?.CoreWebView2 != null)
                {
                    // Handle favicon changed event
                    e.WebView.CoreWebView2.FaviconChanged += async (s, args) => {
                        try
                        {
                            // Get the favicon URI
                            string faviconUri = e.WebView.CoreWebView2.FaviconUri;

                            if (!string.IsNullOrEmpty(faviconUri))
                            {
                                // Set tab icon (implementation would depend on your VerticalTabControl implementation)
                                await SetTabIcon(tabControl, e.TabPage, faviconUri);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error handling favicon: {ex.Message}");
                        }
                    };
                }
            });
        }

        // Helper method to set a tab icon from a URI
        private static async Task SetTabIcon(VerticalTabControl tabControl, TabPage tabPage, string iconUri)
        {
            // This implementation depends on your VerticalTabControl structure
            // Here's a generic approach that you'll need to adapt
            try
            {
                // Load favicon asynchronously
                using (var webClient = new System.Net.WebClient())
                {
                    byte[] imageData = await webClient.DownloadDataTaskAsync(iconUri);
                    using (var ms = new System.IO.MemoryStream(imageData))
                    {
                        // Create bitmap from stream
                        var bitmap = new Bitmap(Bitmap.FromStream(ms), 16, 16);

                        // Use the existing tab control's interface to set the image
                        int tabIndex = GetTabIndexForTabPage(tabControl, tabPage);
                        if (tabIndex >= 0)
                        {
                            // Update the tab's icon
                            // Note: This needs to run on the UI thread
                            tabControl.BeginInvoke(new Action(() => {
                                // Set the tab image using existing method
                                tabControl.SetTabImage(tabIndex, bitmap);

                                // Force redraw
                                tabControl.Invalidate();
                            }));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting tab icon: {ex.Message}");
            }
        }

        // Helper method to find the tab index for a TabPage
        private static int GetTabIndexForTabPage(VerticalTabControl tabControl, TabPage tabPage)
        {
            // This is a placeholder - you need to implement logic to find the tab index
            // based on the TabPage. This depends on how your VerticalTabControl maps
            // TabPage objects to indexes.

            // For example, if you have a way to get the WebView2 from a TabPage:
            // WebView2 webView = GetWebViewFromTabPage(tabPage);
            // return tabControl.GetTabIndex(webView);

            // As a fallback for now:
            return -1;
        }

        // Extension method to get WebView from CoreWebView2
        public static WebView2 GetWebView(this CoreWebView2 coreWebView)
        {
            // Find the WebView2 control associated with this CoreWebView2
            // This assumes you have a way to map CoreWebView2 instances to their WebView2 controls

            // For a simple implementation, you might need to maintain a dictionary:
            if (CoreWebView2ToWebViewMap.TryGetValue(coreWebView, out var webView))
            {
                return webView;
            }

            return null;
        }

        // Dictionary to map CoreWebView2 instances to their WebView2 controls
        private static readonly Dictionary<CoreWebView2, WebView2> CoreWebView2ToWebViewMap =
            new Dictionary<CoreWebView2, WebView2>();

        // Method to register a WebView2 with its CoreWebView2
        public static void RegisterWebView(this CoreWebView2 coreWebView, WebView2 webView)
        {
            if (coreWebView != null && webView != null)
            {
                CoreWebView2ToWebViewMap[coreWebView] = webView;
            }
        }

        // Method to clean up the registration when a WebView is disposed
        public static void UnregisterWebView(this CoreWebView2 coreWebView)
        {
            if (coreWebView != null && CoreWebView2ToWebViewMap.ContainsKey(coreWebView))
            {
                CoreWebView2ToWebViewMap.Remove(coreWebView);
            }
        }
    }
}