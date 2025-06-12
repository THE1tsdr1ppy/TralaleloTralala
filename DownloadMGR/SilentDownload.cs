using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace TralaleroTralala.DownloadMGR
{
    internal class SilentDownload
    {
        public WebView2 _webView;
        public Dictionary<string, InMemoryDownload> _activeDownloads;
        public bool _incognitoMode;
        private int _downloadCounter = 0;

        public event EventHandler<DownloadCompletedEventArgs> DownloadCompleted;
        public event EventHandler<DownloadProgressEventArgs> DownloadProgress;
        public event EventHandler<DownloadStartedEventArgs> DownloadStarted;

        public SilentDownload(WebView2 webView, bool incognitoMode = true)
        {
            _webView = webView ?? throw new ArgumentNullException(nameof(webView));
            _incognitoMode = incognitoMode;
            _activeDownloads = new Dictionary<string, InMemoryDownload>();

            InitializeDownloadHandling();
        }

        private async void InitializeDownloadHandling()
        {
            await _webView.EnsureCoreWebView2Async();
            _webView.CoreWebView2.DownloadStarting += OnDownloadStarting;
        }

        private void OnDownloadStarting(object sender, CoreWebView2DownloadStartingEventArgs e)
        {
            if (_incognitoMode)
            {
                // Cancel the default download behavior
                e.Cancel = true;

                // Start our custom in-memory download
                StartInMemoryDownload(e.DownloadOperation);
            }
        }

        public async void StartInMemoryDownload(CoreWebView2DownloadOperation downloadOp)
        {
            var downloadId = GenerateDownloadId();
            var totalBytes = downloadOp.TotalBytesToReceive.HasValue ? (long)downloadOp.TotalBytesToReceive.Value : -1;

            var inMemoryDownload = new InMemoryDownload
            {
                Id = downloadId,
                FileName = Path.GetFileName(downloadOp.ResultFilePath),
                TotalBytes = totalBytes,
                Data = new MemoryStream(),
                StartTime = DateTime.Now,
                DownloadOperation = downloadOp
            };

            _activeDownloads[downloadId] = inMemoryDownload;

            // Notify download started
            DownloadStarted?.Invoke(this, new DownloadStartedEventArgs
            {
                Id = downloadId,
                FileName = inMemoryDownload.FileName,
                TotalSize = inMemoryDownload.TotalBytes
            });

            // Monitor download progress
            downloadOp.BytesReceivedChanged += (s, args) =>
            {
                if (_activeDownloads.TryGetValue(downloadId, out var download))
                {
                    download.ReceivedBytes = (long)downloadOp.BytesReceived;

                    var progressPercentage = 0;
                    if (downloadOp.TotalBytesToReceive.HasValue && downloadOp.TotalBytesToReceive.Value > 0)
                    {
                        progressPercentage = (int)(((long)downloadOp.BytesReceived * 100) / (long)downloadOp.TotalBytesToReceive.Value);
                    }

                    DownloadProgress?.Invoke(this, new DownloadProgressEventArgs
                    {
                        Id = downloadId,
                        BytesReceived = download.ReceivedBytes,
                        TotalBytes = download.TotalBytes,
                        ProgressPercentage = progressPercentage
                    });
                }
            };

            downloadOp.StateChanged += (s, args) =>
            {
                if (downloadOp.State == CoreWebView2DownloadState.Completed)
                {
                    OnDownloadCompleted(downloadId, downloadOp);
                }
                else if (downloadOp.State == CoreWebView2DownloadState.Interrupted)
                {
                    OnDownloadFailed(downloadId, downloadOp);
                }
            };
        }

        public async void OnDownloadCompleted(string downloadId, CoreWebView2DownloadOperation downloadOp)
        {
            if (_activeDownloads.TryGetValue(downloadId, out var download))
            {
                try
                {
                    // Read the downloaded file into memory
                    if (File.Exists(downloadOp.ResultFilePath))
                    {
                        var fileData = File.ReadAllBytes(downloadOp.ResultFilePath);
                        download.Data = new MemoryStream(fileData);

                        // Delete the temporary file immediately for incognito mode
                        try
                        {
                            File.Delete(downloadOp.ResultFilePath);
                        }
                        catch
                        {
                            // Ignore file deletion errors
                        }
                    }

                    download.IsCompleted = true;
                    download.CompletedTime = DateTime.Now;

                    DownloadCompleted?.Invoke(this, new DownloadCompletedEventArgs
                    {
                        Id = downloadId,
                        FileName = download.FileName,
                        Data = download.Data.ToArray(),
                        Success = true
                    });
                }
                catch (Exception ex)
                {
                    DownloadCompleted?.Invoke(this, new DownloadCompletedEventArgs
                    {
                        Id = downloadId,
                        FileName = download.FileName,
                        Success = false,
                        ErrorMessage = ex.Message
                    });
                }
            }
        }

        public void OnDownloadFailed(string downloadId, CoreWebView2DownloadOperation downloadOp)
        {
            if (_activeDownloads.TryGetValue(downloadId, out var download))
            {
                DownloadCompleted?.Invoke(this, new DownloadCompletedEventArgs
                {
                    Id = downloadId,
                    FileName = download.FileName,
                    Success = false,
                    ErrorMessage = "Download was interrupted"
                });

                download.Data?.Dispose();
                _activeDownloads.Remove(downloadId);
            }
        }

        public string GenerateDownloadId()
        {
            return $"download_{++_downloadCounter}_{DateTime.Now.Ticks}";
        }

        public byte[] GetDownloadData(string downloadId)
        {
            if (_activeDownloads.TryGetValue(downloadId, out var download) && download.IsCompleted)
            {
                return download.Data.ToArray();
            }
            return null;
        }

        public void SaveDownloadToFile(string downloadId, string filePath)
        {
            var data = GetDownloadData(downloadId);
            if (data != null)
            {
                File.WriteAllBytes(filePath, data);
            }
        }

        public void ClearDownload(string downloadId)
        {
            if (_activeDownloads.TryGetValue(downloadId, out var download))
            {
                download.Data?.Dispose();
                _activeDownloads.Remove(downloadId);
            }
        }

        public void ClearAllDownloads()
        {
            foreach (var download in _activeDownloads.Values)
            {
                download.Data?.Dispose();
            }
            _activeDownloads.Clear();
        }

        public IEnumerable<DownloadInfo> GetActiveDownloads()
        {
            return _activeDownloads.Values.Select(d => new DownloadInfo
            {
                Id = d.Id,
                FileName = d.FileName,
                TotalBytes = d.TotalBytes,
                ReceivedBytes = d.ReceivedBytes,
                IsCompleted = d.IsCompleted,
                StartTime = d.StartTime,
                CompletedTime = d.CompletedTime
            });
        }

        public void PauseDownload(string downloadId)
        {
            if (_activeDownloads.TryGetValue(downloadId, out var download))
            {
                download.DownloadOperation?.Pause();
            }
        }

        public void ResumeDownload(string downloadId)
        {
            if (_activeDownloads.TryGetValue(downloadId, out var download))
            {
                download.DownloadOperation?.Resume();
            }
        }

        public void CancelDownload(string downloadId)
        {
            if (_activeDownloads.TryGetValue(downloadId, out var download))
            {
                download.DownloadOperation?.Cancel();
                download.Data?.Dispose();
                _activeDownloads.Remove(downloadId);
            }
        }

        public void Dispose()
        {
            ClearAllDownloads();
        }
    }

    internal class InMemoryDownload
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public long TotalBytes { get; set; }
        public long ReceivedBytes { get; set; }
        public MemoryStream Data { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? CompletedTime { get; set; }
        public CoreWebView2DownloadOperation DownloadOperation { get; set; }
    }

    public class DownloadInfo
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public long TotalBytes { get; set; }
        public long ReceivedBytes { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? CompletedTime { get; set; }
    }

    public class DownloadStartedEventArgs : EventArgs
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public long TotalSize { get; set; }
    }

    public class DownloadProgressEventArgs : EventArgs
    {
        public string Id { get; set; }
        public long BytesReceived { get; set; }
        public long TotalBytes { get; set; }
        public int ProgressPercentage { get; set; }
    }

    public class DownloadCompletedEventArgs : EventArgs
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public byte[] Data { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
}