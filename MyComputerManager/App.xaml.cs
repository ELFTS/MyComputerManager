using System;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyComputerManager.Models;
using MyComputerManager.Services;
using MyComputerManager.Services.Contracts;
using MyComputerManager.ViewModels;
using MyComputerManager.Views;
using Wpf.Ui;

namespace MyComputerManager
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private IHost _host;

        private async void OnStartup(object sender, StartupEventArgs e)
        {
            AppDomain.CurrentDomain.FirstChanceException += FirstChanceHandler;

            _host = Host.CreateDefaultBuilder(e.Args)
            .ConfigureAppConfiguration(c =>
            {
                c.SetBasePath(AppContext.BaseDirectory);
            })
            .ConfigureServices(ConfigureServices)
            .Build();

            await _host.StartAsync();
        }

        private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            // App Host
            services.AddHostedService<ApplicationHostService>();

            // 导航解析服务（Frame 导航）
            services.AddSingleton<Services.Contracts.INavigationService, Services.NavigationService>();

            // 主题
            services.AddSingleton<IThemeService, Wpf.Ui.ThemeService>();

            // 配置持久化 + 三态主题管理
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IThemeManagerService, ThemeManagerService>();

            // Snackbar / Dialog
            services.AddSingleton<ISnackbarService, SnackbarService>();
            services.AddSingleton<IContentDialogService, ContentDialogService>();
            services.AddSingleton<ISnackBarService, SnackBarService>();
            services.AddSingleton<IDialogService, DialogService>();

            // 数据服务
            services.AddSingleton<IDataService, DataService>();

            // 主窗口
            services.AddSingleton<MainWindow>();

            // Views and ViewModels
            services.AddSingleton<MainPage>();
            services.AddSingleton<MainPageViewModel>();

            services.AddTransient<DetailPage>();
            services.AddTransient<DetailPageViewModel>();

            services.AddSingleton<AboutPage>();
        }

        private async void OnExit(object sender, ExitEventArgs e)
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
                _host = null;
            }
        }

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try { System.IO.File.AppendAllText("err.log", DateTime.Now + " DISP: " + e.Exception + "\n"); } catch { }
        }

        public static void FirstChanceHandler(object source, FirstChanceExceptionEventArgs e)
        {
            Console.WriteLine("FirstChanceException event raised in {0}: {1}",
                AppDomain.CurrentDomain.FriendlyName, e.Exception.Message);
        }
    }
}