using System.Collections.Generic;
using System.IO;
using System.Windows;
using Prism.Mvvm;
using Yadex.Retirement.Common;
using Yadex.Retirement.Models;
using Yadex.Retirement.Services;

namespace Yadex.Retirement.Views
{
    public class SettingsDialogViewModel : BindableBase
    {
        public SettingsDialogViewModel(MainWindowViewModel parent)
        {
            Parent = Guard.NotNull(nameof(parent), parent);
            YadexRetirementSettingsService = new YadexRetirementSettingsService();

            // get settings
            var (succeededSettings, errorSettings, settings) = parent.SettingsService.GetYadexRetirementSettings();
            if (!succeededSettings)
            {
                MessageBox.Show($"Get all setting failed. {errorSettings}", "ERROR");
                return;
            }

            AssetRootFolder = settings.AssetRootFolder;
        }

        public MainWindowViewModel Parent { get; }

        public IYadexRetirementSettingsService YadexRetirementSettingsService { get; set; }

        #region Bindings

        public string AssetRootFolder
        {
            get => _rootFolder;
            set
            {
                _rootFolder = value;
                RaisePropertyChanged();
            }
        }

        private string _rootFolder;

        #endregion

        #region Actions

        public List<string> ValidateViewModel()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(AssetRootFolder))
                errors.Add("AssetRootFolder is empty. ");

            if (!Directory.Exists(AssetRootFolder))
                errors.Add("AssetRootFolder is not a folder.");

            return errors;
        }

        public List<string> SaveViewModel()
        {
            var errors = ValidateViewModel();
            if (errors.Count > 0)
                return errors;

            var settings = new YadexRetirementSettings(AssetRootFolder);

            var (succeeded, errorMessage, _) = YadexRetirementSettingsService.UpdateYadexRetirementSettings(settings);

            if (!succeeded)
                errors.Add(errorMessage);

            return errors;
        }

        #endregion
    }
}