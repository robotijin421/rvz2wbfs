using System;
using System.IO;
using System.Text.Json;

namespace WiiConverterDesktop.Services
{
    public class AppSettings
    {
        public string DolphinToolPath { get; set; } = "";
        public string WitPath { get; set; } = "";
        public string OutputDirectory { get; set; } = "";
    }

    public class SettingsService
    {
        private readonly string _settingsFilePath;
        private AppSettings _settings;

        public SettingsService()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string folder = Path.Combine(appData, "WiiConverterDesktop");
            Directory.CreateDirectory(folder);
            _settingsFilePath = Path.Combine(folder, "settings.json");
            Load();
        }

        public AppSettings Settings => _settings;

        public void Load()
        {
            if (File.Exists(_settingsFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_settingsFilePath);
                    _settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
                catch
                {
                    _settings = new AppSettings();
                }
            }
            else
            {
                _settings = new AppSettings();
            }
        }

        public void Save()
        {
            try
            {
                string json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }
    }
}
