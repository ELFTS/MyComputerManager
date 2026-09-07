using MyComputerManager.Models;

namespace MyComputerManager.Services.Contracts
{
    public interface ISettingsService
    {
        AppSettings Load();

        void Save(AppSettings settings);
    }
}