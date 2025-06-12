using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using Spijuniro_Golubiro;

namespace TralaleroTralala
{
    public class IncognitoForm : Form
    {
        private readonly Incognito _incognitoService;
        public WebView2 WebView { get; private set; }
        private bool _isInitialized = false;

        public IncognitoForm()
        {
            // Set basic form properties
            this.Text = "Spijuniro Golubiro";
            this.Icon = Properties.Resources.Incognito; // Make sure this icon exists in resources
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Create progress indicator
            Label loadingLabel = new Label
            {
                Text = "Loading incognito mode...",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(Font.FontFamily, 12),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(50, 50, 50)
            };
            this.Controls.Add(loadingLabel);

            // Initialize incognito service
            _incognitoService = new Incognito();

            // Handle form closing
            this.FormClosing += IncognitoForm_FormClosing;

            // Initialize WebView2 asynchronously
            InitializeIncognitoAsync();
        }

        private async void InitializeIncognitoAsync()
        {
            try
            {
                // Create WebView2 with incognito settings
                WebView = await _incognitoService.CreateIncognitoWindowAsync(this);

                if (WebView != null)
                {
                    // Initialize was successful
                    _isInitialized = true;

                    // Configure WebView2
                    WebView.Source = new Uri("https://www.google.com");

                    // Stop cookie and history persistence
                    await _incognitoService.StopCookieAndHistoryAndSettingsImportAsync();

                    // Remove loading label
                    foreach (Control control in Controls)
                    {
                        if (control is Label label && label.Text == "Loading incognito mode...")
                        {
                            Controls.Remove(label);
                            label.Dispose();
                            break;
                        }
                    }
                }
                else
                {
                    // WebView creation failed
                    MessageBox.Show("Failed to initialize incognito mode. The window will close.",
                        "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing incognito browser: {ex.Message}",
                    "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private async void IncognitoForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // If initialization didn't complete, no cleanup needed
            if (!_isInitialized) return;

            try
            {
                // Set flag to show we're cleaning up
                _isInitialized = false;

                // Cleanup WebView2 and temporary files
                await _incognitoService.CleanupAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during incognito cleanup: {ex.Message}");
            }
        }
    }
}