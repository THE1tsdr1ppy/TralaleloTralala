using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TralaleroTralala.Properties;

namespace TralaleroTralala
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView2;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.panel1 = new System.Windows.Forms.Panel();
            this.webView21 = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.webView2 = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.panel5 = new System.Windows.Forms.Panel();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.HomeButton = new System.Windows.Forms.Panel();
            this.HomeB = new System.Windows.Forms.Button();
            this.backButton = new System.Windows.Forms.Panel();
            this.BackB = new System.Windows.Forms.Button();
            this.forwardButton = new System.Windows.Forms.Panel();
            this.ForwardB = new System.Windows.Forms.Button();
            this.Reload = new System.Windows.Forms.Panel();
            this.ReloadB = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.Bookmarks = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.webView21)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.webView2)).BeginInit();
            this.panel5.SuspendLayout();
            this.HomeButton.SuspendLayout();
            this.backButton.SuspendLayout();
            this.forwardButton.SuspendLayout();
            this.Reload.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(27)))), ((int)(((byte)(27)))));
            this.panel1.Location = new System.Drawing.Point(12, 6);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(280, 765);
            this.panel1.TabIndex = 0;
            // 
            // webView21
            // 
            this.webView21.AllowExternalDrop = true;
            this.webView21.CreationProperties = null;
            this.webView21.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webView21.Location = new System.Drawing.Point(298, 62);
            this.webView21.Name = "webView21";
            this.webView21.Size = new System.Drawing.Size(1620, 967);
            this.webView21.TabIndex = 2;
            this.webView21.ZoomFactor = 1D;
            // 
            // webView2
            // 
            this.webView2.AllowExternalDrop = true;
            this.webView2.CreationProperties = null;
            this.webView2.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webView2.Location = new System.Drawing.Point(0, 0);
            this.webView2.Name = "webView2";
            this.webView2.Size = new System.Drawing.Size(0, 0);
            this.webView2.TabIndex = 0;
            this.webView2.ZoomFactor = 1D;
            // 
            // panel5
            // 
            this.panel5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(27)))), ((int)(((byte)(27)))));
            this.panel5.Controls.Add(this.richTextBox1);
            this.panel5.Location = new System.Drawing.Point(556, 6);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(1362, 30);
            this.panel5.TabIndex = 3;
            // 
            // richTextBox1
            // 
            this.richTextBox1.AcceptsTab = true;
            this.richTextBox1.AccessibleDescription = "URLTEXT";
            this.richTextBox1.AccessibleName = "URLTEXT";
            this.richTextBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(27)))), ((int)(((byte)(27)))));
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBox1.ForeColor = System.Drawing.Color.White;
            this.richTextBox1.Location = new System.Drawing.Point(3, 6);
            this.richTextBox1.Multiline = false;
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(1123, 21);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            // 
            // HomeButton
            // 
            this.HomeButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(27)))), ((int)(((byte)(27)))));
            this.HomeButton.Controls.Add(this.HomeB);
            this.HomeButton.Location = new System.Drawing.Point(312, 7);
            this.HomeButton.Name = "HomeButton";
            this.HomeButton.Size = new System.Drawing.Size(49, 50);
            this.HomeButton.TabIndex = 4;
            // 
            // HomeB
            // 
            this.HomeB.AccessibleName = "HomeB";
            this.HomeB.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(27)))), ((int)(((byte)(27)))));
            this.HomeB.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.HomeB.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HomeB.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(27)))), ((int)(((byte)(27)))));
            this.HomeB.Image = ((System.Drawing.Image)(resources.GetObject("HomeB.Image")));
            this.HomeB.Location = new System.Drawing.Point(3, 2);
            this.HomeB.Name = "HomeB";
            this.HomeB.Size = new System.Drawing.Size(43, 45);
            this.HomeB.TabIndex = 0;
            this.HomeB.UseVisualStyleBackColor = false;
            // 
            // backButton
            // 
            this.backButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(27)))), ((int)(((byte)(27)))));
            this.backButton.Controls.Add(this.BackB);
            this.backButton.Location = new System.Drawing.Point(373, 6);
            this.backButton.Name = "backButton";
            this.backButton.Size = new System.Drawing.Size(49, 50);
            this.backButton.TabIndex = 5;
            // 
            // BackB
            // 
            this.BackB.AccessibleName = "BackB";
            this.BackB.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(27)))), ((int)(((byte)(27)))));
            this.BackB.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BackB.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BackB.ForeColor = System.Drawing.Color.White;
            this.BackB.Location = new System.Drawing.Point(0, 1);
            this.BackB.Name = "BackB";
            this.BackB.Size = new System.Drawing.Size(49, 49);
            this.BackB.TabIndex = 7;
            this.BackB.Text = "←";
            this.BackB.UseVisualStyleBackColor = false;
            // 
            // forwardButton
            // 
            this.forwardButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(27)))), ((int)(((byte)(27)))));
            this.forwardButton.Controls.Add(this.ForwardB);
            this.forwardButton.Location = new System.Drawing.Point(434, 6);
            this.forwardButton.Name = "forwardButton";
            this.forwardButton.Size = new System.Drawing.Size(49, 50);
            this.forwardButton.TabIndex = 6;
            // 
            // ForwardB
            // 
            this.ForwardB.AccessibleName = "ForwardB";
            this.ForwardB.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(27)))), ((int)(((byte)(27)))));
            this.ForwardB.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ForwardB.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForwardB.ForeColor = System.Drawing.Color.White;
            this.ForwardB.Location = new System.Drawing.Point(0, 1);
            this.ForwardB.Name = "ForwardB";
            this.ForwardB.Size = new System.Drawing.Size(49, 49);
            this.ForwardB.TabIndex = 7;
            this.ForwardB.Text = "→";
            this.ForwardB.UseVisualStyleBackColor = false;
            // 
            // Reload
            // 
            this.Reload.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(27)))), ((int)(((byte)(27)))));
            this.Reload.Controls.Add(this.ReloadB);
            this.Reload.Location = new System.Drawing.Point(494, 6);
            this.Reload.Name = "Reload";
            this.Reload.Size = new System.Drawing.Size(49, 50);
            this.Reload.TabIndex = 6;
            // 
            // ReloadB
            // 
            this.ReloadB.AccessibleName = "ReloadB";
            this.ReloadB.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(27)))), ((int)(((byte)(27)))));
            this.ReloadB.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ReloadB.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ReloadB.ForeColor = System.Drawing.Color.White;
            this.ReloadB.Location = new System.Drawing.Point(0, 1);
            this.ReloadB.Name = "ReloadB";
            this.ReloadB.Size = new System.Drawing.Size(49, 49);
            this.ReloadB.TabIndex = 7;
            this.ReloadB.Text = "⟳";
            this.ReloadB.UseVisualStyleBackColor = false;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.Bookmarks);
            this.panel2.Location = new System.Drawing.Point(12, 777);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(280, 252);
            this.panel2.TabIndex = 7;
            // 
            // Bookmarks
            // 
            this.Bookmarks.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Bookmarks.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.Bookmarks.Location = new System.Drawing.Point(78, 10);
            this.Bookmarks.Name = "Bookmarks";
            this.Bookmarks.Size = new System.Drawing.Size(112, 32);
            this.Bookmarks.TabIndex = 0;
            this.Bookmarks.Text = "Bookmarks";
            this.Bookmarks.UseMnemonic = false;
            this.Bookmarks.Click += new System.EventHandler(this.label1_Click_1);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(19)))), ((int)(((byte)(19)))), ((int)(((byte)(19)))));
            this.ClientSize = new System.Drawing.Size(1904, 1041);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.Reload);
            this.Controls.Add(this.forwardButton);
            this.Controls.Add(this.backButton);
            this.Controls.Add(this.HomeButton);
            this.Controls.Add(this.panel5);
            this.Controls.Add(this.webView21);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Tralalero Tralala";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.webView21)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.webView2)).EndInit();
            this.panel5.ResumeLayout(false);
            this.HomeButton.ResumeLayout(false);
            this.backButton.ResumeLayout(false);
            this.forwardButton.ResumeLayout(false);
            this.Reload.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView21;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Panel HomeButton;
        private System.Windows.Forms.Panel backButton;
        private System.Windows.Forms.Panel forwardButton;
        private System.Windows.Forms.Panel Reload;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button HomeB;
        private System.Windows.Forms.Button BackB;
        private System.Windows.Forms.Button ForwardB;
        private System.Windows.Forms.Button ReloadB;

        public void SetApplicationIdentity()
        {
            try
            {
                // This helps Windows identify your application properly
                if (Environment.OSVersion.Version.Major >= 6) // Vista or later
                {
                    NativeMethods.SetCurrentProcessExplicitAppUserModelID("TralaleloTralala.UniqueAppID");
                }
            }
            catch { /* Ignore if API not available */ }
        }

        private Panel panel2;
        private Label Bookmarks;

        internal static class NativeMethods
        {
            [DllImport("shell32.dll", SetLastError = true)]
            public static extern void SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string AppID);
        }
    }
}

