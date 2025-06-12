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
    // Form1 extension class
    public static class Form1Extensions
    {
        // Extension method to update Form1 with our new functionality
        public static void EnableRedirectionAndFaviconSupport(this Form1 form, VerticalTabControl tabControl)
        {
            if (form == null || tabControl == null) return;

            try
            {
                // Add redirection handling
                tabControl.AddRedirectionHandling();

                // Add favicon support
                tabControl.AddFaviconSupport();

                // Log successful integration
                Console.WriteLine("Successfully added redirection handling and favicon support");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting up browser extensions: {ex.Message}",
                    "Extension Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Helper method to modify existing Form1 initialization code
        public static void UpdateInitializeTabControl(this Form1 form)
        {
            // This method shows the changes that need to be made to the Form1.InitializeTabControl method
            // You should manually integrate these changes into your actual InitializeTabControl method

            /*
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
            */
        }
    }
}