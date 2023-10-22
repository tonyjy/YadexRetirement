using System.Windows;

namespace Yadex.Retirement;

public partial class MainWindowViewModel 
{
    /// <summary>
    /// Save to the setting file %LOCALAPPDATA%\Yadex\YadexRetirementSettings.json
    /// </summary>
    private void SaveSettings()
    {
        var result = SettingsService.UpdateYadexRetirementSettings(_settings);
        if (!result.Succeeded)
            MessageBox.Show($"Couldn't save to settings {result.ErrorMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    /// <summary>
    /// The assets are ordered by AssetName, then by year descending.
    /// </summary>
    private void CalcPerformance()
    {
        // Get the assets for the YearBefore
        var result = AssetService.GetAssetsByYear(YearBefore);
        var assetsYearBefore = result.Result;
        _assetTotalBefore = assetsYearBefore.Sum(x => x.AssetAmount);
        _assetDict[YearBefore] = assetsYearBefore;
        
        // target year
        result = AssetService.GetAssetsByYear(Year);
        var assetsYear = result.Result;
        var dtoList = new List<PerformanceDto>();
        foreach (var asset in assetsYear)
        {
            var assetBefore = assetsYearBefore.SingleOrDefault(x => x.AssetId == asset.AssetId);
            var performanceDto = CreatePerformanceDto(asset, assetBefore);
            dtoList.Add(performanceDto);
        }
        VisibleAssets = new ObservableCollection<PerformanceDto>(dtoList);
    }

    /// <summary>
    /// Calculate the performance for two assets with the same asset ID.
    /// </summary>
    /// <returns>PerformanceDto</returns>
    private static PerformanceDto CreatePerformanceDto(Asset asset, Asset assetBefore)
        => new(asset)
        {
            PercentValue = (assetBefore != null &&
                            assetBefore.AssetId == asset.AssetId &&
                            assetBefore.AssetAmount != 0)
                ? (asset.AssetAmount - assetBefore.AssetAmount) / assetBefore.AssetAmount
                : 0m
        };

    private void CalcAllocations()
    {
        var result = AllocationService.GetAllAllocations(_assetDict);

        AllAllocations = new ObservableCollection<AllocationDto>();
        if (!result.Succeeded)
        {
            MessageBox.Show(result.ErrorMessage, "Error in Calculate Allocation");
            return;
        }

        AllAllocations = new ObservableCollection<AllocationDto>(result.Result);
    }

}
