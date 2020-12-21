using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Prism.Mvvm;
using Yadex.Retirement.Models;

namespace Yadex.Retirement.Views
{
    public class AssetDialogViewModel : BindableBase
    {
        public AssetDialogViewModel(MainWindowViewModel parent, Asset asset = null)
        {
            Parent = Guard.NotNull(nameof(parent), parent);
            OldAsset = asset;

            ResetViewModel();
        }

        public MainWindowViewModel Parent { get; }
        public Asset OldAsset { get; }

        private bool IsNew => OldAsset == null;

        public void ResetViewModel()
        {
            GetAssetNameList();

            GetDefaultValues();
        }

        private void GetDefaultValues()
        {
            if (IsNew)
            {
                AssetId = Guid.NewGuid();
                AssetName = string.Empty;
                AssetDate = DateTime.Today;
                AssetAmount = 0m;
                return;
            }

            AssetId = OldAsset.AssetId;
            AssetName = OldAsset.AssetName;
            AssetDate = OldAsset.AssetDate;
            AssetAmount = OldAsset.AssetAmount;
        }

        private void GetAssetNameList()
        {
            AssetNameList = new ObservableCollection<string>(Parent
                .AllAssets
                .Select(x => x.AssetName)
                .Distinct()
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .OrderBy(x => x));
        }

        #region Bindings

        public string ActionButtonContent => IsNew ? "Create" : "Update";

        public Guid AssetId
        {
            get => _assetId;
            set
            {
                _assetId = value;
                RaisePropertyChanged();
            }
        }

        private Guid _assetId;

        public string AssetName
        {
            get => _assetName;
            set
            {
                _assetName = value;
                RaisePropertyChanged();
            }
        }

        private string _assetName;

        public DateTime AssetDate
        {
            get => _assetDate;
            set
            {
                _assetDate = value;
                RaisePropertyChanged();
            }
        }

        private DateTime _assetDate;

        public decimal AssetAmount
        {
            get => _assetAmount;
            set
            {
                _assetAmount = value;
                RaisePropertyChanged();
            }
        }

        private decimal _assetAmount;

        public ObservableCollection<string> AssetNameList
        {
            get => _assetNameList;
            set
            {
                _assetNameList = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<string> _assetNameList;

        #endregion

        #region Actions

        public List<string> ValidateViewModel()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(AssetName))
                errors.Add("Asset name is empty. Please provide asset name.");

            if (AssetAmount <= 0m)
                errors.Add("Asset amount is zero or negative. Please enter positive number");

            if (AssetDate > DateTime.Today)
                errors.Add("Asset date is in the future. Please enter the date to be today or earlier");

            return errors;
        }

        public List<string> SaveViewModel()
        {
            var errors = ValidateViewModel();
            if (errors.Count > 0)
                return errors;

            var newAsset = new Asset(AssetId, AssetName, AssetAmount, AssetDate);

            var (succeeded, errorMessage) = IsNew
                ? Parent.AssetService.AddAsset(newAsset)
                : Parent.AssetService.UpdateAsset(newAsset);

            if (!succeeded)
                errors.Add(errorMessage);

            return errors;
        }

        #endregion
    }
}