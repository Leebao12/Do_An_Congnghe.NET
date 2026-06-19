using System;
using System.IO;
using System.Text;
using System.Text.Json;
using CoffeeTea.Models;

namespace CoffeeTea.Services
{
    public class SettingsStorageService
    {
        private const string SettingsFolderName = "Data";
        private const string SettingsFileName = "software-settings.json";
        private const string BackupFolderName = "Backup";

        private readonly JsonSerializerOptions _jsonOptions;

        public SettingsStorageService()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
        }

        public SoftwareSettingsModel Load()
        {
            try
            {
                string settingsFilePath = GetSettingsFilePath();
                if (!File.Exists(settingsFilePath))
                {
                    return SoftwareSettingsModel.CreateDefault();
                }

                string json = File.ReadAllText(settingsFilePath, Encoding.UTF8);
                SoftwareSettingsModel settings = JsonSerializer.Deserialize<SoftwareSettingsModel>(json, _jsonOptions);
                return settings ?? SoftwareSettingsModel.CreateDefault();
            }
            catch (Exception)
            {
                return SoftwareSettingsModel.CreateDefault();
            }
        }

        public void Save(SoftwareSettingsModel settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            string settingsFilePath = GetSettingsFilePath();
            EnsureDirectoryExists(Path.GetDirectoryName(settingsFilePath));

            string json = JsonSerializer.Serialize(settings, _jsonOptions);
            File.WriteAllText(settingsFilePath, json, Encoding.UTF8);
        }

        public string CreateBackup(SoftwareSettingsModel settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            string backupFolderPath = GetBackupFolderPath();
            EnsureDirectoryExists(backupFolderPath);

            string fileName = string.Format("software-settings-{0:yyyyMMdd-HHmmss}.json", DateTime.Now);
            string backupFilePath = Path.Combine(backupFolderPath, fileName);

            string json = JsonSerializer.Serialize(settings, _jsonOptions);
            File.WriteAllText(backupFilePath, json, Encoding.UTF8);

            return backupFilePath;
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private static string GetRootFolderPath()
        {
            string projectFolder = GetProjectFolderPath();
            return Path.Combine(projectFolder, SettingsFolderName);
        }

        private static string GetProjectFolderPath()
        {
            string runTimeCandidate = Path.GetFullPath(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\"));

            if (Directory.Exists(runTimeCandidate) &&
                string.Equals(new DirectoryInfo(runTimeCandidate).Name, "CoffeeTea", StringComparison.OrdinalIgnoreCase))
            {
                return runTimeCandidate;
            }

            string workingDirectoryCandidate = Path.Combine(Directory.GetCurrentDirectory(), "CoffeeTea");
            if (Directory.Exists(workingDirectoryCandidate))
            {
                return workingDirectoryCandidate;
            }

            return runTimeCandidate;
        }

        private static string GetSettingsFilePath()
        {
            return Path.Combine(GetRootFolderPath(), SettingsFileName);
        }

        private static string GetBackupFolderPath()
        {
            return Path.Combine(GetRootFolderPath(), BackupFolderName);
        }
    }
}
