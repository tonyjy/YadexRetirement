using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using Prism.Mvvm;
using Yadex.Retirement.Dtos;
using Yadex.Retirement.Services;

namespace Yadex.Retirement
{
    public class MainWindowViewModel : BindableBase
    {
        private const string AllYears = "All Years";

        public MainWindowViewModel()
        {
            SettingsService = new YadexRetirementSettingsService();
            
            ResetViewModel();
            RefreshViewModel();
        }

        public IAssetService AssetService { get; set; }
        public IYadexRetirementSettingsService SettingsService { get; }

        private void ResetViewModel()
        {
            AllAssets = new ObservableCollection<PerformanceDto>();

            var list = new List<string> {AllYears};
            var thisYear = DateTime.Now.Year;
            list.AddRange(Enumerable.Range(thisYear, 20)
                .Select(x => (2*thisYear - x).ToString()));
            FilterYearList = new ObservableCollection<string>(list);
        }

        public void RefreshViewModel()
        {
            // get settings
            var (succeededSettings, errorSettings, settings) = SettingsService.GetYadexRetirementSettings();
            if (!succeededSettings)
            {
                MessageBox.Show($"Get all setting failed. {errorSettings}", "ERROR");
                return;
            }

            // get assets
            AssetService = new JsonFileAssetService(settings.AssetRootFolder);
            var (succeeded, errorMessage, allAssets) = AssetService.GetAllAssets();
            if (!succeeded)
            {
                MessageBox.Show($"Get all assets failed. {errorMessage}", "ERROR");
                return;
            }
            
            // bind to grid
            var dtoList = new List<PerformanceDto>();
            for (var i = 0; i < allAssets.Length; i++)
            {
                var curItem = allAssets[i];
                var nextItem = i < allAssets.Length - 1 ? allAssets[i + 1] : null;

                var dto = new PerformanceDto(curItem);
                if (nextItem != null && nextItem.AssetAmount != 0 && curItem.AssetName == nextItem.AssetName)
                    dto.PercentValue = (curItem.AssetAmount - nextItem.AssetAmount) / nextItem.AssetAmount;

                dtoList.Add(dto);
            }

            AllAssets = new ObservableCollection<PerformanceDto>(dtoList);
            YearSelected = DateTime.Now.Year.ToString();
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
                LatestAssetTotal = $"{_latestAssetTotalValue:C2}";
            }
        }

        private decimal _latestAssetTotalValue;

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
        #endregion

        private void FilterAssetsByYear(string yearSelected)
        {
            if (yearSelected == AllYears)
            {
                VisibleAssets = AllAssets;
                LatestAssetTotal = "";
                return;
            }

            var targetYear = int.Parse(yearSelected);
            var filteredList = AllAssets.Where(x => x.Asset.AssetDate.Year <= targetYear);

            VisibleAssets = LatestAssets = new ObservableCollection<PerformanceDto>(
                filteredList.GroupBy(x => x.AssetName).Select(grouping => grouping.First()));

            LatestAssetTotalValue = LatestAssets.Sum(x => x.Asset.AssetAmount);
        }

    }
}