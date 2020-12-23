using Prism.Mvvm;
using Yadex.Retirement.Models;

namespace Yadex.Retirement.Dtos
{
    public class PerformanceDto : BindableBase
    {
        public PerformanceDto(Asset asset)
        {
            Asset = asset;
            AssetId = asset.AssetId.ToString();
            AssetName = asset.AssetName;
            AssetType= asset.AssetType;
            AssetAmount = $"{asset.AssetAmount:C2}";
            AssetDate = $"{asset.AssetDate:d}";
            LastUpdatedTime = $"{asset.LastUpdatedTime:s}";
        }

        public Asset Asset { get; }

        public string AssetId
        {
            get => _assetId;
            set
            {
                _assetId = value;
                RaisePropertyChanged();
            }
        }

        private string _assetId;

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

        public string AssetType
        {
            get => _assetType;
            set
            {
                _assetType = value;
                RaisePropertyChanged();
            }
        }

        private string _assetType;

        public string AssetAmount
        {
            get => _assetAmount;
            set
            {
                _assetAmount = value;
                RaisePropertyChanged();
            }
        }

        private string _assetAmount;

        public string AssetDate
        {
            get => _assetDate;
            set
            {
                _assetDate = value;
                RaisePropertyChanged();
            }
        }

        private string _assetDate;

        public string LastUpdatedTime
        {
            get => _lastUpdatedTime;
            set
            {
                _lastUpdatedTime = value;
                RaisePropertyChanged();
            }
        }

        private string _lastUpdatedTime;

        public decimal PercentValue
        {
            get => _percentValue;
            set
            {
                _percentValue = value;
                PercentString = $"{value:P2}";
            }
        }

        private decimal _percentValue;

        public string PercentString
        {
            get => _percentString;
            set
            {
                _percentString = value;
                RaisePropertyChanged();
            }
        }

        private string _percentString = "-";
    }

}