using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using TralaleroTralala;
namespace Spijuniro_Golubiro
{
    public class Incognito
    {
        private WebView2 _incognitoWebView;
        private string _tempUserDataFolder;
        private CoreWebView2Environment _environment;
        private bool _isInitialized = false;

        public async Task<WebView2> CreateIncognitoWindowAsync(Form parentForm)
        {
            try
            {
                // Create a temporary folder for incognito session
                _tempUserDataFolder = Path.Combine(Path.GetTempPath(), "TralaleloIncognito_" + Guid.NewGuid().ToString());
                Directory.CreateDirectory(_tempUserDataFolder);

                // Configure WebView2 for incognito mode
                var webView = new WebView2
                {
                    Dock = DockStyle.Fill,
                    Visible = false
                };

                parentForm.Controls.Add(webView);

                // Configure environment options
                var envOptions = new CoreWebView2EnvironmentOptions
                {
                    AdditionalBrowserArguments = "--inprivate",
                    AllowSingleSignOnUsingOSPrimaryAccount = false
                };

                try
                {
                    // Create environment asynchronously
                    _environment = await CoreWebView2Environment.CreateAsync(
                        browserExecutableFolder: null,
                        userDataFolder: _tempUserDataFolder,
                        options: envOptions);

                    // Initialize WebView2 with the environment
                    await webView.EnsureCoreWebView2Async(_environment);

                    // Configure privacy settings
                    ConfigurePrivacySettings(webView);

                    // Make the WebView visible
                    webView.BringToFront();
                    webView.Visible = true;

                    _incognitoWebView = webView;
                    _isInitialized = true;
                    return webView;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to initialize WebView2: {ex.Message}\n\n{ex.StackTrace}",
                        "Incognito Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    // Clean up if initialization failed
                    parentForm.Controls.Remove(webView);
                    webView.Dispose();
                    CleanupTempFolder();
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create incognito window: {ex.Message}",
                    "Incognito Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CleanupTempFolder();
                return null;
            }
        }

        private void ConfigurePrivacySettings(WebView2 webView)
        {
            if (webView?.CoreWebView2 != null)
            {
                try
                {
                    // Configure WebView2 settings for privacy
                    webView.CoreWebView2.Settings.IsGeneralAutofillEnabled = false;
                    webView.CoreWebView2.Settings.IsPasswordAutosaveEnabled = false;
                    webView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = true; // Keep keyboard shortcuts working
                    webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true; // Keep context menus
                    webView.CoreWebView2.Settings.IsZoomControlEnabled = true; // Keep zoom functionality

                    // Add navigation events
                    webView.CoreWebView2.HistoryChanged += (s, e) => {
                        // Refresh title when navigation occurs
                        if (webView.Parent is Form form)
                        {
                            form.Invoke(new Action(() => {
                                form.Text = $"Incognito - {webView.CoreWebView2.DocumentTitle}";
                            }));
                        }
                    };
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error configuring privacy settings: {ex.Message}");
                }
            }
        }

        public async Task StopCookieAndHistoryAndSettingsImportAsync()
        {
            try
            {
                if (_incognitoWebView?.CoreWebView2?.Profile != null)
                {
                    // Clear any existing data
                    await _incognitoWebView.CoreWebView2.Profile.ClearBrowsingDataAsync(
                        CoreWebView2BrowsingDataKinds.AllProfile);

                    // Set dark mode for incognito
                    _incognitoWebView.CoreWebView2.Profile.PreferredColorScheme =
                        CoreWebView2PreferredColorScheme.Dark;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Privacy cleanup failed: {ex.Message}");
            }
        }

        private void CleanupTempFolder()
        {
            try
            {
                // Delete temporary folder with retries
                if (!string.IsNullOrEmpty(_tempUserDataFolder) && Directory.Exists(_tempUserDataFolder))
                {
                    try
                    {
                        Directory.Delete(_tempUserDataFolder, true);
                    }
                    catch (IOException)
                    {
                        // If delete fails, schedule it for deletion after the app exits
                        Application.ApplicationExit += (s, e) => {
                            try { Directory.Delete(_tempUserDataFolder, true); } catch { }
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Temp folder cleanup failed: {ex.Message}");
            }
        }

        public async Task CleanupAsync()
        {
            if (!_isInitialized) return;

            try
            {
                // Clear browsing data before disposal
                if (_incognitoWebView?.CoreWebView2?.Profile != null)
                {
                    try
                    {
                        await _incognitoWebView.CoreWebView2.Profile.ClearBrowsingDataAsync(
                            CoreWebView2BrowsingDataKinds.AllProfile);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to clear browsing data: {ex.Message}");
                    }
                }

                // Dispose WebView2 control properly
                if (_incognitoWebView != null)
                {
                    // Remove event handlers
                    if (_incognitoWebView.CoreWebView2 != null)
                    {
                        // Remove any event handlers here
                    }

                    _incognitoWebView.Dispose();
                    _incognitoWebView = null;
                }

                // Clean up the environment
                _environment = null;

                // Delete temporary folder
                CleanupTempFolder();

                _isInitialized = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Incognito cleanup failed: {ex.Message}");
            }
        }
    }
}