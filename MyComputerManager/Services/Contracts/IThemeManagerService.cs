using MyComputerManager.Models;

namespace MyComputerManager.Services.Contracts
{
    /// <summary>
    /// 三态主题（跟随系统 / 浅色 / 深色）管理服务
    /// </summary>
    public interface IThemeManagerService
    {
        /// <summary>当前主题模式</summary>
        AppTheme CurrentMode { get; }

        /// <summary>
        /// 应用指定主题模式并持久化
        /// </summary>
        void ApplyThemeMode(AppTheme mode);

        /// <summary>
        /// 启动时应用上次保存的主题模式（需在窗口 Loaded 后调用）
        /// </summary>
        /// <param name="window">主窗口</param>
        void ApplyStartupTheme(System.Windows.Window window);
    }
}