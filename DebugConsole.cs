using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Management;
using System.Threading;

namespace TralaleroTralala
{
    public partial class DebugConsole : Form
    {
        private static DebugConsole _instance;
        private TabControl _mainTabControl;
        private RichTextBox _consoleOutput;
        private ListView _networkListView;
        private Panel _performanceChartsPanel;
        private ListView _performanceListView;
        private TextBox _filterTextBox;
        private ComboBox _logLevelComboBox;
        private ToolStripStatusLabel _statusLabel;

        // Performance monitoring
        private PerformanceCounter _cpuCounter;
        private PerformanceCounter _memoryCounter;
        private System.Windows.Forms.Timer _performanceTimer;
        private Queue<double> _cpuHistory = new Queue<double>();
        private Queue<double> _memoryHistory = new Queue<double>();
        private const int MAX_HISTORY_POINTS = 60; // 1 minute of data at 1-second intervals

        // Performance visualization controls
        private Panel _cpuGraphPanel;
        private Panel _memoryGraphPanel;
        private Label _cpuLabel;
        private Label _memoryLabel;
        private ProgressBar _cpuProgressBar;
        private ProgressBar _memoryProgressBar;

        // Log filtering
        private List<LogEntry> _allLogEntries = new List<LogEntry>();
        private LogLevel _currentLogLevel = LogLevel.All;
        private string _currentFilter = "";

        public DebugConsole()
        {
            InitializeComponent();
            SetupConsole();
            InitializePerformanceCounters();
            this.FormClosing += (s, e) => { e.Cancel = true; this.Hide(); };
        }

        private void SetupConsole()
        {
            this.Text = "Debug Console - Tralalero Tralala";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 500);

            // Create main tab control
            _mainTabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F)
            };

            // Setup tabs
            SetupConsoleTab();
            SetupNetworkTab();
            SetupPerformanceTab();
            SetupElementsTab();

            this.Controls.Add(_mainTabControl);

            // Setup status bar
            SetupStatusBar();

            // Apply dark theme
            ApplyDarkTheme();
        }

        private void SetupConsoleTab()
        {
            var consoleTab = new TabPage("Console");
            var consolePanel = new Panel { Dock = DockStyle.Fill };

            // Top toolbar for console
            var toolbarPanel = new Panel
            {
                Height = 35,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(30, 30, 30)
            };

            // Clear button
            var clearButton = new Button
            {
                Text = "Clear",
                Location = new Point(5, 5),
                Size = new Size(60, 25),
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            clearButton.Click += (s, e) => Clear();
            clearButton.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 60);

            // Filter textbox
            _filterTextBox = new TextBox
            {
                Location = new Point(75, 7),
                Size = new Size(200, 21),
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            _filterTextBox.TextChanged += FilterTextBox_TextChanged;

            // Add placeholder functionality for filter textbox
            AddPlaceholderText(_filterTextBox, "Filter console output...");

            // Log level combo box
            _logLevelComboBox = new ComboBox
            {
                Location = new Point(285, 6),
                Size = new Size(100, 23),
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _logLevelComboBox.Items.AddRange(Enum.GetNames(typeof(LogLevel)));
            _logLevelComboBox.SelectedItem = "All";
            _logLevelComboBox.SelectedIndexChanged += LogLevelComboBox_SelectedIndexChanged;

            toolbarPanel.Controls.AddRange(new Control[] { clearButton, _filterTextBox, _logLevelComboBox });

            // Console output
            _consoleOutput = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.FromArgb(20, 20, 20),
                ForeColor = Color.White,
                Font = new Font("Consolas", 10),
                ScrollBars = RichTextBoxScrollBars.Both,
                BorderStyle = BorderStyle.None
            };

            consolePanel.Controls.Add(_consoleOutput);
            consolePanel.Controls.Add(toolbarPanel);
            consoleTab.Controls.Add(consolePanel);
            _mainTabControl.TabPages.Add(consoleTab);
        }

        private void AddPlaceholderText(TextBox textBox, string placeholder)
        {
            textBox.Text = placeholder;
            textBox.ForeColor = Color.Gray;

            textBox.Enter += (sender, e) =>
            {
                if (textBox.Text == placeholder)
                {
                    textBox.Text = "";
                    textBox.ForeColor = Color.White;
                }
            };

            textBox.Leave += (sender, e) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = placeholder;
                    textBox.ForeColor = Color.Gray;
                }
            };
        }

        private void SetupNetworkTab()
        {
            var networkTab = new TabPage("Network");

            _networkListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White
            };

            _networkListView.Columns.Add("Name", 300);
            _networkListView.Columns.Add("Status", 80);
            _networkListView.Columns.Add("Type", 100);
            _networkListView.Columns.Add("Size", 80);
            _networkListView.Columns.Add("Time", 80);
            _networkListView.Columns.Add("Waterfall", 150);

            networkTab.Controls.Add(_networkListView);
            _mainTabControl.TabPages.Add(networkTab);
        }

        private void SetupPerformanceTab()
        {
            var performanceTab = new TabPage("Performance");
            var performancePanel = new Panel { Dock = DockStyle.Fill };

            // Performance charts container (using simple panels instead of charts)
            _performanceChartsPanel = new Panel
            {
                Height = 300,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(30, 30, 30)
            };

            // CPU Performance Panel
            _cpuGraphPanel = new Panel
            {
                Size = new Size(480, 290),
                Location = new Point(5, 5),
                BackColor = Color.FromArgb(40, 40, 40),
                BorderStyle = BorderStyle.FixedSingle
            };

            _cpuLabel = new Label
            {
                Text = "CPU Usage: 0%",
                Location = new Point(10, 10),
                Size = new Size(200, 20),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            _cpuProgressBar = new ProgressBar
            {
                Location = new Point(10, 35),
                Size = new Size(450, 20),
                Minimum = 0,
                Maximum = 100,
                Style = ProgressBarStyle.Continuous
            };

            var cpuHistoryPanel = new Panel
            {
                Location = new Point(10, 65),
                Size = new Size(450, 200),
                BackColor = Color.FromArgb(20, 20, 20),
                BorderStyle = BorderStyle.FixedSingle
            };
            cpuHistoryPanel.Paint += (s, e) => DrawCpuHistory(e.Graphics, cpuHistoryPanel.Size);

            _cpuGraphPanel.Controls.AddRange(new Control[] { _cpuLabel, _cpuProgressBar, cpuHistoryPanel });

            // Memory Performance Panel
            _memoryGraphPanel = new Panel
            {
                Size = new Size(480, 290),
                Location = new Point(490, 5),
                BackColor = Color.FromArgb(40, 40, 40),
                BorderStyle = BorderStyle.FixedSingle
            };

            _memoryLabel = new Label
            {
                Text = "Memory Usage: 0 MB",
                Location = new Point(10, 10),
                Size = new Size(200, 20),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            _memoryProgressBar = new ProgressBar
            {
                Location = new Point(10, 35),
                Size = new Size(450, 20),
                Minimum = 0,
                Maximum = 1000, // Will be adjusted dynamically
                Style = ProgressBarStyle.Continuous
            };

            var memoryHistoryPanel = new Panel
            {
                Location = new Point(10, 65),
                Size = new Size(450, 200),
                BackColor = Color.FromArgb(20, 20, 20),
                BorderStyle = BorderStyle.FixedSingle
            };
            memoryHistoryPanel.Paint += (s, e) => DrawMemoryHistory(e.Graphics, memoryHistoryPanel.Size);

            _memoryGraphPanel.Controls.AddRange(new Control[] { _memoryLabel, _memoryProgressBar, memoryHistoryPanel });

            _performanceChartsPanel.Controls.AddRange(new Control[] { _cpuGraphPanel, _memoryGraphPanel });

            // Performance details list
            _performanceListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White
            };

            _performanceListView.Columns.Add("Metric", 200);
            _performanceListView.Columns.Add("Current", 100);
            _performanceListView.Columns.Add("Average", 100);
            _performanceListView.Columns.Add("Peak", 100);
            _performanceListView.Columns.Add("Description", 300);

            performancePanel.Controls.Add(_performanceListView);
            performancePanel.Controls.Add(_performanceChartsPanel);
            performanceTab.Controls.Add(performancePanel);
            _mainTabControl.TabPages.Add(performanceTab);
        }

        private void DrawCpuHistory(Graphics g, Size panelSize)
        {
            if (_cpuHistory.Count < 2) return;

            var pen = new Pen(Color.FromArgb(0, 150, 255), 2);
            var points = new List<PointF>();

            float xStep = (float)panelSize.Width / (MAX_HISTORY_POINTS - 1);
            float yScale = (float)(panelSize.Height - 20) / 100f; // 100% max

            int index = 0;
            foreach (var value in _cpuHistory)
            {
                float x = index * xStep;
                float y = panelSize.Height - 10 - (float)(value * yScale);
                points.Add(new PointF(x, y));
                index++;
            }

            if (points.Count > 1)
            {
                g.DrawLines(pen, points.ToArray());
            }

            pen.Dispose();
        }

        private void DrawMemoryHistory(Graphics g, Size panelSize)
        {
            if (_memoryHistory.Count < 2) return;

            var pen = new Pen(Color.FromArgb(255, 150, 0), 2);
            var brush = new SolidBrush(Color.FromArgb(50, 255, 150, 0));
            var points = new List<PointF>();

            float xStep = (float)panelSize.Width / (MAX_HISTORY_POINTS - 1);
            double maxMemory = _memoryHistory.Count > 0 ? _memoryHistory.Max() * 1.2 : 1000;
            float yScale = (float)(panelSize.Height - 20) / (float)maxMemory;

            int index = 0;
            foreach (var value in _memoryHistory)
            {
                float x = index * xStep;
                float y = panelSize.Height - 10 - (float)(value * yScale);
                points.Add(new PointF(x, y));
                index++;
            }

            // Draw filled area
            if (points.Count > 1)
            {
                var areaPoints = new List<PointF>(points);
                areaPoints.Add(new PointF(points.Last().X, panelSize.Height - 10));
                areaPoints.Add(new PointF(points.First().X, panelSize.Height - 10));
                g.FillPolygon(brush, areaPoints.ToArray());
                g.DrawLines(pen, points.ToArray());
            }

            pen.Dispose();
            brush.Dispose();
        }

        private void SetupElementsTab()
        {
            var elementsTab = new TabPage("Elements");

            var elementsTreeView = new TreeView
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                LineColor = Color.Gray
            };

            // Add some sample nodes
            var rootNode = new TreeNode("html") { ForeColor = Color.FromArgb(100, 150, 255) };
            var headNode = new TreeNode("head") { ForeColor = Color.FromArgb(100, 150, 255) };
            var bodyNode = new TreeNode("body") { ForeColor = Color.FromArgb(100, 150, 255) };

            headNode.Nodes.Add(new TreeNode("title") { ForeColor = Color.FromArgb(100, 150, 255) });
            bodyNode.Nodes.Add(new TreeNode("div.container") { ForeColor = Color.FromArgb(100, 150, 255) });

            rootNode.Nodes.AddRange(new TreeNode[] { headNode, bodyNode });
            elementsTreeView.Nodes.Add(rootNode);
            rootNode.Expand();

            elementsTab.Controls.Add(elementsTreeView);
            _mainTabControl.TabPages.Add(elementsTab);
        }

        private void SetupStatusBar()
        {
            var statusStrip = new StatusStrip
            {
                BackColor = Color.FromArgb(25, 25, 25)
            };

            _statusLabel = new ToolStripStatusLabel
            {
                ForeColor = Color.White,
                Text = "Ready"
            };

            statusStrip.Items.Add(_statusLabel);
            this.Controls.Add(statusStrip);
        }

        private void InitializePerformanceCounters()
        {
            try
            {
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _memoryCounter = new PerformanceCounter("Memory", "Available MBytes");

                _performanceTimer = new System.Windows.Forms.Timer
                {
                    Interval = 1000,
                    Enabled = true
                };
                _performanceTimer.Tick += UpdatePerformanceData;

                // Initialize performance list
                InitializePerformanceList();
            }
            catch (Exception ex)
            {
                Log($"Failed to initialize performance counters: {ex.Message}", LogLevel.Error);
            }
        }

        private void InitializePerformanceList()
        {
            if (_performanceListView == null) return;

            var items = new[]
            {
                new ListViewItem(new[] { "CPU Usage", "0%", "0%", "0%", "Processor time percentage" }),
                new ListViewItem(new[] { "Available Memory", "0 MB", "0 MB", "0 MB", "Available system memory" }),
                new ListViewItem(new[] { "Process Memory", "0 MB", "0 MB", "0 MB", "Current process memory usage" }),
                new ListViewItem(new[] { "GC Collections", "0", "0", "0", "Garbage collection count" }),
                new ListViewItem(new[] { "Thread Count", "0", "0", "0", "Active thread count" }),
                new ListViewItem(new[] { "Handle Count", "0", "0", "0", "System handle count" })
            };

            _performanceListView.Items.AddRange(items);
        }

        private void UpdatePerformanceData(object sender, EventArgs e)
        {
            try
            {
                if (_cpuCounter == null || _memoryCounter == null) return;

                // Get current values
                double cpuUsage = _cpuCounter.NextValue();
                double availableMemory = _memoryCounter.NextValue();
                double processMemory = Process.GetCurrentProcess().WorkingSet64 / (1024 * 1024); // MB

                // Update history
                _cpuHistory.Enqueue(cpuUsage);
                _memoryHistory.Enqueue(processMemory);

                if (_cpuHistory.Count > MAX_HISTORY_POINTS)
                    _cpuHistory.Dequeue();
                if (_memoryHistory.Count > MAX_HISTORY_POINTS)
                    _memoryHistory.Dequeue();

                // Update UI controls
                UpdatePerformanceUI(cpuUsage, availableMemory, processMemory);

                // Update performance list
                UpdatePerformanceList(cpuUsage, availableMemory, processMemory);

                // Update status
                _statusLabel.Text = $"CPU: {cpuUsage:F1}% | Memory: {processMemory:F0} MB";
            }
            catch (Exception ex)
            {
                Log($"Performance update error: {ex.Message}", LogLevel.Error);
            }
        }

        private void UpdatePerformanceUI(double cpuUsage, double availableMemory, double processMemory)
        {
            if (_cpuLabel != null)
            {
                _cpuLabel.Text = $"CPU Usage: {cpuUsage:F1}%";
                _cpuProgressBar.Value = Math.Min(100, Math.Max(0, (int)cpuUsage));
            }

            if (_memoryLabel != null)
            {
                _memoryLabel.Text = $"Memory Usage: {processMemory:F0} MB";
                double maxMemory = _memoryHistory.Count > 0 ? _memoryHistory.Max() * 1.2 : 1000;
                _memoryProgressBar.Maximum = (int)maxMemory;
                _memoryProgressBar.Value = Math.Min(_memoryProgressBar.Maximum, Math.Max(0, (int)processMemory));
            }

            // Invalidate panels to trigger redraw
            _cpuGraphPanel?.Invalidate();
            _memoryGraphPanel?.Invalidate();
        }

        private void UpdatePerformanceList(double cpuUsage, double availableMemory, double processMemory)
        {
            if (_performanceListView?.Items == null) return;

            var process = Process.GetCurrentProcess();
            var gcGen0 = GC.CollectionCount(0);
            var gcGen1 = GC.CollectionCount(1);
            var gcGen2 = GC.CollectionCount(2);
            var totalGC = gcGen0 + gcGen1 + gcGen2;

            var updates = new[]
            {
                new { Index = 0, Current = $"{cpuUsage:F1}%", Avg = $"{(_cpuHistory.Count > 0 ? _cpuHistory.Average() : 0):F1}%", Peak = $"{(_cpuHistory.Count > 0 ? _cpuHistory.Max() : 0):F1}%" },
                new { Index = 1, Current = $"{availableMemory:F0} MB", Avg = "N/A", Peak = "N/A" },
                new { Index = 2, Current = $"{processMemory:F0} MB", Avg = $"{(_memoryHistory.Count > 0 ? _memoryHistory.Average() : 0):F0} MB", Peak = $"{(_memoryHistory.Count > 0 ? _memoryHistory.Max() : 0):F0} MB" },
                new { Index = 3, Current = totalGC.ToString(), Avg = "N/A", Peak = "N/A" },
                new { Index = 4, Current = process.Threads.Count.ToString(), Avg = "N/A", Peak = "N/A" },
                new { Index = 5, Current = process.HandleCount.ToString(), Avg = "N/A", Peak = "N/A" }
            };

            foreach (var update in updates)
            {
                if (update.Index < _performanceListView.Items.Count)
                {
                    var item = _performanceListView.Items[update.Index];
                    item.SubItems[1].Text = update.Current;
                    item.SubItems[2].Text = update.Avg;
                    item.SubItems[3].Text = update.Peak;
                }
            }
        }

        private void FilterTextBox_TextChanged(object sender, EventArgs e)
        {
            // Only process if not showing placeholder text
            if (_filterTextBox.ForeColor != Color.Gray)
            {
                _currentFilter = _filterTextBox.Text;
                FilterAndDisplayLogs();
            }
        }

        private void LogLevelComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Enum.TryParse<LogLevel>(_logLevelComboBox.SelectedItem.ToString(), out LogLevel level))
            {
                _currentLogLevel = level;
                FilterAndDisplayLogs();
            }
        }

        private void FilterAndDisplayLogs()
        {
            if (_consoleOutput == null) return;

            _consoleOutput.Clear();

            var filteredLogs = _allLogEntries.Where(log =>
                (_currentLogLevel == LogLevel.All || log.Level == _currentLogLevel) &&
                (string.IsNullOrEmpty(_currentFilter) || log.Message.IndexOf(_currentFilter, StringComparison.OrdinalIgnoreCase) >= 0)
            );

            foreach (var log in filteredLogs)
            {
                AppendLogEntry(log);
            }
        }

        private void ApplyDarkTheme()
        {
            this.BackColor = Color.FromArgb(25, 25, 25);
            _mainTabControl.BackColor = Color.FromArgb(30, 30, 30);

            foreach (TabPage tab in _mainTabControl.TabPages)
            {
                tab.BackColor = Color.FromArgb(30, 30, 30);
                tab.ForeColor = Color.White;
            }
        }

        public static DebugConsole Instance
        {
            get
            {
                if (_instance == null || _instance.IsDisposed)
                {
                    _instance = new DebugConsole();
                }
                return _instance;
            }
        }

        public void Log(string message, LogLevel level = LogLevel.Info)
        {
            var logEntry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Message = message,
                Level = level
            };

            _allLogEntries.Add(logEntry);

            // Keep only last 1000 entries
            if (_allLogEntries.Count > 1000)
            {
                _allLogEntries.RemoveAt(0);
            }

            if (_consoleOutput?.InvokeRequired == true)
            {
                _consoleOutput.Invoke(new Action(() => Log(message, level)));
                return;
            }

            // Check if should display based on current filters
            if ((_currentLogLevel == LogLevel.All || level == _currentLogLevel) &&
                (string.IsNullOrEmpty(_currentFilter) || message.IndexOf(_currentFilter, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                AppendLogEntry(logEntry);
            }
        }

        private void AppendLogEntry(LogEntry logEntry)
        {
            if (_consoleOutput == null) return;

            var color = GetLogLevelColor(logEntry.Level);
            var prefix = GetLogLevelPrefix(logEntry.Level);

            _consoleOutput.SelectionStart = _consoleOutput.TextLength;
            _consoleOutput.SelectionLength = 0;

            // Timestamp
            _consoleOutput.SelectionColor = Color.Gray;
            _consoleOutput.AppendText($"[{logEntry.Timestamp:HH:mm:ss.fff}] ");

            // Log level prefix
            _consoleOutput.SelectionColor = color;
            _consoleOutput.AppendText($"{prefix} ");

            // Message
            _consoleOutput.SelectionColor = Color.White;
            _consoleOutput.AppendText($"{logEntry.Message}\n");

            _consoleOutput.ScrollToCaret();
        }

        private Color GetLogLevelColor(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Error:
                    return Color.FromArgb(255, 100, 100);
                case LogLevel.Warning:
                    return Color.FromArgb(255, 200, 100);
                case LogLevel.Info:
                    return Color.FromArgb(100, 150, 255);
                case LogLevel.Debug:
                    return Color.Gray;
                default:
                    return Color.White;
            }
        }

        private string GetLogLevelPrefix(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Error:
                    return "❌";
                case LogLevel.Warning:
                    return "⚠️";
                case LogLevel.Info:
                    return "ℹ️";
                case LogLevel.Debug:
                    return "🐛";
                default:
                    return "📝";
            }
        }

        public void Clear()
        {
            if (_consoleOutput?.InvokeRequired == true)
            {
                _consoleOutput.Invoke(new Action(Clear));
                return;
            }

            _consoleOutput?.Clear();
            _allLogEntries.Clear();
        }

        public void AddNetworkEntry(string name, string status, string type, string size, string time)
        {
            if (_networkListView?.InvokeRequired == true)
            {
                _networkListView.Invoke(new Action(() => AddNetworkEntry(name, status, type, size, time)));
                return;
            }

            var item = new ListViewItem(new[] { name, status, type, size, time, "████████░░" });
            Color statusColor;
            if (status == "200")
                statusColor = Color.LightGreen;
            else if (status.StartsWith("4"))
                statusColor = Color.Orange;
            else if (status.StartsWith("5"))
                statusColor = Color.Red;
            else
                statusColor = Color.White;

            item.ForeColor = statusColor;

            _networkListView?.Items.Add(item);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _performanceTimer?.Stop();
            _cpuCounter?.Dispose();
            _memoryCounter?.Dispose();
            base.OnFormClosing(e);
        }
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
        public LogLevel Level { get; set; }
    }

    public enum LogLevel
    {
        All,
        Debug,
        Info,
        Warning,
        Error
    }
}