namespace MyComputerManager.Models
{
    /// <summary>
    /// 应用本地配置
    /// </summary>
    public class AppSettings
    {
        /// <summary>主题模式</summary>
        public AppTheme ThemeMode { get; set; } = AppTheme.System;
    }
}