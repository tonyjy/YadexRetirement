using Prism.Mvvm;

namespace Yadex.Retirement.Dtos
{
    public class AllocationDto : BindableBase
    {
        public AllocationDto(int year, string status)
        {
            Year = year;
            Status = status;
        }

        public int Year { get; }
        public string Status { get; }

        private static string FormalizeNumber(decimal value)
        {
            var decimalNumber = decimal.Round(value, 2);
            var sign = decimalNumber switch
            {
                0 => "",
                > 0 => "+",
                _ => "-"
            };

            return $"{sign}{decimalNumber:C2}";
        }

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

        public string CashAmountText => FormalizeNumber(_cashAmount);

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

        public string R401KAmountText => FormalizeNumber(_r401KAmount);

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
        public string AssetTotalText => FormalizeNumber(_assetTotal);

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
        public string SocialSecurityAmountText => FormalizeNumber(SocialSecurityAmount);

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
        public string PensionAmountText => FormalizeNumber(_pensionAmount);

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
        public string FixedAmountText => FormalizeNumber(_fixedAmount);

        public decimal TotalWithdrawal =>
            _fixedAmount + 
            _pensionAmount + 
            _socialSecurityAmount + 
            _r401KAmount + 
            _cashAmount;

        public string TotalWithdrawalText => FormalizeNumber(TotalWithdrawal);

        #endregion
    }
}