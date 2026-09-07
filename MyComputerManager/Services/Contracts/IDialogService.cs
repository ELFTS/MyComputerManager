using System;
using System.Threading.Tasks;
using MyComputerManager.Models;
using Wpf.Ui.Controls;

namespace MyComputerManager.Services.Contracts
{
    public interface IDialogService
    {
        void SetDialogHost(ContentDialogHost dialogHost);

        Task<bool> ShowDialog(DialogMessage content, string primaryText, string secondaryText);
    }
}