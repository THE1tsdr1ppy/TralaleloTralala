using System;
using System.Drawing;
using System.Windows.Forms;

namespace TralaleroTralala
{
    /// <summary>
    /// A prompt dialog that asks users how to handle redirections
    /// </summary>
    public class RedirectionPrompt : Form
    {
        private readonly string _sourceUrl;
        private readonly string _targetUrl;
        public RedirectionResult Result { get; private set; } = RedirectionResult.Cancel;

        public RedirectionPrompt(string sourceUrl, string targetUrl)
        {
            _sourceUrl = sourceUrl;
            _targetUrl = targetUrl;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Form settings
            this.Text = "Redirection Detected";
            this.Size = new Size(500, 280);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.BackColor = Color.FromArgb(27, 27, 27);
            this.ForeColor = Color.White;

            // Icon (warning icon)
            PictureBox iconBox = new PictureBox
            {
                Size = new Size(48, 48),
                Location = new Point(20, 20),
                Image = SystemIcons.Warning.ToBitmap(),
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            // Title label
            Label titleLabel = new Label
            {
                Text = "Redirection Detected",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(80, 20)
            };

            // Message label
            Label messageLabel = new Label
            {
                Text = $"You are being redirected from:\n{TruncateUrl(_sourceUrl, 60)}\n\nTo:\n{TruncateUrl(_targetUrl, 60)}",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                Size = new Size(380, 100),
                Location = new Point(80, 50)
            };

            // Current tab button
            Button currentTabButton = CreateButton("Open in Current Tab", new Point(80, 160));
            currentTabButton.Click += (s, e) =>
            {
                Result = RedirectionResult.CurrentTab;
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            // New tab button
            Button newTabButton = CreateButton("Open in New Tab", new Point(230, 160));
            newTabButton.Click += (s, e) =>
            {
                Result = RedirectionResult.NewTab;
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            // Cancel button
            Button cancelButton = CreateButton("Cancel", new Point(380, 160));
            cancelButton.Click += (s, e) =>
            {
                Result = RedirectionResult.Cancel;
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            // Add controls to form
            this.Controls.Add(iconBox);
            this.Controls.Add(titleLabel);
            this.Controls.Add(messageLabel);
            this.Controls.Add(currentTabButton);
            this.Controls.Add(newTabButton);
            this.Controls.Add(cancelButton);

            // Set proper tab order
            currentTabButton.TabIndex = 0;
            newTabButton.TabIndex = 1;
            cancelButton.TabIndex = 2;
        }

        private Button CreateButton(string text, Point location)
        {
            Button button = new Button
            {
                Text = text,
                Size = new Size(140, 35),
                Location = location,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9)
            };

            button.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 60);
            button.Cursor = Cursors.Hand;

            // Add hover effects
            button.MouseEnter += (s, e) => button.BackColor = Color.FromArgb(60, 60, 60);
            button.MouseLeave += (s, e) => button.BackColor = Color.FromArgb(40, 40, 40);

            return button;
        }

        private string TruncateUrl(string url, int maxLength)
        {
            if (url.Length <= maxLength)
                return url;

            return url.Substring(0, maxLength - 3) + "...";
        }
    }

    /// <summary>
    /// Possible results from the redirection prompt
    /// </summary>
    public enum RedirectionResult
    {
        CurrentTab,
        NewTab,
        Cancel
    }
}