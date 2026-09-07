using System.Windows;
using MyComputerManager.Models;
using MyComputerManager.Services.Contracts;
using Wpf.Ui.Appearance;

namespace MyComputerManager.Services
{
    /// <summary>
    /// 三态主题（跟随系统 / 浅色 / 深色）管理服务
    /// </summary>
    public class ThemeManagerService : IThemeManagerService
    {
        private readonly ISettingsService _settingsService;
        private AppTheme _current;
        private Window _window;

        public ThemeManagerService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            _current = _settingsService.Load().ThemeMode;
        }

        public AppTheme CurrentMode => _current;

        public void ApplyStartupTheme(Window window)
        {
            _window = window;
            ApplyThemeMode(_current);
        }

        public void ApplyThemeMode(AppTheme mode)
        {
            _current = mode;

            // 跟随系统：监听系统主题变化自动应用
            if (mode == AppTheme.System)
            {
                if (_window != null)
                    SystemThemeWatcher.Watch(_window);
                else
                    ApplicationThemeManager.ApplySystemTheme();
            }
            // 固定主题：取消系统监听，强制应用目标主题
            else
            {
                if (_window != null)
                {
                    // UnWatch 需要窗口已加载，此处尽量保留，忽略未加载情况
                    try
                    {
                        if (_window.IsLoaded)
                            SystemThemeWatcher.UnWatch(_window);
                    }
                    catch
                    {
                        // 窗口未加载时忽略
                    }
                }

                var theme = mode == Models.AppTheme.Dark ? ApplicationTheme.Dark : ApplicationTheme.Light;
                ApplicationThemeManager.Apply(theme);
            }

            var settings = _settingsService.Load();
            settings.ThemeMode = mode;
            _settingsService.Save(settings);
        }
    }
}