using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TralaleroTralala
{
    public class TabPersistenceService
    {
        private readonly string _dataDirectory;
        private readonly string _tabsFilePath;

        public TabPersistenceService()
        {
            _dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "browsingData");
            _tabsFilePath = Path.Combine(_dataDirectory, "saved_tabs.txt");

            // Ensure directory exists
            EnsureDirectoryExists();
        }

        private void EnsureDirectoryExists()
        {
            if (!Directory.Exists(_dataDirectory))
            {
                Directory.CreateDirectory(_dataDirectory);
            }
        }

        public async Task SaveTabsAsync(List<string> urls)
        {
            if (urls == null || urls.Count == 0)
                return;



            EnsureDirectoryExists();

            try
            {
                // Write all URLs to file
                await Task.Run(() => File.WriteAllLines(_tabsFilePath, urls));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving tabs: {ex.Message}");
                throw;
            }
        }

        public async Task<List<string>> LoadTabsAsync()
        {
            var urls = new List<string>();

            try
            {
                if (File.Exists(_tabsFilePath))
                {
                    string[] lines = await Task.Run(() => File.ReadAllLines(_tabsFilePath));
                    urls.AddRange(lines.Where(line => !string.IsNullOrWhiteSpace(line)));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading tabs: {ex.Message}");
                // Return empty list on error
            }

            return urls;
        }

        public void ClearSavedTabs()
        {
            try
            {
                if (File.Exists(_tabsFilePath))
                {
                    File.Delete(_tabsFilePath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing saved tabs: {ex.Message}");
            }
        }
    }
}