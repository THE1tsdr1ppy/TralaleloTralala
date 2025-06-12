using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.WinForms;

namespace TralaleroTralala
{
    // This class provides helper methods for comparing WebView2 instances
    public static class WebViewExtensions
    {
        // Custom comparison method for WebView2 objects
        public static bool AreEqual(this WebView2 webView1, WebView2 webView2)
        {
            if (webView1 is null && webView2 is null) return true;
            if (webView1 is null || webView2 is null) return false;

            // Compare by reference
            return ReferenceEquals(webView1, webView2);
        }

        // Extension method to replace direct comparison
        public static bool IsEqualTo(this WebView2 webView1, WebView2 webView2)
        {
            return AreEqual(webView1, webView2);
        }
    }
}