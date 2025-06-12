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
    /// <summary>
    /// A custom tab control that displays tabs vertically on the left side
    /// and contains WebView2 controls for each tab.
    /// </summary>
    public class VerticalTabControl : UserControl
    {
        private Panel tabPanel;
        private List<TabItem> tabs;
        private int selectedIndex = -1;
        private const int TabHeight = 40;
        private const int TabWidth = 260;
        private Color selectedTabColor = Color.FromArgb(124, 164, 212); // Match the blue in the control panel
        private Color normalTabColor = Color.FromArgb(40, 40, 40);      // Match the dark theme
        private Color textColor = Color.White;                          // Text color for tabs
        private Button addTabButton;

        public event EventHandler<TabEventArgs> TabClosed;
        public event EventHandler<TabEventArgs> TabSelected;
        // Add these properties to the VerticalTabControl class
        public int SelectedIndex => selectedIndex;
        public int TabCount => tabs.Count;
        public WebView2 GetWebViewAt(int index) => tabs[index].WebView;

        // Add this method to find a tab index by WebView
        public int GetTabIndex(WebView2 webView)
        {
            for (int i = 0; i < tabs.Count; i++)
            {
                if (tabs[i].WebView == webView)
                    return i;
            }
            return -1;
        }

        public VerticalTabControl()
        {
            // Initialize fields before calling InitializeComponent
            tabs = new List<TabItem>();

            // Now initialize the component
            InitializeComponent();
            _tabImages.ImageSize = new Size(16, 16); // Favicon size
        }

        private void InitializeComponent()
        {
            try
            {
                // Set up the main control
                this.BackColor = Color.FromArgb(27, 27, 27); // Match panel1 color
                this.Size = new Size(280, 765);              // Explicitly set size to match panel1
                this.Location = new Point(12, 6);            // Explicitly set location

                // Create tab panel (left side)
                tabPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(27, 27, 27), // Match panel1 color
                    AutoScroll = true
                };

                // Add panels to this control
                this.Controls.Add(tabPanel);

                // Add button to create new tab
                addTabButton = new Button
                {
                    Text = "+",
                    Size = new Size(TabWidth - 10, 30),
                    Location = new Point(5, 10), // Start at top since tabs.Count might be 0
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(40, 40, 40), // Dark color
                    ForeColor = Color.White,
                    Font = new Font("Arial", 12, FontStyle.Bold)
                };
                addTabButton.FlatAppearance.BorderSize = 0;
                addTabButton.Click += AddTabButton_Click;
                tabPanel.Controls.Add(addTabButton);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in InitializeComponent: {ex.Message}\nStack Trace: {ex.StackTrace}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddTabButton_Click(object sender, EventArgs e)
        {
            try
            {
                AddNewTab("New Tab", "https://www.Google.com");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding new tab: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RepositionAddButton()
        {
            try
            {
                if (addTabButton != null && !this.IsDisposed && !addTabButton.IsDisposed)
                {
                    int tabCount = tabs != null ? tabs.Count : 0;
                    addTabButton.Location = new Point(5, tabCount * TabHeight + 10);
                }
            }
            catch (Exception ex)
            {
                DebugLogger.LogMSG($"Error repositioning add button: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Adds a new tab with the specified title and URL
        /// </summary>
        /// <summary>
        /// Adds a new tab with the specified title and URL
        /// </summary>
        public async void AddNewTab(string title, string url)
        {
            var tabPage = new TabPage(title);

            try
            {
                // Get the form that contains this control
                Form parentForm = this.FindForm();
                if (parentForm == null)
                {
                    MessageBox.Show("Cannot add tab: parent form not found.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (IncludedWebpages.SpecialPages.TryGetValue(url, out var internalUrl))
                {
                    url = internalUrl;
                }
                // Create WebView2 control for the tab content
                WebView2 webView = new WebView2
                {
                    Location = new Point(298, 62),  // Changed from (298, 6)
                    Size = new Size(1620, 967),     // Changed from (1594, 1023)
                    Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                    Visible = false
                };
                var environment = await CoreWebView2Environment.CreateAsync();
                await webView.EnsureCoreWebView2Async(environment);

                // Configure audio session identification
                if (webView.CoreWebView2 != null)
                {
                    webView.CoreWebView2.Settings.IsStatusBarEnabled = false;

                    // Additional settings to improve audio session handling
                    webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                    webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
                }
                // Add the webView to the form directly
                parentForm.Controls.Add(webView);
                webView.BringToFront();

                // Initialize the WebView2 environment with robust error handling
                try
                {
                    await webView.EnsureCoreWebView2Async(null);


                    // Additional safety check
                    if (webView.CoreWebView2 == null)
                    {
                        throw new InvalidOperationException("CoreWebView2 initialization failed");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"WebView2 initialization error: {ex.Message}\nPlease make sure WebView2 Runtime is installed.",
                        "WebView2 Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return; // Exit the method to prevent further errors
                }
                webView.Source = new Uri(url);
                try
                {
                    // Make sure CoreWebView2 is initialized before using it
                    if (webView.CoreWebView2 != null)
                    {
                        // Navigate to the specified URL
                        webView.CoreWebView2.Navigate(url);

                        // Set up title changed event handler
                        webView.CoreWebView2.DocumentTitleChanged += (sender, args) =>
                        {
                            if (this.IsDisposed) return;

                            string newTitle = webView.CoreWebView2.DocumentTitle;
                            if (string.IsNullOrEmpty(newTitle))
                                newTitle = "New Tab";

                            // Update the tab title if this is the associated WebView
                            foreach (TabItem tab in tabs)
                            {
                                if (tab.WebView == webView)
                                {
                                    tab.UpdateTitle(newTitle);
                                    break;
                                }
                            }
                        };
                    }
                    else
                    {
                        // If CoreWebView2 is not yet initialized, set up a handler for when it's ready
                        webView.CoreWebView2InitializationCompleted += (s, e) =>
                        {
                            if (this.IsDisposed) return;

                            if (e.IsSuccess && webView.CoreWebView2 != null)
                            {
                                webView.CoreWebView2.Navigate(url);

                                webView.CoreWebView2.DocumentTitleChanged += (sender, args) =>
                                {
                                    if (this.IsDisposed) return;

                                    string newTitle = webView.CoreWebView2.DocumentTitle;
                                    if (string.IsNullOrEmpty(newTitle))
                                        newTitle = "New Tab";

                                    // Update the tab title if this is the associated WebView
                                    foreach (TabItem tab in tabs)
                                    {
                                        if (tab.WebView == webView)
                                        {
                                            tab.UpdateTitle(newTitle);
                                            break;
                                        }
                                    }
                                };
                            }
                        };
                    }
                }
                catch (Exception ex)
                {
                    DebugLogger.LogMSG($"Error initializing WebView2: {ex.Message}");
                }
                await webView.EnsureCoreWebView2Async(null);
                webView.CoreWebView2.NewWindowRequested += (sender, e) =>
                {
                    e.Handled = true; // Prevent the default new window behavior
                    webView.CoreWebView2.Navigate(e.Uri); // Navigate in current tab
                };
                await webView.EnsureCoreWebView2Async(null);
                // Inject JavaScript to handle middle-clicks
                try
                {
                    await webView.EnsureCoreWebView2Async(null);

                    // Handle new window requests (for regular clicks)
                    webView.CoreWebView2.NewWindowRequested += (sender, e) =>
                    {
                        e.Handled = true;
                        webView.CoreWebView2.Navigate(e.Uri);
                    };

                    // Inject JavaScript to handle middle-clicks
                    await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(@"
            document.addEventListener('auxclick', function(e) {
                // Only handle middle-clicks (button 1)
                if (e.button === 1) {
                    const anchor = e.target.closest('a');
                    if (anchor && anchor.href) {
                        e.preventDefault();
                        window.open(anchor.href, '_blank');
                    }
                }
            }, true);
        ");

                    webView.CoreWebView2.Navigate(url);
                }
                catch (Exception ex)
                {
                    DebugLogger.LogMSG($"Error initializing WebView: {ex.Message}");
                }
                webView.CoreWebView2.Navigate(url);

                // Create a tab button with dark theme
                Panel tabButton = new Panel
                {
                    Size = new Size(TabWidth - 10, TabHeight),
                    BackColor = normalTabColor,
                    Location = new Point(5, tabs.Count * TabHeight + 5), // Position with spacing
                    Margin = new Padding(5),
                    Cursor = Cursors.Hand
                };

                // Add title label to tab
                Label titleLabel = new Label
                {
                    Text = title,
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Size = new Size(TabWidth - 40, TabHeight),
                    Location = new Point(30, 0), // Move right to make space for favicon
                    AutoEllipsis = true,
                    ForeColor = textColor,
                    Cursor = Cursors.Hand
                };

                // Add favicon image (placeholder)
                PictureBox faviconBox = new PictureBox
                {
                    Size = new Size(16, 16),
                    Location = new Point(8, 12),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Cursor = Cursors.Hand
                };

                Button closeButton = new Button
                {
                    Text = "×",
                    Size = new Size(20, 20),
                    Location = new Point(TabWidth - 35, 10),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = textColor, // Keep original text color (white)
                    BackColor = Color.Transparent,
                    
                };

                // Customize hover effects
                closeButton.FlatAppearance.BorderSize = 0;
                closeButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 80, 80); // Red highlight on hover
                closeButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(200, 60, 60); // Darker red when clicked

                // Make the × symbol turn red on hover
           
                closeButton.MouseLeave += (s, e) => closeButton.ForeColor = textColor;

                // Add controls to tab button
                tabButton.Controls.Add(closeButton);
                tabButton.Controls.Add(titleLabel);
                tabButton.Controls.Add(faviconBox);

                // Create the tab item
                int index = tabs.Count;
                TabItem tabItem = new TabItem(index, tabButton, webView, titleLabel, faviconBox);
                tabs.Add(tabItem);

                // Set up event handlers for tab selection
                tabButton.Click += (s, e) => SelectTab(index);

                // Set up event handlers for all child controls to also trigger tab selection
                foreach (Control control in tabButton.Controls)
                {
                    control.Click += (s, e) => SelectTab(index);
                    control.MouseEnter += (s, e) => tabButton.BackColor = Color.FromArgb(60, 60, 60);
                    control.MouseLeave += (s, e) =>
                    {
                        if (index != selectedIndex)
                            tabButton.BackColor = normalTabColor;
                    };
                }

                // Special handling for close button to prevent event bubbling
                closeButton.Click += (s, e) =>
                {
                    // Prevent the event from bubbling up to the parent panel
                    ((Control)s).MouseCaptureChanged += (sender, args) => CloseTab(index);
                    return;
                };

                // Hover effects for the tab button
                tabButton.MouseEnter += (s, e) =>
                {
                    if (index != selectedIndex)
                        tabButton.BackColor = Color.FromArgb(60, 60, 60);
                };

                tabButton.MouseLeave += (s, e) =>
                {
                    if (index != selectedIndex)
                        tabButton.BackColor = normalTabColor;
                };

                // Add the tab button to the tab panel
                tabPanel.Controls.Add(tabButton);

                // Reposition the add button
                RepositionAddButton();

                // Select the new tab
                SelectTab(index);

                // Try to load the favicon
                try
                {
                    webView.CoreWebView2.NavigationCompleted += (s, e) =>
                    {
                        if (e.IsSuccess)
                        {
                            throw new NotImplementedException();
                        }
                    };
                }
                catch (Exception ex)
                {
                    DebugLogger.LogMSG($"Error loading favicon: {ex.Message}");
                }

                // Refresh the tab panel
                tabPanel.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding tab: {ex.Message}\nStack Trace: {ex.StackTrace}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private readonly TabPersistenceService _tabPersistence = new TabPersistenceService();
        private readonly object _saveLock = new object();
        public Task SaveOpenTabsAsync()
        {
            // Create local variable outside the lock
            List<string> urls;

            lock (_saveLock)
            {
                urls = tabs.Select(t => t.WebView?.Source?.ToString())
                         .Where(url => !string.IsNullOrWhiteSpace(url))
                         .ToList();
            }

            // Call the async method outside the lock
            return _tabPersistence.SaveTabsAsync(urls);
        }

        public Task<List<string>> LoadOpenTabsAsync()
        {
            return _tabPersistence.LoadTabsAsync();
        }

       
        /// <summary>
        /// Selects the tab at the specified index
        /// </summary>

        public void SelectTab(int index)
        {
            try
            {
                if (tabs == null || index < 0 || index >= tabs.Count)
                    return;

                // Hide previously selected webview
                if (selectedIndex >= 0 && selectedIndex < tabs.Count)
                {
                    tabs[selectedIndex].WebView.Visible = false;
                    tabs[selectedIndex].TabButton.BackColor = normalTabColor;
                }

                // Show newly selected webview
                selectedIndex = index;
                tabs[index].WebView.Visible = true;
                tabs[index].TabButton.BackColor = selectedTabColor;
                this.FindForm()?.Focus();

                // Bring webview to front
                tabs[index].WebView.BringToFront();

                // Fire the tab selected event
                TabSelected?.Invoke(this, new TabEventArgs(index));
            }
            catch (Exception ex)
            {
                DebugLogger.LogMSG($"Error selecting tab: {ex.Message}");
            }
        }
        public void SubscribeToNavigationEvents(WebView2 webView)
        {
            if (webView?.CoreWebView2 != null)
            {
                webView.NavigationCompleted += (sender, e) =>
                {
                    // Fire the TabSelected event to notify the form
                    if (GetTabIndex(webView) >= 0)
                    {
                        TabSelected?.Invoke(this, new TabEventArgs(GetTabIndex(webView)));
                    }
                };
            }
        }

        // Then update the AddNewTab method to call this - add this in the AddNewTab method after creating the webView
        private void SubscribeToEventsForNewTab(WebView2 webView)
        {
            webView.NavigationCompleted += (sender, e) =>
            {
                // Notify listeners that navigation completed
                TabSelected?.Invoke(this, new TabEventArgs(GetTabIndex(webView)));

                // Update the tab title
                if (e.IsSuccess && webView.CoreWebView2 != null)
                {
                    string newTitle = webView.CoreWebView2.DocumentTitle;
                    if (string.IsNullOrEmpty(newTitle))
                        newTitle = "New Tab";

                    // Find and update the tab
                    int index = GetTabIndex(webView);
                    if (index >= 0 && index < tabs.Count)
                    {
                        tabs[index].UpdateTitle(newTitle);
                    }
                }
            };
        }
        private List<WebView2> _tabs = new List<WebView2>();
        private List<Image> _tabIcons = new List<Image>();


        public void SetTabIcon(int index, Image icon)
        {
            if (index >= 0 && index < _tabs.Count)
            {
                // Dispose old icon if exists
                if (_tabIcons[index] != null)
                    _tabIcons[index].Dispose();

                _tabIcons[index] = icon;
                Invalidate(); // Redraw control
            }
        }

        /// <summary>
        /// Closes the tab at the specified index
        /// </summary>
        public void CloseTab(int index)
        {
            try
            {
                if (tabs == null || index < 0 || index >= tabs.Count)
                    return;

                TabItem tabToClose = tabs[index];

                // Remove the tab from the panel
                if (tabPanel != null && !tabPanel.IsDisposed &&
                    tabToClose.TabButton != null && !tabToClose.TabButton.IsDisposed)
                {
                    tabPanel.Controls.Remove(tabToClose.TabButton);
                }

                // Get the form that contains this control
                Form parentForm = this.FindForm();
                if (parentForm != null && tabToClose.WebView != null && !tabToClose.WebView.IsDisposed)
                {
                    parentForm.Controls.Remove(tabToClose.WebView);
                }

                // Remove from our list
                tabs.RemoveAt(index);

                // Dispose of resources
                if (tabToClose.WebView != null && !tabToClose.WebView.IsDisposed)
                    tabToClose.WebView.Dispose();

                if (tabToClose.TabButton != null && !tabToClose.TabButton.IsDisposed)
                    tabToClose.TabButton.Dispose();

                // Reposition remaining tabs
                for (int i = 0; i < tabs.Count; i++)
                {
                    tabs[i].Index = i;
                    if (!tabs[i].TabButton.IsDisposed)
                        tabs[i].TabButton.Location = new Point(5, i * TabHeight + 5);
                }

                // Reposition the add button
                RepositionAddButton();

                // Fire the tab closed event
                TabClosed?.Invoke(this, new TabEventArgs(index));

                // If we closed the selected tab
                if (index == selectedIndex)
                {
                    // Select the next tab, or the previous one if this was the last tab
                    if (tabs.Count > 0)
                    {
                        int newIndex = index < tabs.Count ? index : tabs.Count - 1;
                        SelectTab(newIndex);
                    }
                    else
                    {
                        selectedIndex = -1;
                    }
                }
                // If we closed a tab before the selected one, adjust the selected index
                else if (index < selectedIndex)
                {
                    selectedIndex--;
                }

                // Refresh the tab panel
                if (tabPanel != null && !tabPanel.IsDisposed)
                    tabPanel.Refresh();
            }
            catch (Exception ex)
            {
                DebugLogger.LogMSG($"Error closing tab: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the currently selected WebView2 control
        /// </summary>
        public WebView2 CurrentWebView
        {
            get
            {
                if (tabs != null && selectedIndex >= 0 && selectedIndex < tabs.Count)
                    return tabs[selectedIndex].WebView;
                return null;
            }
        }

        /// <summary>
        /// Gets the URL of the currently selected tab
        /// </summary>
        public string CurrentUrl
        {
            get
            {
                if (CurrentWebView != null)
                    return CurrentWebView.Source?.ToString();
                return string.Empty;
            }
        }
        public ImageList _tabImages = new ImageList();
        public void SetTabImage(int tabIndex, Image image)
        {
            if (tabIndex >= 0 && tabIndex < TabCount)
            {
                // Add to ImageList and assign index
                _tabImages.Images.Add(image);
            }
            
        }
        /// <summary>
        /// Class to store information about a tab
        /// </summary>
        private class TabItem
        {
            public int Index { get; set; }
            public Panel TabButton { get; }
            public WebView2 WebView { get; }
            private Label TitleLabel { get; }
            private PictureBox FaviconBox { get; }

            public TabItem(int index, Panel tabButton, WebView2 webView, Label titleLabel, PictureBox faviconBox)
            {
                Index = index;
                TabButton = tabButton;
                WebView = webView;
                TitleLabel = titleLabel;
                FaviconBox = faviconBox;

                // Make child controls not interfere with mouse events
                TitleLabel.Cursor = Cursors.Default;
                FaviconBox.Cursor = Cursors.Default;
            }

            public void UpdateTitle(string title)
            {
                try
                {
                    if (TitleLabel == null || TitleLabel.IsDisposed)
                        return;

                    if (TitleLabel.InvokeRequired)
                    {
                        TitleLabel.Invoke(new Action(() => {
                            if (!TitleLabel.IsDisposed)
                                TitleLabel.Text = title;
                        }));
                    }
                    else
                    {
                        TitleLabel.Text = title;
                    }
                }
                catch (Exception ex)
                {
                    DebugLogger.LogError($"Error updating tab title: {ex.Message}");
                }
            }

            public void UpdateFavicon(Image favicon)
            {
                try
                {
                    if (FaviconBox == null || FaviconBox.IsDisposed)
                        return;

                    if (FaviconBox.InvokeRequired)
                    {
                        FaviconBox.Invoke(new Action(() => {
                            if (!FaviconBox.IsDisposed)
                                FaviconBox.Image = favicon;
                        }));
                    }
                    else
                    {
                        FaviconBox.Image = favicon;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating favicon: {ex.Message}");
                }
            }
        }

    }

    /// <summary>
    /// Event arguments for tab events
    /// </summary>
    public class TabEventArgs : EventArgs
    {
        public int TabIndex { get; }

        public TabEventArgs(int tabIndex)
        {
            TabIndex = tabIndex;
        }
    }
}
