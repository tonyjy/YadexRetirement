using System.Globalization;
using System.IO;
using System.Windows;
using Prism.Mvvm;
using Yadex.Retirement.Services;

namespace Yadex.Retirement.Views
{
    public class SettingsDialogViewModel : BindableBase
    {
        public SettingsDialogViewModel(MainWindowViewModel parent)
        {
            Parent = Guard.NotNull(nameof(parent), parent);
            YadexRetirementSettingsService = new YadexRetirementSettingsService();

            ResetViewModel(parent);
        }

        private void ResetViewModel(MainWindowViewModel parent)
        {
            // get settings from settings service
            var (succeededSettings, errorSettings, settings) = parent.SettingsService.GetYadexRetirementSettings();
            if (!succeededSettings)
            {
                MessageBox.Show($"Get all setting failed. {errorSettings}", "ERROR");
                return;
            }

            // init BirthYearList
            var thisYear = DateTime.Now.Year;
            BirthYearList = new ObservableCollection<int>(Enumerable
                .Range(thisYear - 18, 120)
                .Select(x => (2 * thisYear - x)));
            
            // set bindings
            AssetRootFolder = settings.AssetRootFolder;
            BirthYearSelected = settings.BirthYear;
            PensionIncomeText = settings.PensionIncome.ToString(CultureInfo.InvariantCulture);
            SocialSecurityIncomeText = settings.SocialSecurityIncome.ToString(CultureInfo.InvariantCulture);
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

        public ObservableCollection<int> BirthYearList
        {
            get => _birthYearList;
            set
            {
                _birthYearList = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<int> _birthYearList;

        public int BirthYearSelected
        {
            get => _birthYearSelected;
            set
            {
                _birthYearSelected = value;
                RaisePropertyChanged();
            }
        }

        private int _birthYearSelected;

        public decimal SocialSecurityIncome
        {
            get => _socialSecurityIncome;
            set
            {
                _socialSecurityIncome = value;
                RaisePropertyChanged();
            }
        }

        private decimal _socialSecurityIncome;

        public string SocialSecurityIncomeText
        {
            get => _socialSecurityIncomeText;
            set
            {
                if (decimal.TryParse(value, out var decimalValue))
                    SocialSecurityIncome = decimal.Round(decimalValue, 2);

                _socialSecurityIncomeText = $"{SocialSecurityIncome:N2}";
                RaisePropertyChanged();
            }
        }

        private string _socialSecurityIncomeText;

        public decimal PensionIncome
        {
            get => _pensionIncome;
            set
            {
                _pensionIncome = value;
                RaisePropertyChanged();
            }
        }

        private decimal _pensionIncome;

        public string PensionIncomeText
        {
            get => _pensionIncomeText;
            set
            {
                if (decimal.TryParse(value, out var decimalValue))
                    PensionIncome = decimal.Round(decimalValue, 2);

                _pensionIncomeText = $"{PensionIncome:N2}";
                RaisePropertyChanged();
            }
        }

        private string _pensionIncomeText;

        #endregion

        #region Actions

        public List<string> ValidateViewModel()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(AssetRootFolder))
                errors.Add("Data directory is invalid. It is empty. ");

            if (!Directory.Exists(AssetRootFolder))
                errors.Add($"Data directory is not existing - {AssetRootFolder}.");

            if (BirthYearSelected < 0)
                errors.Add($"Birth Year is invalid. The value is negative - {BirthYearSelected}.");

            if (SocialSecurityIncome < 0)
                errors.Add($"Social Security is invalid. The value is negative - {SocialSecurityIncome}.");

            if (PensionIncome < 0)
                errors.Add($"Pension is invalid. The value is negative - {BirthYearSelected}.");

            return errors;
        }

        public List<string> SaveViewModel()
        {
            var errors = ValidateViewModel();
            if (errors.Count > 0)
                return errors;

            var settings = new YadexRetirementSettings(AssetRootFolder)
            {
                BirthYear = BirthYearSelected,
                SocialSecurityIncome = SocialSecurityIncome,
                PensionIncome = PensionIncome
            };

            var (succeeded, errorMessage, _) = YadexRetirementSettingsService.UpdateYadexRetirementSettings(settings);

            if (!succeeded)
                errors.Add(errorMessage);

            return errors;
        }

        #endregion
    }
}