using System;
using Wpf.Ui.Controls;

namespace MyComputerManager.Services.Contracts
{
    public interface ISnackBarService
    {
        void SetSnackbarPresenter(Wpf.Ui.Controls.SnackbarPresenter presenter);

        void Show(string title, string message, SymbolRegular icon = SymbolRegular.Info20,
            ControlAppearance appearance = ControlAppearance.Secondary, int timeout = 5000, bool showclosebutton = true);
    }
}