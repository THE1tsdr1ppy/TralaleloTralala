using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace TralaleroTralala
{
    // New extension class for VerticalTabControl
    public static class TabControlExtensions
    {
        /// <summary>
        /// Adds redirection handling to the VerticalTabControl
        /// </summary>
        public static void AddRedirectionHandling2(this VerticalTabControl tabControl)
        {
            if (tabControl == null) return;

            // Store the original URLs for each tab to detect redirections
            Dictionary<WebView2, string> originalUrls = new Dictionary<WebView2, string>();

            // Event to monitor for redirections
            EventHandler<CoreWebView2NavigationStartingEventArgs> navigationStartingHandler = async (sender, e) =>
            {
                try
                {
                    var webView = sender as CoreWebView2;
                    if (webView == null) return;

                    // Get the parent WebView2 control
                    WebView2 parentWebView = null;
                    for (int i = 0; i < tabControl.TabCount; i++)
                    {
                        var currentWebView = tabControl.GetWebViewAt(i);
                        if (currentWebView?.CoreWebView2 == webView)
                        {
                            parentWebView = currentWebView;
                            break;
                        }
                    }

                    if (parentWebView == null) return;

                    // Get or update the original URL
                    string currentUrl = webView.Source;

                    // If this is the first navigation, store the URL
                    if (!originalUrls.ContainsKey(parentWebView))
                    {
                        originalUrls[parentWebView] = e.Uri;
                        return;
                    }

                    // Check if this is a new user-initiated navigation
                    if (originalUrls[parentWebView] == e.Uri)
                    {
                        return; // Not a redirection, just a refresh or initial load
                    }

                    // Possible redirection - check if the domain is different
                    Uri originalUri = new Uri(originalUrls[parentWebView]);
                    Uri newUri = new Uri(e.Uri);

                    // Only show the prompt when domain changes or explicitly set by page
                    bool isDifferentDomain = originalUri.Host != newUri.Host;

                    // Only show if it appears to be a redirection (not user clicking a link)
                    bool seemsLikeRedirection = isDifferentDomain &&
                                               !e.IsUserInitiated &&
                                               !e.Uri.Contains("@");  // Avoid email links

                    if (seemsLikeRedirection)
                    {
                        // Cancel the current navigation
                        e.Cancel = true;

                        // Get the form that contains the tab control
                        Form parentForm = tabControl.FindForm();
                        if (parentForm == null) return;

                        // Use BeginInvoke to avoid UI thread issues
                        parentForm.BeginInvoke(new Action(() =>
                        {
                            // Show redirection prompt
                            using (var prompt = new RedirectionPrompt(originalUrls[parentWebView], e.Uri))
                            {
                                var result = prompt.ShowDialog(parentForm);

                                if (result == DialogResult.OK)
                                {
                                    switch (prompt.Result)
                                    {
                                        case RedirectionResult.CurrentTab:
                                            // Navigate in current tab
                                            originalUrls[parentWebView] = e.Uri; // Update stored URL
                                            webView.Navigate(e.Uri);
                                            break;

                                        case RedirectionResult.NewTab:
                                            // Open in new tab
                                            tabControl.AddNewTab("New Tab", e.Uri);
                                            break;

                                        case RedirectionResult.Cancel:
                                            // Cancel navigation - already handled by e.Cancel = true
                                            break;
                                    }
                                }
                            }
                        }));
                    }
                    else
                    {
                        // Update the stored URL for future reference
                        originalUrls[parentWebView] = e.Uri;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in redirection handling: {ex.Message}");
                }
            };

            // Helper to attach navigation events
            Action<WebView2> attachRedirectionHandling = (webView) =>
            {
                if (webView?.CoreWebView2 != null)
                {
                    webView.CoreWebView2.NavigationStarting += navigationStartingHandler;
                }
            };

            // Attach to existing tabs
            for (int i = 0; i < tabControl.TabCount; i++)
            {
                attachRedirectionHandling(tabControl.GetWebViewAt(i));
            }

            // Monitor for new tabs
            tabControl.TabSelected += (sender, e) =>
            {
                var currentWebView = tabControl.CurrentWebView;
                if (currentWebView?.CoreWebView2 != null && !originalUrls.ContainsKey(currentWebView))
                {
                    attachRedirectionHandling(currentWebView);
                    originalUrls[currentWebView] = currentWebView.CoreWebView2.Source;
                }
            };
        }

        /// <summary>
        /// Adds favicon loading to the VerticalTabControl
        /// </summary>
        public static void AddFaviconSupport1(this VerticalTabControl tabControl)
        {
            // Extract the private TabItem class using reflection
            var tabItemType = tabControl.GetType().GetNestedType("TabItem",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (tabItemType == null)
            {
                Console.WriteLine("Error: Unable to access TabItem class via reflection");
                return;
            }

            // Create a function to load favicon for a specific WebView
            Action<WebView2, int> loadFaviconForTab = async (webView, tabIndex) =>
            {
                if (webView?.CoreWebView2 == null) return;

                try
                {
                    // Find the TabItem for this webView
                    object tabItem = null;
                    var tabsField = tabControl.GetType().GetField("tabs",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    if (tabsField != null)
                    {
                        var tabs = tabsField.GetValue(tabControl) as System.Collections.IList;
                        if (tabs != null && tabIndex >= 0 && tabIndex < tabs.Count)
                        {
                            tabItem = tabs[tabIndex];
                        }
                    }

                    if (tabItem == null) return;

                    // Get the UpdateFavicon method
                    var updateFaviconMethod = tabItemType.GetMethod("UpdateFavicon");
                    if (updateFaviconMethod == null) return;

                    // Load the favicon using our FaviconLoader
                    await FaviconLoader.LoadFaviconAsync(webView.CoreWebView2, favicon =>
                    {
                        try
                        {
                            // Use the UpdateFavicon method via reflection
                            updateFaviconMethod.Invoke(tabItem, new object[] { favicon });
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error updating favicon: {ex.Message}");
                        }
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in favicon loading: {ex.Message}");
                }
            };

            // Set up navigation completed handler for each WebView
            EventHandler<CoreWebView2NavigationCompletedEventArgs> navigationCompletedHandler = async (sender, e) =>
            {
                try
                {
                    if (sender is CoreWebView2 coreWebView && e.IsSuccess)
                    {
                        // Find the WebView associated with this CoreWebView2
                        for (int i = 0; i < tabControl.TabCount; i++)
                        {
                            var tabWebView = tabControl.GetWebViewAt(i);
                            if (tabWebView?.CoreWebView2 == coreWebView)
                            {
                                // Load favicon for this tab
                                await Task.Delay(500); // Brief delay to ensure page is fully loaded
                                loadFaviconForTab(tabWebView, i);
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in navigation completed handler: {ex.Message}");
                }
            };

            // Attach the handler to existing tabs
            for (int i = 0; i < tabControl.TabCount; i++)
            {
                var webView = tabControl.GetWebViewAt(i);
                if (webView?.CoreWebView2 != null)
                {
                    webView.CoreWebView2.NavigationCompleted += navigationCompletedHandler;
                    loadFaviconForTab(webView, i);
                }
            }

            // Attach to new tabs when they're created
            tabControl.TabSelected += (sender, e) =>
            {
                try
                {
                    var webView = tabControl.GetWebViewAt(e.TabIndex);
                    if (webView?.CoreWebView2 != null)
                    {
                        // Check if we've already attached the handler
                        var handlers = webView.CoreWebView2.GetType()
                            .GetField("NavigationCompletedEvent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            ?.GetValue(webView.CoreWebView2);

                        // If we can't check, just attach it (at worst it gets attached twice)
                        if (handlers == null || (handlers as System.Collections.ICollection)?.Count == 0)
                        {
                            webView.CoreWebView2.NavigationCompleted += navigationCompletedHandler;
                        }

                        // Try to load favicon right away for this tab
                        loadFaviconForTab(webView, e.TabIndex);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error attaching navigation handler: {ex.Message}");
                }
            };
        }
    }
}