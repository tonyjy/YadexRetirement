using System;
using System.Linq;
using Prism.Mvvm;
using Yadex.Retirement.Models;

namespace Yadex.Retirement.Dtos
{
    public class AllocationDto : BindableBase
    {
        public AllocationDto(int year, string status, decimal target)
        {
            Year = year;
            Status = status;
            Target = target;
        }

        public int Year { get; }
        public string Status { get; }
        
        public decimal Target { get; }

        public Asset[] PreAssets { get; set; }
        
        public Asset[] Assets 
        {
            get => _assets;
            set
            {
                _assets = value;
                AssetTotal = _assets.Sum(x => x.AssetAmount);
            }
        }
        private Asset[] _assets;


        #region Bindings

        /// <summary>
        ///     This property will contains the age and the year
        /// </summary>
        public string AgeYear
        {
            get => _ageYear;
            set
            {
                _ageYear = value;
                RaisePropertyChanged();
            }
        }

        private string _ageYear;

        public decimal CashAmount
        {
            get => _cashAmount;
            set
            {
                _cashAmount = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CashAmountText));
            }
        }
        private decimal _cashAmount;

        public string CashAmountText => $@"{(-_cashAmount).ToKilo()} / {AssetsHelper.GetTotalWithChange(Assets.ForCash(), PreAssets.ForCash())}";

        public decimal R401KAmount
        {
            get => _r401KAmount;
            set
            {
                _r401KAmount = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(R401KAmountText));
            }
        }
        private decimal _r401KAmount;

        public string R401KAmountText => $@"{(-_r401KAmount).ToKilo()} / {AssetsHelper.GetTotalWithChange(Assets.For401K(), PreAssets.For401K())}";

        public decimal AssetTotal
        {
            get => _assetTotal;
            set
            {
                _assetTotal = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(AssetTotalText));
            }
        }

        private decimal _assetTotal;
        public string AssetTotalText => AssetsHelper.GetTotalWithChange(Assets, PreAssets);

        public string AssetTotalChanged
        {
            get => _assetTotalChanged;
            set
            {
                _assetTotalChanged = value;
                RaisePropertyChanged();
            }
        }

        private string _assetTotalChanged;

        public decimal SocialSecurityAmount
        {
            get => _socialSecurityAmount;
            set
            {
                _socialSecurityAmount = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(SocialSecurityAmountText));
            }
        }

        private decimal _socialSecurityAmount;
        public string SocialSecurityAmountText => $"-{SocialSecurityAmount.ToKilo()}";

        public decimal PensionAmount
        {
            get => _pensionAmount;
            set
            {
                _pensionAmount = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(PensionAmountText));
            }
        }

        private decimal _pensionAmount;
        public string PensionAmountText => $"-{_pensionAmount.ToKilo()}";

        public decimal FixedAmount
        {
            get => _fixedAmount;
            set
            {
                _fixedAmount = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(FixedAmountText));
            }
        }

        private decimal _fixedAmount;
        public string FixedAmountText => $"-{_fixedAmount.ToKilo()} / {Assets.ForFixedTotal().ToKilo()}";

        public decimal TotalWithdrawal =>
            _fixedAmount + 
            _pensionAmount + 
            _socialSecurityAmount + 
            _r401KAmount + 
            _cashAmount;

        public string TotalWithdrawalText => $"{Math.Abs(TotalWithdrawal).ToKilo()} / target {Target.ToKilo()}";

        #endregion
    }
}