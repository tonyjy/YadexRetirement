using System;
using System.Collections.ObjectModel;
using Prism.Mvvm;
using Yadex.Retirement.Models;
using Yadex.Retirement.Services;

namespace Yadex.Retirement
{
    public class MainWindowViewModel : BindableBase
    {
        public IAssetService AssetService { get; }

        public MainWindowViewModel()
        {
            AssetService = new JsonFileAssetService(@"C:\Users\tony_\OneDrive\My Family\My Assets\JwFamily Assets\");

            ResetViewModel();
            RefreshViewModel();
        }

        private void ResetViewModel()
        {
            AllAssets = new ObservableCollection<Asset>();
        }

        public void RefreshViewModel()
        {
            AllAssets = new ObservableCollection<Asset>(AssetService.GetAllAssets());
        }

        #region Bindings

        private ObservableCollection<Asset> _allAssets;

        public ObservableCollection<Asset> AllAssets
        {
            get => _allAssets;
            set
            {
                _allAssets = value;
                RaisePropertyChanged();
            }
        }

        public Asset AssetSelected
        {
            get => _assetSelected;
            set
            {
                _assetSelected = value;
                RaisePropertyChanged();
            }
        }

        private Asset _assetSelected;
        
        #endregion

        #region Actions

        public void AddAsset()
        {
            
            var asset = new Asset(
                Guid.NewGuid(),
                "Merrill Personal Investment", 
                223000m,
                new DateTime(2020, 12, 15));
            
            AllAssets = new ObservableCollection<Asset>(new [] {asset});
        }

        #endregion
    }
}