using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using MyComputerManager.Models;
using MyComputerManager.Services.Contracts;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace MyComputerManager.Services
{
    public class DialogService : IDialogService
    {
        private readonly IContentDialogService _contentDialogService;

        public DialogService(IServiceProvider serviceProvider)
        {
            _contentDialogService = serviceProvider.GetRequiredService<IContentDialogService>();
        }

        public void SetDialogHost(ContentDialogHost dialogHost)
        {
            _contentDialogService.SetDialogHost(dialogHost);
        }

        public async Task<bool> ShowDialog(DialogMessage content, string primaryText, string secondaryText)
        {
            var dialog = new ContentDialog
            {
                Title = content.Title,
                Content = new TextBlock
                {
                    Text = content.Message,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 4, 0, 0),
                    FontSize = 14
                },
                PrimaryButtonText = primaryText,
                SecondaryButtonText = secondaryText,
                PrimaryButtonAppearance = ControlAppearance.Danger
            };

            var result = await _contentDialogService.ShowAsync(dialog, CancellationToken.None);
            return result == ContentDialogResult.Primary;
        }
    }
}