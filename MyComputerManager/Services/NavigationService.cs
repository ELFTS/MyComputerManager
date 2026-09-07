using System;
using System.Windows.Controls;

namespace MyComputerManager.Services
{
    public class NavigationService : Contracts.INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private Frame _frame;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void SetFrame(Frame frame)
        {
            _frame = frame;
        }

        public bool Navigate(Type pageType)
        {
            if (_frame == null)
                return false;

            var page = _serviceProvider.GetService(pageType) as System.Windows.FrameworkElement;
            if (page == null)
                return false;

            _frame.Navigate(page);
            return true;
        }
    }
}