using System;

namespace MyComputerManager.Services.Contracts
{
    public interface INavigationService
    {
        void SetFrame(System.Windows.Controls.Frame frame);

        bool Navigate(Type pageType);
    }
}