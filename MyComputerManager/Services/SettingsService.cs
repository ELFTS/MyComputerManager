using System;
using System.IO;
using System.Text.Json;
using MyComputerManager.Models;
using MyComputerManager.Services.Contracts;

namespace MyComputerManager.Services
{
    /// <summary>
    /// 基于本地 JSON 文件的配置持久化服务
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _jsonOptions;

        public SettingsService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dir = Path.Combine(appData, "MyComputerManager");
            Directory.CreateDirectory(dir);
            _filePath = Path.Combine(dir, "settings.json");
            _jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        }

        public AppSettings Load()
        {
            try
            {
                if (!File.Exists(_filePath))
                    return new AppSettings();

                var json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions) ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        public void Save(AppSettings settings)
        {
            try
            {
                var json = JsonSerializer.Serialize(settings, _jsonOptions);
                File.WriteAllText(_filePath, json);
            }
            catch
            {
                // 忽略写入失败
            }
        }
    }
}