using System.Windows.Media.Animation;
using Accessibility;
using Prism.Mvvm;

namespace Yadex.Retirement.Dtos
{
    public class AllocationDto : BindableBase
    {
        public AllocationDto(int birthYear, int allocationYear, 
            decimal cash, 
            decimal r401, 
            decimal socialSecurity,
            decimal pension,
            string status,
            AllocationDto lastYearAllocation)
        {
            
        }

        private static string FormalizeNumber(decimal value)
        {
            var decimalNumber = decimal.Round(value, 2);
            var sign = decimalNumber switch
            {
                0 => "",
                >0 => "+",
                _ => "-"
            };
            
            return $"{sign}{decimalNumber:C2}";
        }
        
        public string AllocationYear
        {
            get => _year;
            set
            {
                _year = value;
                RaisePropertyChanged();
            }
        }

        private string _year;

        public string AllocationCash
        {
            get => _allocationCash;
            set
            {
                _allocationCash = value;
                RaisePropertyChanged();
            }
        }

        private string _allocationCash;

        public string Allocation401K
        {
            get => _allocation401K;
            set
            {
                _allocation401K = value;
                RaisePropertyChanged();
            }
        }

        private string _allocation401K;

        public string SocialSecurityAmount
        {
            get => _socialSecurityAmount;
            set
            {
                _socialSecurityAmount = value;
                RaisePropertyChanged();
            }
        }

        private string _socialSecurityAmount;

        public string PensionAmount
        {
            get => _pensionAmount;
            set
            {
                _pensionAmount = value;
                RaisePropertyChanged();
            }
        }

        private string _pensionAmount;

        public string AssetRemaining
        {
            get => _assetRemaining;
            set
            {
                _assetRemaining = value;
                RaisePropertyChanged();
            }
        }

        private string _assetRemaining;

        public string AssetDifference
        {
            get => _assetDifference;
            set
            {
                _assetDifference = value;
                RaisePropertyChanged();
            }
        }

        private string _assetDifference = "-";

        public string Status

        {
            get => _status;
            set
            {
                _status = value;
                RaisePropertyChanged();
            }
        }

        private string _status = AllocationStatusTypes.Actual;

        public string TotalAllocation
        {
            get => _totalAllocation;
            set
            {
                _totalAllocation = value;
                RaisePropertyChanged();
            }
        }

        private string _totalAllocation;
    }

}