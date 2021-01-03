using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Prism.Mvvm;
using Yadex.Retirement.Dtos;
using Yadex.Retirement.Models;
using Yadex.Retirement.Services;

namespace Yadex.Retirement
{
    public class MainWindowViewModel : BindableBase
    {

        public MainWindowViewModel()
        {
            SettingsService = new YadexRetirementSettingsService();

            ResetViewModel();
            RefreshViewModel();
        }

        public IAssetService AssetService { get; set; }
        public IYadexRetirementSettingsService SettingsService { get; }

        private YadexRetirementSettings _settings;

        // Const variables
        private const string AllYears = "All Years";
        private const int MinRetirementYear = 25;
        private const int TargetRetirementYear = 55;
        private const int MaxRetirementYear = 75;

        private void ResetViewModel()
        {
            AllAssets = new ObservableCollection<PerformanceDto>();

            if (!InitSettings()) return;

            InitAssetYears();

            InitRetirementAge();
        }

        private bool InitSettings()
        {
            // get settings
            var (succeededSettings, errorSettings, settings) = SettingsService.GetYadexRetirementSettings();
            if (!succeededSettings)
            {
                MessageBox.Show($"Get all setting failed. {errorSettings}", "ERROR");
                return false;
            }

            _settings = settings;
            return true;
        }

        private void InitAssetYears()
        {
            var list = new List<string> { AllYears };
            var thisYear = DateTime.Now.Year;
            list.AddRange(Enumerable.Range(thisYear, 20)
                .Select(x => (2 * thisYear - x).ToString()));
            FilterYearList = new ObservableCollection<string>(list);
            YearSelected = DateTime.Now.Year.ToString();
        }
        private void InitRetirementAge()
        {
            var birthYear = _settings.BirthYear;

            var list = new RetirementAge[MaxRetirementYear - MinRetirementYear + 1];
            for (var i = 0; i < list.Length; i++)
            {
                var age = MinRetirementYear + i;
                var year = birthYear + age;
                list[i] = new RetirementAge(age, year);
                if (age == TargetRetirementYear)
                    _retirementAgeSelected = list[i];
            }

            var nowYear = DateTime.Now.Year;
            RetirementAges = new ObservableCollection<RetirementAge>(list.Where(x => x.Year > nowYear));
        }

        public void RefreshViewModel()
        {
            FetchAllAssetsFromService();

            CalculateAssertsPerformance();
        }

        /// <summary>
        /// The assets are ordered by AssetName, then by year descending.
        /// </summary>
        private void CalculateAssertsPerformance()
        {
            var allAssets = _allAssetsFromService;
            var length = allAssets.Length;

            var dtoList = new PerformanceDto[length];
            for (var i = 0; i < length; i++)
            {
                var curItem = allAssets[i];
                var nextItem = i < length - 1 ? allAssets[i + 1] : null;

                dtoList[i] = CreatePerformanceDto(curItem, nextItem);
            }

            AllAssets = new ObservableCollection<PerformanceDto>(dtoList);
        }

        /// <summary>
        /// Calculate the performance only if asset name is the same and asset amount is not zero.
        /// </summary>
        /// <param name="curItem">Current Asset</param>
        /// <param name="nextItem">Asset In Previous Year</param>
        /// <returns>PerformanceDto</returns>
        private static PerformanceDto CreatePerformanceDto(Asset curItem, Asset nextItem)
            => new (curItem)
            {
                PercentValue = (nextItem != null &&
                                nextItem.AssetName == curItem.AssetName &&
                                nextItem.AssetAmount != 0)
                    ? (curItem.AssetAmount - nextItem.AssetAmount) / nextItem.AssetAmount
                    : 0m
            };
        
        /// <summary>
        /// Get all the assets from IAssetService to _allAssetsFromService. 
        /// </summary>
        private void FetchAllAssetsFromService()
        {
            _allAssetsFromService = Array.Empty<Asset>();

            AssetService = new JsonFileAssetService(_settings.AssetRootFolder);
            var (succeeded, errorMessage, allAssets) = AssetService.GetAllAssets();
            if (!succeeded)
            {
                MessageBox.Show($"Get all assets failed. {errorMessage}", "ERROR");
                return;
            }

            _allAssetsFromService = allAssets;
        }


        #region Bindings

        public ObservableCollection<PerformanceDto> VisibleAssets
        {
            get => _visibleAssets;
            set
            {
                _visibleAssets = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<PerformanceDto> _visibleAssets;


        private ObservableCollection<PerformanceDto> _allAssets;

        public ObservableCollection<PerformanceDto> AllAssets
        {
            get => _allAssets;
            set
            {
                _allAssets = value;
                RaisePropertyChanged();

                FilterAssetsByYear(_yearSelected);
            }
        }

        public ObservableCollection<PerformanceDto> LatestAssets
        {
            get => _latestAssets;
            set
            {
                _latestAssets = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<PerformanceDto> _latestAssets;

        public PerformanceDto AssetSelected
        {
            get => _assetSelected;
            set
            {
                _assetSelected = value;
                RaisePropertyChanged();
            }
        }

        private PerformanceDto _assetSelected;

        public string LatestAssetTotal
        {
            get => _latestAssetTotal;
            set
            {
                _latestAssetTotal = value;
                RaisePropertyChanged();
            }
        }

        private string _latestAssetTotal;

        public decimal LatestAssetTotalValue
        {
            get => _latestAssetTotalValue;
            set
            {
                _latestAssetTotalValue = value;
                RaisePropertyChanged();

                var changeYtd = _yearBeforeAssetTotalValue == 0 ? 0 : _latestAssetTotalValue - _yearBeforeAssetTotalValue;
                var percentYtd = _yearBeforeAssetTotalValue == 0 ? 0 : changeYtd / _yearBeforeAssetTotalValue;
                var sign = changeYtd > 0 ? "+" : "";

                var ytd = _yearBeforeAssetTotalValue == 0 ? "-" : $"({sign}{changeYtd / 1000:N0}k {percentYtd:P1})";
                LatestAssetTotal = $"{_latestAssetTotalValue / 1000000:N3}m {ytd}";
            }
        }

        private decimal _latestAssetTotalValue;
        private decimal _yearBeforeAssetTotalValue;


        public ObservableCollection<string> FilterYearList
        {
            get => _filterYearList;
            set
            {
                _filterYearList = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<string> _filterYearList;

        public string YearSelected
        {
            get => _yearSelected;
            set
            {
                _yearSelected = value;
                RaisePropertyChanged();

                FilterAssetsByYear(_yearSelected);
            }
        }

        private string _yearSelected;

        public ObservableCollection<RetirementAge> RetirementAges
        {
            get => _retirementAges;
            set
            {
                _retirementAges = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<RetirementAge> _retirementAges;

        public RetirementAge RetirementAgeSelected
        {
            get => _retirementAgeSelected;
            set
            {
                _retirementAgeSelected = value;
                RaisePropertyChanged();
            }
        }

        private RetirementAge _retirementAgeSelected;
        private Asset[] _allAssetsFromService;

        #endregion

        private void FilterAssetsByYear(string yearSelected)
        {
            if (yearSelected == AllYears || yearSelected == null)
            {
                VisibleAssets = AllAssets;
                LatestAssetTotal = "";
                return;
            }

            var targetYear = int.Parse(yearSelected);
            YearBefore = targetYear - 1;

            // The year before
            var yearBeforeList = AllAssets.Where(x => x.Asset.AssetDate.Year <= YearBefore);
            var yearBeforeAssets = yearBeforeList.GroupBy(x => x.AssetName).Select(grouping => grouping.First());
            _yearBeforeAssetTotalValue = yearBeforeAssets.Sum(x => x.Asset.AssetAmount);

            // target year
            var filteredList = AllAssets.Where(x => x.Asset.AssetDate.Year <= targetYear);

            VisibleAssets = LatestAssets = new ObservableCollection<PerformanceDto>(
                filteredList.GroupBy(x => x.AssetName).Select(grouping => grouping.First()));

            LatestAssetTotalValue = LatestAssets.Sum(x => x.Asset.AssetAmount);

        }

        public int YearBefore { get; set; }
    }
}