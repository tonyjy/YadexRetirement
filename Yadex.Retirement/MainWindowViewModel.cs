using System.Collections.Generic;
using System.Collections.ObjectModel;
using Prism.Mvvm;
using Yadex.Retirement.Dtos;
using Yadex.Retirement.Services;

namespace Yadex.Retirement
{
    public class MainWindowViewModel : BindableBase
    {
        public MainWindowViewModel()
        {
            AssetService = new JsonFileAssetService(@"C:\Users\tony_\OneDrive\My Family\My Assets\JwFamily Assets\");

            ResetViewModel();
            RefreshViewModel();
        }

        public IAssetService AssetService { get; }

        private void ResetViewModel()
        {
            AllAssets = new ObservableCollection<PerformanceDto>();
        }

        public void RefreshViewModel()
        {
            var allAssets = AssetService.GetAllAssets();
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
        }

        #region Bindings

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

        #endregion
    }
}