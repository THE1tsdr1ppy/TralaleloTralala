using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using TralaleroTralala.DownloadMGR;

namespace TralaleroTralala
{
    public partial class Form1 : Form
    {
        public string homePage = "https://www.google.com";
        public bool isUrlBeingUpdatedByNavigation = false;
        private VerticalTabControl tabControl;
        private readonly string _dataDirectory;
        private readonly string _tabsFilePath;


        private ContextMenuStrip _contextMenu;

        public Form1()
        {
            InitializeTabSaving();
            InitializeComponent();
            InitializeNavigationControls();
            InitializeContextMenu();
            InitializeTabControl();
            this.WindowState = FormWindowState.Maximized;
            //set in designer this.Text = "Tralalero Tralala";
            this.KeyPreview = true;
            // Load saved tabs after initialization
            LoadSavedTabsAfterInitialization();
            _dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "browsingData");
            _tabsFilePath = Path.Combine(_dataDirectory, "saved_tabs.txt");
            var silentDownload = new SilentDownload(webView2, incognitoMode: true);

            silentDownload.DownloadCompleted += (s, e) => {
                if (e.Success)
                {
                    // Process downloaded data in memory
                    var data = e.Data;
                    // Or save to a specific location if needed
                    // silentDownload.SaveDownloadToFile(e.Id, @"C:\temp\myfile.pdf");
                }
            };
        }

        private void InitializeContextMenu()
        {
            _contextMenu = new ContextMenuStrip();

            // Add debug console item
            var debugConsoleItem = new ToolStripMenuItem("Debug Console");
            debugConsoleItem.Click += (s, e) => DebugConsole.Instance.Show();

            // Add other menu items if needed
            var clearLogsItem = new ToolStripMenuItem("Clear Logs");
            clearLogsItem.Click += (s, e) => DebugConsole.Instance.Clear();

            _contextMenu.Items.Add(debugConsoleItem);
            _contextMenu.Items.Add(clearLogsItem);

            // Assign the context menu to the form
            this.ContextMenuStrip = _contextMenu;
        }


        private async void InitializeTabControl()
        {
            try
            {
                // Safely check if webView21 exists and is part of Controls collection before removing
                if (webView21 != null && this.Controls.Contains(webView21))
                {
                    this.Controls.Remove(webView21);
                    webView21.Dispose();
                }

                // Create the tab control
                tabControl = new VerticalTabControl();

                // Position and size it explicitly to match panel1's location and size
                tabControl.Location = new Point(12, 6);
                tabControl.Size = new Size(280, 765);

                // Add it directly to the form (not to panel1)
                this.Controls.Add(tabControl);

                // Set proper Z-order for the tab control
                tabControl.BringToFront();

                // Add event handlers
                tabControl.TabSelected += TabControl_TabSelected;
                tabControl.TabClosed += TabControl_TabClosed;

                // Add our new functionality
                this.EnableRedirectionAndFaviconSupport(tabControl);

                // Wait a bit to ensure the form is fully loaded
                await Task.Delay(100);
                if (File.Exists(_tabsFilePath) != true)
                {
                    // Create initial tabs
                    tabControl.AddNewTab("Google", "https://www.google.com");
                    // Wait a bit between tab creations to avoid race conditions
                    await Task.Delay(100);
                    tabControl.AddNewTab("Bing", "https://www.bing.com");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing tab control: {ex.Message}\n\nStack Trace: {ex.StackTrace}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            // Check if this was triggered by our middle-click JavaScript
            if (e.IsUserInitiated)
            {
                // Open in new tab
                tabControl.AddNewTab("New Tab", e.Uri);
            }
            else
            {
                // Regular click - open in current tab
                if (tabControl?.CurrentWebView?.CoreWebView2 != null)
                {
                    tabControl.CurrentWebView.CoreWebView2.Navigate(e.Uri);
                }
            }
            e.Handled = true;
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Debug output to verify the method is being called
            Console.WriteLine($"ProcessCmdKey called with: {keyData}, Msg: {msg.Msg}");

            try
            {
                // Handle our global shortcuts first, regardless of focus or message type
                switch (keyData)
                {
                    case Keys.Control | Keys.T:
                        Debug.WriteLine("CTRL+T detected, creating new tab");
                        tabControl?.AddNewTab("New Tab", "https://www.google.com");
                        return true;

                    case Keys.Control | Keys.Shift | Keys.N:
                        Debug.WriteLine("CTRL+SHIFT+N detected, opening incognito window");
                        OpenIncognitoWindow();
                        return true;

                    case Keys.Control | Keys.W:
                        Debug.WriteLine("CTRL+W detected, closing current tab");
                        if (tabControl?.CurrentWebView != null)
                        {
                            int index = tabControl.GetTabIndex(tabControl.CurrentWebView);
                            if (index >= 0) tabControl.CloseTab(index);
                        }
                        return true;

                    case Keys.Control | Keys.Tab:
                        Debug.WriteLine("CTRL+TAB detected, switching to next tab");
                        if (tabControl?.TabCount > 1)
                        {
                            int newIndex = (tabControl.SelectedIndex + 1) % tabControl.TabCount;
                            tabControl.SelectTab(newIndex);
                        }
                        return true;

                    case Keys.Control | Keys.Shift | Keys.Tab:
                        Debug.WriteLine("CTRL+SHIFT+TAB detected, switching to previous tab");
                        if (tabControl?.TabCount > 1)
                        {
                            int newIndex = (tabControl.SelectedIndex - 1 + tabControl.TabCount) % tabControl.TabCount;
                            tabControl.SelectTab(newIndex);
                        }
                        return true;

                    case Keys.F5:
                        Debug.WriteLine("F5 detected, refreshing page");
                        tabControl?.CurrentWebView?.Reload();
                        return true;

                    case Keys.Control | Keys.L:
                        Debug.WriteLine("CTRL+L detected, focusing address bar");
                        richTextBox1.Focus();
                        richTextBox1.SelectAll();
                        return true;

                    case Keys.End | Keys.C:
                        DebugLogger.LogMSG("End+C : starting silent dowload");
                        
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ProcessCmdKey: {ex.Message}");
            }

            // Let base class handle any other key combinations
            return base.ProcessCmdKey(ref msg, keyData);
        }
        private void OpenIncognitoWindow()
        {
            try
            {
                DebugLogger.LogMSG("Opening incognito window...");
                var incognitoForm = new IncognitoForm();
                incognitoForm.Show();
                Console.WriteLine("Incognito window opened successfully");
            }
            catch (Exception ex)
            {
                DebugLogger.LogMSG($"Error opening incognito window: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Failed to open incognito window: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void UpdateTabIcon(WebView2 webView, Image image)
        {
            // Implement your tab icon update logic here
            // For example, if using a custom tab control:
            int tabIndex = tabControl.GetTabIndex(webView);
            if (tabIndex >= 0)
            {
                tabControl.SetTabIcon(tabIndex, image);
            }
        }
        private void TabControl_TabSelected(object sender, TabEventArgs e)
        {
            // Update UI or perform actions when a tab is selected
            if (tabControl.CurrentWebView != null && tabControl.CurrentUrl != null)
            {
                this.Text = "Tralalero Tralala";
            }
        }

        private async void TabControl_TabClosed(object sender, TabEventArgs e)
        {
            // Handle tab closed event if needed
            if (tabControl.CurrentWebView == null)
            {
                this.Text = "Tralalero Tralala";
            }

            // Save tabs state when a tab is closedWWW
            await SaveCurrentTabsAsync();
        }

        // Add this method to handle tab navigation completed events
        private async void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                // Save the tabs state after successful navigation
                await SaveCurrentTabsAsync();
            }
        }

        [DllImport("DwmApi")] //System.Runtime.InteropServices
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, int[] attrValue, int attrSize);
        protected override void OnHandleCreated(EventArgs e)
        {
            if (DwmSetWindowAttribute(Handle, 19, new[] { 1 }, 4) != 0)
                DwmSetWindowAttribute(Handle, 20, new[] { 1 }, 4);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
        private Timer resourceMonitorTimer;

     

       
        #region nav
        private void InitializeNavigationControls()
        {
            // URL bar events
            richTextBox1.KeyDown += UrlBar_KeyDown;
            richTextBox1.Leave += UrlBar_Leave;

            // Button click events
            BackB.Click += (s, e) => BackButton_Click();
            ForwardB.Click += (s, e) => ForwardButton_Click();
            ReloadB.Click += (s, e) => ReloadButton_Click();
            HomeB.Click += (s, e) => HomeButton_Click();

            // Set cursor to hand for buttons
            BackB.Cursor = Cursors.Hand;
            ForwardB.Cursor = Cursors.Hand;
            ReloadB.Cursor = Cursors.Hand;
            HomeB.Cursor = Cursors.Hand;

            //border
            BackB.FlatAppearance.BorderSize = 0;
            ForwardB.FlatAppearance.BorderSize = 0;
            ReloadB.FlatAppearance.BorderSize = 0;
            //home is an image not text so i can just set color

            // Add hover effects
            AddHoverEffect(BackB);
            AddHoverEffect(ForwardB);
            AddHoverEffect(ReloadB);
            AddHoverEffect(HomeB);
        }

        private void AddHoverEffect(Button button)
        {
            Color originalColor = button.BackColor;

            button.MouseEnter += (s, e) => button.BackColor = Color.FromArgb(50, 50, 50);
            button.MouseLeave += (s, e) => button.BackColor = originalColor;
        }

        private void UrlBar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                NavigateToUrl(richTextBox1.Text);
                e.Handled = e.SuppressKeyPress = true;
            }
        }

        private void UrlBar_Leave(object sender, EventArgs e)
        {
            UpdateUrlBarText();
        }

        private void UpdateUrlBarText()
        {
            if (tabControl?.CurrentWebView?.CoreWebView2?.Source != null && !isUrlBeingUpdatedByNavigation)
            {
                richTextBox1.Text = tabControl.CurrentWebView.CoreWebView2.Source;
            }
        }

        private void NavigateToUrl(string url)
        {
            if (tabControl?.CurrentWebView == null) return;

            // Add https:// if not present and doesn't start with http://
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                // Check if it's a valid domain (simplified check)
                if (Regex.IsMatch(url, @"^[a-zA-Z0-9-]+\.[a-zA-Z]{2,}$") ||
                    url.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                {
                    url = "https://" + url;
                }
                else
                {
                    // Treat as search query
                    url = $"https://www.google.com/search?q={Uri.EscapeDataString(url)}";
                }
            }

            try
            {
                tabControl.CurrentWebView.CoreWebView2.Navigate(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error navigating to URL: {ex.Message}", "Navigation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BackButton_Click()
        {
            if (tabControl?.CurrentWebView?.CoreWebView2?.CanGoBack == true)
            {
                tabControl.CurrentWebView.CoreWebView2.GoBack();
            }
        }

        private void ForwardButton_Click()
        {
            if (tabControl?.CurrentWebView?.CoreWebView2?.CanGoForward == true)
            {
                tabControl.CurrentWebView.CoreWebView2.GoForward();
            }
        }

        private void ReloadButton_Click()
        {
            tabControl?.CurrentWebView?.Reload();
        }

        private void HomeButton_Click()
        {
            NavigateToUrl(homePage);
        }

        #endregion


        private System.Timers.Timer _saveTimer;
        private bool _isSaving;

        private readonly TabPersistenceService _tabPersistence = new TabPersistenceService();

        // Update the InitializeTabSaving method
        private void InitializeTabSaving()
        {
            // Set up periodic saving (every 30 seconds)
            _saveTimer = new System.Timers.Timer(30000) { AutoReset = true };
            _saveTimer.Elapsed += async (s, e) => await SaveCurrentTabsAsync();
            _saveTimer.Start();

            // Save on exit
            this.FormClosing += async (s, e) =>
            {
                _saveTimer.Stop();
                if (!_isSaving)
                {
                    await SaveCurrentTabsAsync();
                }
            };
        }

        // Update the SaveCurrentTabsAsync method
        private async Task SaveCurrentTabsAsync()
        {
            if (_isSaving || tabControl == null) return;

            _isSaving = true;
            try
            {
                var urls = new List<string>();

                for (int i = 0; i < tabControl.TabCount; i++)
                {
                    var webView = tabControl.GetWebViewAt(i);
                    if (webView?.CoreWebView2 != null)
                    {
                        string currentUrl = webView.CoreWebView2.Source;
                        if (!string.IsNullOrWhiteSpace(currentUrl))
                        {
                            urls.Add(currentUrl);
                            Debug.WriteLine($"Saving tab with URL: {currentUrl}");
                        }
                    }
                }

                Debug.WriteLine($"Saving {urls.Count} tabs");

                if (urls.Count > 0)
                {
                    await _tabPersistence.SaveTabsAsync(urls);
                    Debug.WriteLine("Tabs saved successfully");
                }
                else
                {
                    Debug.WriteLine("No tabs to save");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving tabs: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                _isSaving = false;
            }
        }

        // Update the LoadSavedTabsAsync method
        private async Task LoadSavedTabsAsync()
        {
            try
            {
                if (tabControl == null)
                {
                    Debug.WriteLine("Error: tabControl is not initialized.");
                    return;
                }

                // Load saved URLs
                var savedUrls = await _tabPersistence.LoadTabsAsync();

                // Log what we're loading for debugging
                Debug.WriteLine($"Loading {savedUrls.Count} saved tabs");
                foreach (var url in savedUrls)
                {
                    Debug.WriteLine($"URL to load: {url}");
                }

                if (savedUrls != null && savedUrls.Count > 0)
                {
                    // Close any existing tabs
                    while (tabControl.TabCount > 0)
                    {
                        tabControl.CloseTab(0);
                        await Task.Delay(50); // Give UI time to update
                    }

                    // Restore saved tabs
                    foreach (var url in savedUrls)
                    {
                        if (!string.IsNullOrWhiteSpace(url))
                        {
                            Debug.WriteLine($"Adding tab for URL: {url}");
                            tabControl.AddNewTab("Loading...", url);
                            await Task.Delay(200); // Slightly longer delay between tab creations
                        }
                    }

                    // Make sure at least one tab is selected
                    if (tabControl.TabCount > 0)
                    {
                        tabControl.SelectTab(0);
                    }
                }
                else
                {
                    Debug.WriteLine("No saved tabs found or loading failed. Opening default tabs.");

                    // If no saved tabs, open default tabs
                    // First close any existing tabs
                    while (tabControl.TabCount > 0)
                    {
                        tabControl.CloseTab(0);
                        await Task.Delay(50);
                    }

                    // Create default tabs
                    tabControl.AddNewTab("Google", "https://www.google.com");
                    await Task.Delay(200);
                    tabControl.AddNewTab("Bing", "https://www.bing.com");

                    // Select the first tab
                    tabControl.SelectTab(0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading saved tabs: {ex.Message}\n{ex.StackTrace}");

                // Fallback to default tab if loading fails
                if (tabControl.TabCount == 0)
                {
                    tabControl.AddNewTab("Google", "https://www.google.com");
                }
            }
        }
        private async void LoadSavedTabsAfterInitialization()
        {
            // Wait a moment for the UI to fully initialize
            await Task.Delay(500);

            // Now load the saved tabs
            await LoadSavedTabsAsync();
        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}