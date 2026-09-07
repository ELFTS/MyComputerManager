using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyComputerManager.Helpers;
using MyComputerManager.Models;
using MyComputerManager.Services.Contracts;
using MyComputerManager.Views;
using System.Collections.ObjectModel;
using Wpf.Ui.Controls;

namespace MyComputerManager.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;
        private readonly IDataService _dataService;
        private readonly ISnackBarService _snackBarService;

        public MainPageViewModel(INavigationService navigationService, IDataService dataService, ISnackBarService snackBarService)
        {
            _navigationService = navigationService;
            _dataService = dataService;
            _snackBarService = snackBarService;
            Items = (ObservableCollection<NamespaceItem>)_dataService.GetData();
            dataService.SetVM(this);
        }

        [ObservableProperty]
        private ObservableCollection<NamespaceItem> items;

        [RelayCommand]
        private void GoDetail(object item)
        {
            _dataService.SetData(item);
            _navigationService.Navigate(typeof(DetailPage));
        }

        [RelayCommand]
        private void ToggleEnabled(object obj)
        {
            NamespaceItem item = (NamespaceItem)obj;
            var res = NamespaceHelper.SetEnabled(item, item.IsEnabled);
            if (!res.success)
            {
                _snackBarService.Show("操作失败", res.result, SymbolRegular.ShieldError16);
                item.IsEnabled = !item.IsEnabled;
            }
        }

        public void DeleteItem(NamespaceItem item)
        {
            if (item != null)
                if (Items.Contains(item))
                    Items.Remove(item);
        }

        public void AddItem(NamespaceItem item)
        {
            Items?.Add(item);
        }
    }
}