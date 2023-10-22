using System.Collections.ObjectModel;
using System.Windows;
using Prism.Mvvm;
using Yadex.Retirement.Services;

namespace Yadex.Retirement;

public partial class MainWindowViewModel : BindableBase
{
    public MainWindowViewModel()
    {
        SettingsService = new YadexRetirementSettingsService();
        ResetViewModel();
        RefreshViewModel();
    }

    public IAssetService AssetService { get; set; }
    public IAllocationService AllocationService => new SimpleAllocationService(_settings);
    public IYadexRetirementSettingsService SettingsService { get; }

    private YadexRetirementSettings _settings;

    // Const variables
    private const int MinRetirementYear = 25;
    private const int MaxRetirementYear = 75;

    private void ResetViewModel()
    {
        var succeeded = InitSettings();
        if (!succeeded) 
            return;

        InitYearsDropdown();
        InitRetirementAgeDropdown();
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
        
        // Set the asset service based on root folder
        AssetService = new JsonFileAssetService(_settings.AssetRootFolder);
        
        // Initialize the bindings
        RetirementIncomeText = $"{_settings.RetirementIncome:N0}";
        RiskFactorText = $"{_settings.RiskFactor:N0}";
        RetirementIncomeAdjustmentRate = _settings.RetirementIncomeAdjustmentRate;
        InvestmentReturnRate = _settings.InvestmentReturnRate;
        TransitionYear401KSavingText = $"{_settings.TransitionYear401KSaving:N0}";
        return true;
    }
    
    public void RefreshViewModel()
    {
        InitSettings();
        CalcPerformance();
        CalcAllocations();
    }
    
}
