using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using MyComputerManager.Services.Contracts;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace MyComputerManager.Services
{
    public class SnackBarService : ISnackBarService
    {
        private readonly ISnackbarService _snackbarService;

        public SnackBarService(IServiceProvider serviceProvider)
        {
            _snackbarService = serviceProvider.GetRequiredService<ISnackbarService>();
        }

        public void SetSnackbarPresenter(SnackbarPresenter presenter)
        {
            _snackbarService.SetSnackbarPresenter(presenter);
        }

        public void Show(string title, string message, SymbolRegular icon = SymbolRegular.Info20,
            ControlAppearance appearance = ControlAppearance.Secondary, int timeout = 5000, bool showclosebutton = true)
        {
            if (_snackbarService.GetSnackbarPresenter() == null)
                return;

            _snackbarService.Show(title, message, appearance, new SymbolIcon { Symbol = icon }, TimeSpan.FromMilliseconds(timeout));
        }
    }
}