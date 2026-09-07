using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using MyComputerManager.Helpers;
using MyComputerManager.Models;
using MyComputerManager.Services.Contracts;
using MyComputerManager.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui.Controls;

namespace MyComputerManager.ViewModels
{
    public partial class DetailPageViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;
        private readonly IDataService _dataService;
        private readonly ISnackBarService _snackBarService;
        private readonly IDialogService _dialogService;

        public DetailPageViewModel(INavigationService navigationService, IDataService dataService, ISnackBarService snackBarService, IDialogService dialogService)
        {
            _navigationService = navigationService;
            _dataService = dataService;
            _snackBarService = snackBarService;
            _dialogService = dialogService;
            OriItem = (NamespaceItem)_dataService.GetData();
            if (OriItem.CLSID == null)
            {
                Item = OriItem;
                var guid = Guid.NewGuid().ToString().ToUpper();
                Item.CLSID = "{" + guid + "}";
                OriItem = null;
            }
            else
            {
                Item = OriItem.Clone();
            }
        }

        private NamespaceItem OriItem;

        [ObservableProperty]
        private NamespaceItem item;

        [RelayCommand]
        private void Ok()
        {
            var res = SaveToOrigin();
            if (res.success)
                _navigationService.Navigate(typeof(MainPage));
            else
                _snackBarService.Show("操作失败", res.result, SymbolRegular.ShieldError16);
        }

        [RelayCommand]
        private void Cancel()
        {
            _navigationService.Navigate(typeof(MainPage));
        }

        [RelayCommand]
        private void Apply()
        {
            var res = SaveToOrigin();
            if (res.success)
                _snackBarService.Show("操作成功", Item.Name, SymbolRegular.CheckmarkCircle16);
            else
                _snackBarService.Show("操作失败", res.result, SymbolRegular.ShieldError16);
        }

        public CommonResult SaveToOrigin()
        {
            var res = NamespaceHelper.UpdateItem(Item);
            if (res.success)
            {
                if (OriItem != null)
                {
                    OriItem.Desc = Item.Desc;
                    OriItem.Tip = Item.Tip;
                    OriItem.Name = Item.Name;
                    OriItem.Icon = Item.Icon;
                    OriItem.ExePath = Item.ExePath;
                    OriItem.IconPath = Item.IconPath;
                }
                else
                {
                    var vm = _dataService.GetVM();
                    OriItem = Item.Clone();
                    vm.AddItem(OriItem);
                }
            }
            return res;
        }

        [RelayCommand]
        private void OpenIcon()
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = StringHelper.BuildFilter("exe,ico,dll");
            if (d.ShowDialog() ?? false)
                Item.IconPath = d.FileName;

            Item.Icon = IconHelper.ReadIcon(Item.IconPath);
            if (Item.Icon == null)
                Item.IconPath = "";
        }

        [RelayCommand]
        private void Copy(object content)
        {
            System.Windows.Clipboard.SetText(content.ToString());
            _snackBarService.Show("已复制到剪切板", content.ToString(), SymbolRegular.Info16, ControlAppearance.Secondary, 3000);
        }

        [RelayCommand]
        private void CopyIconPath()
        {
            System.Windows.Clipboard.SetText(Item.IconPath);
            if (Item.IconPath != "")
                _snackBarService.Show("已复制到剪切板", Item.IconPath, SymbolRegular.Info16, ControlAppearance.Secondary, 3000);
        }

        [RelayCommand]
        private void ClearIcon()
        {
            Item.Icon = null;
            Item.IconPath = "";
        }

        [RelayCommand]
        private void Drop(object obj)
        {
            DragEventArgs e = (DragEventArgs)obj;
            var filelist = e.Data.GetData("FileDrop");
            if (filelist == null)
            {
                _snackBarService.Show("不支持的操作", "拖放exe/ico/dll文件到此以打开图标", SymbolRegular.Info16, ControlAppearance.Secondary, 5000);
                return;
            }
            string filepath = ((string[])filelist).First();
            FileInfo fileInfo = new FileInfo(filepath);

            if (!fileInfo.Exists)
            {
                _snackBarService.Show("操作失败", "文件不存在或无法访问", SymbolRegular.Info16, ControlAppearance.Secondary, 5000);
                return;
            }

            if (!new List<string>() { ".exe", ".ico", ".dll" }.Contains(fileInfo.Extension.ToLower()))
            {
                _snackBarService.Show("不支持的文件类型", "拖放exe/ico/dll文件到此以打开图标", SymbolRegular.Info16, ControlAppearance.Secondary, 5000);
                return;
            }

            Item.IconPath = filepath;
            Item.Icon = IconHelper.ReadIcon(Item.IconPath);
            if (Item.Icon == null)
                Item.IconPath = "";
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (OriItem == null)
            {
                _navigationService.Navigate(typeof(MainPage));
                return;
            }
            bool isConfirmed = false;
            await App.Current.Dispatcher.BeginInvoke(new Action(async () =>
            {
                isConfirmed = await _dialogService.ShowDialog(
                    new DialogMessage("警告", $"此操作将删除项目\"{OriItem?.Name ?? "未命名项目"}\"，且无法恢复！"),
                    "确认删除", "取消操作");
            }));

            if (isConfirmed)
            {
                var res2 = NamespaceHelper.DeleteItem(Item);
                if (res2.success)
                {
                    var vm = _dataService.GetVM();
                    vm.DeleteItem(OriItem);
                    _navigationService.Navigate(typeof(MainPage));
                    if (OriItem?.Name != null)
                        _snackBarService.Show("操作成功", $"已删除{OriItem?.Name}", SymbolRegular.Info16);
                }
                else
                {
                    _snackBarService.Show("操作失败", res2.result, SymbolRegular.ShieldError16);
                    Item.IsEnabled = !Item.IsEnabled;
                }
            }
        }

        [RelayCommand]
        private async Task Export()
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "registry file|*.reg";
            if (dialog.ShowDialog() != true)
                return;
            var res1 = await Task.Run(() =>
            {
                try
                {
                    var filename1 = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                    var filename2 = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                    ProcessStartInfo psi1 = new ProcessStartInfo("regedit", $"-e {filename1} {Item.RegKey_CLSID}");
                    ProcessStartInfo psi2 = new ProcessStartInfo("regedit", $"-e {filename2} {Item.RegKey_Namespace}");
                    Process p1 = Process.Start(psi1);
                    p1.WaitForExit();
                    Process p2 = Process.Start(psi2);
                    p2.WaitForExit();

                    var text1 = File.ReadAllText(filename1);
                    var text2 = File.ReadAllText(filename2);
                    text2 = text2.Replace("Windows Registry Editor Version 5.00", "");
                    File.WriteAllText(dialog.FileName, text1 + text2, Encoding.Unicode);
                    return 0;
                }
                catch
                {
                    return 1;
                }
            });
            if (res1 == 0)
            {
                _snackBarService.Show("已导出到", dialog.FileName, SymbolRegular.Info16, ControlAppearance.Secondary, 3000);
            }
            else
            {
                _snackBarService.Show("操作失败", "原因未知", SymbolRegular.ShieldError16);
            }
        }
    }
}