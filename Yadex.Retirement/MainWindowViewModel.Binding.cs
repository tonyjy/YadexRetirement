namespace Yadex.Retirement;

public partial class MainWindowViewModel 
{
    /// <summary>
    /// Binding for the year drop down
    /// </summary>
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

            RefreshViewModel();
            // FilterAssetsByYear(_yearSelected);
        }
    }

    private string _yearSelected;

    public int Year => String.IsNullOrEmpty(YearSelected)
        ? DateTime.Now.Year
        : Convert.ToInt32(YearSelected);

    public int YearBefore => Year - 1;

    private void InitYearsDropdown()
    {
        var list = new List<string> { };
        var thisYear = DateTime.Now.Year;
        list.AddRange(Enumerable.Range(thisYear, 20)
            .Select(x => (2 * thisYear - x).ToString()));
        FilterYearList = new ObservableCollection<string>(list);
        YearSelected = DateTime.Now.Year.ToString();
    }
    private void InitRetirementAgeDropdown()
    {
        var birthYear = _settings.BirthYear;

        var list = new RetirementAge[MaxRetirementYear - MinRetirementYear + 1];
        for (var i = 0; i < list.Length; i++)
        {
            var age = MinRetirementYear + i;
            var year = birthYear + age;
            list[i] = new RetirementAge(age, year);
            if (age == _settings.RetirementAge)
                _retirementAgeSelected = list[i];
        }

        var nowYear = DateTime.Now.Year;
        RetirementAges = new ObservableCollection<RetirementAge>(list.Where(x => x.Year > nowYear));
    }

    #region Top Section 

    /// <summary>
    /// Bindings for the top Grid 
    /// </summary>
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

    ////private ObservableCollection<PerformanceDto> _allAssets;

    ////public ObservableCollection<PerformanceDto> AllAssets
    ////{
    ////    get => _allAssets;
    ////    set
    ////    {
    ////        _allAssets = value;
    ////        RaisePropertyChanged();
    ////    }
    ////}

    //public ObservableCollection<PerformanceDto> LatestAssets
    //{
    //    get => _latestAssets;
    //    set
    //    {
    //        _latestAssets = value;
    //        RaisePropertyChanged();
    //    }
    //}

    //private ObservableCollection<PerformanceDto> _latestAssets;

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
        get => _assetTotal;
        set
        {
            _assetTotal = value;
            RaisePropertyChanged();

            LatestAssetTotal = AssetsHelper.GetTotalWithChange(
                _assetTotal, _assetTotalBefore);
        }
    }

    private decimal _assetTotal;
    private decimal _assetTotalBefore;

    private Dictionary<int, Asset[]> _assetDict = new Dictionary<int, Asset[]>();

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

            // check if different with settings
            if (_settings.RetirementAge == _retirementAgeSelected.Age)
                return;

            _settings.RetirementAge = _retirementAgeSelected.Age;
            SaveSettings();
            CalcAllocations();
        }
    }

    private RetirementAge _retirementAgeSelected;

    public decimal RetirementIncome
    {
        get => _retirementIncome;
        set
        {
            _retirementIncome = value;
            RaisePropertyChanged();
        }
    }

    private decimal _retirementIncome;

    public string RetirementIncomeText
    {
        get => _retirementIncomeText;
        set
        {
            if (decimal.TryParse(value, out var decimalValue))
                RetirementIncome = decimal.Round(decimalValue, 2);

            _retirementIncomeText = $"{RetirementIncome:N0}";
            RaisePropertyChanged();

            // check if different with settings
            if (_settings.RetirementIncome == RetirementIncome)
                return;

            _settings.RetirementIncome = RetirementIncome;
            SaveSettings();
            CalcAllocations();
        }
    }

    private string _retirementIncomeText;

    public decimal RiskFactor
    {
        get => _riskFactor;
        set
        {
            _riskFactor = value;
            RaisePropertyChanged();
        }
    }

    private decimal _riskFactor;

    public string RiskFactorText
    {
        get => _riskFactorText;
        set
        {
            if (decimal.TryParse(value, out var decimalValue))
                RiskFactor = decimal.Round(decimalValue, 2);

            _riskFactorText = $"{RiskFactor:N0}";
            RaisePropertyChanged();

            // check if different with settings
            if (_settings.RiskFactor == RiskFactor)
                return;

            _settings.RiskFactor = RiskFactor;
            SaveSettings();
            CalcAllocations();
        }
    }

    private string _riskFactorText;

    public decimal RetirementIncomeAdjustmentRate
    {
        get => _retirementIncomeAdjustmentRate;
        set
        {
            _retirementIncomeAdjustmentRate = value;
            RaisePropertyChanged();

            // check if different with settings
            if (_settings.RetirementIncomeAdjustmentRate == RetirementIncomeAdjustmentRate)
                return;

            _settings.RetirementIncomeAdjustmentRate = RetirementIncomeAdjustmentRate;
            SaveSettings();
            CalcAllocations();
        }
    }

    private decimal _retirementIncomeAdjustmentRate;

    public decimal InvestmentReturnRate
    {
        get => _investmentReturnRate;
        set
        {
            _investmentReturnRate = value;
            RaisePropertyChanged();

            // check if different with settings
            if (_settings.InvestmentReturnRate == InvestmentReturnRate)
                return;

            _settings.InvestmentReturnRate = InvestmentReturnRate;
            SaveSettings();
            CalcAllocations();
        }
    }

    private decimal _investmentReturnRate;

    public decimal TransitionYear401KSaving
    {
        get => _transitionYear401KSaving;
        set
        {
            _transitionYear401KSaving = value;
            RaisePropertyChanged();
        }
    }

    private decimal _transitionYear401KSaving;

    public string TransitionYear401KSavingText
    {
        get => _transitionYear401KSavingText;
        set
        {
            if (decimal.TryParse(value, out var decimalValue))
                TransitionYear401KSaving = decimal.Round(decimalValue, 2);

            _transitionYear401KSavingText = $"{TransitionYear401KSaving:N0}";
            RaisePropertyChanged();

            // check if different with settings
            if (_settings.TransitionYear401KSaving == TransitionYear401KSaving)
                return;

            _settings.TransitionYear401KSaving = TransitionYear401KSaving;
            SaveSettings();
            CalcAllocations();
        }
    }

    public ObservableCollection<AllocationDto> AllAllocations { get; set; }

    private string _transitionYear401KSavingText;

    #endregion Bindings
}
