using MyComputerManager.Helpers;
using MyComputerManager.Models;
using MyComputerManager.Services.Contracts;
using MyComputerManager.ViewModels;
using MyComputerManager.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls;
using IContentDialogService = Wpf.Ui.IContentDialogService;

namespace MyComputerManager
{
    public partial class MainWindow
    {
        private readonly IDataService _dataService;
        private readonly INavigationService _navigationService;
        private readonly ISnackBarService _snackBarService;
        private readonly IDialogService _dialogService;
        private readonly IThemeManagerService _themeManagerService;
        private readonly IContentDialogService _contentDialogService;

        public MainWindow(INavigationService navigationService, IDataService dataService,
            ISnackBarService snackBarService, IDialogService dialogService, IThemeManagerService themeManagerService,
            IContentDialogService contentDialogService)
        {
            InitializeComponent();
            _dataService = dataService;
            _navigationService = navigationService;
            _snackBarService = snackBarService;
            _dialogService = dialogService;
            _themeManagerService = themeManagerService;
            _contentDialogService = contentDialogService;

            _navigationService.SetFrame(RootFrame);
            _snackBarService.SetSnackbarPresenter(RootSnackbar);
            _dialogService.SetDialogHost(RootDialog);

            WelcomeGrid.Visibility = Visibility.Visible;
        }

        public void ShowWindow()
        {
            Show();
            Activate();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 启动时应用上次保存的主题模式
            _themeManagerService.ApplyStartupTheme(this);
            UpdateThemeMenuCheckState();

            Task.Run(async () =>
            {
                var data = NamespaceHelper.GetItems();
                await Task.Delay(1000);

                await Dispatcher.InvokeAsync(() =>
                {
                    WelcomeGrid.Visibility = Visibility.Collapsed;
                    RootMainGrid.Visibility = Visibility.Visible;

                    var o = new ObservableCollection<NamespaceItem>();
                    if (data != null)
                        foreach (var item in data)
                            o.Add(item);
                    _dataService.SetData(o);
                    _navigationService.Navigate(typeof(MainPage));
                });
            });
        }

        private void MenuBack_Click(object sender, RoutedEventArgs e)
        {
            _navigationService.Navigate(typeof(MainPage));
        }

        private void MenuAdd_Click(object sender, RoutedEventArgs e)
        {
            var item = new NamespaceItem("新建项目");
            _dataService.SetData(item);
            _navigationService.Navigate(typeof(DetailPage));
        }

        private void MenuTheme_Click(object sender, RoutedEventArgs e)
        {
            ThemeContextMenu.PlacementTarget = ThemeMenuButton;
            ThemeContextMenu.IsOpen = true;
        }

        private void ThemeModeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Models.AppTheme mode;
            if (sender == MenuItem_System)
                mode = Models.AppTheme.System;
            else if (sender == MenuItem_Light)
                mode = Models.AppTheme.Light;
            else
                mode = Models.AppTheme.Dark;

            _themeManagerService.ApplyThemeMode(mode);
            UpdateThemeMenuCheckState();
        }

        private void UpdateThemeMenuCheckState()
        {
            var current = _themeManagerService.CurrentMode;
            MenuItem_System.IsChecked = current == Models.AppTheme.System;
            MenuItem_Light.IsChecked = current == Models.AppTheme.Light;
            MenuItem_Dark.IsChecked = current == Models.AppTheme.Dark;
        }

        private void MenuInfo_Click(object sender, RoutedEventArgs e)
        {
            _navigationService.Navigate(typeof(AboutPage));
        }

        private async void MenuFolderShow_Click(object sender, RoutedEventArgs e)
        {
            var folders = ThisPcFolderHelper.GetItems();

            var panel = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, 8)
            };

            foreach (var folder in folders)
            {
                var toggle = new ToggleSwitch
                {
                    Content = folder.Name,
                    IsChecked = folder.IsHidden,
                    Margin = new Thickness(0, 4, 0, 0)
                };
                panel.Children.Add(toggle);
            }

            var dialog = new ContentDialog
            {
                Title = "此电脑文件夹",
                Content = new ScrollViewer
                {
                    MaxHeight = 320,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Content = panel
                },
                PrimaryButtonText = "应用",
                CloseButtonText = "取消"
            };

            var result = await _contentDialogService.ShowAsync(dialog, CancellationToken.None);
            if (result != ContentDialogResult.Primary)
                return;

            var toggles = panel.Children.OfType<ToggleSwitch>().ToList();
            for (int i = 0; i < folders.Count && i < toggles.Count; i++)
            {
                var folder = folders[i];
                bool hide = toggles[i].IsChecked == true;
                folder.IsHidden = hide;
                var res = ThisPcFolderHelper.SetHidden(folder.Clsid, hide);
                if (!res.success)
                {
                    _snackBarService.Show("操作失败", res.result, SymbolRegular.ShieldError16);
                    return;
                }
            }

            _snackBarService.Show("操作成功", "已更新此电脑文件夹显示设置", SymbolRegular.CheckmarkCircle16);
        }
    }
}