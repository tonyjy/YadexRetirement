namespace Yadex.Retirement.Models;

public static class AssetsHelper
{
    public static Asset[] ForYear(this Asset[] assets, int year)
    {
        var filteredList = assets.Where(x => x.AssetDate.Year <= year);

        return filteredList
            .GroupBy(x => x.AssetName)
            .Select(grouping => grouping.First())
            .ToArray();
    }

    public static decimal ForTotal(this Asset[] assets)
        => assets.Sum(x => x.AssetAmount);

    public static decimal ForCashTotal(this Asset[] assets)
        => assets.ForCash().Sum(x => x.AssetAmount);
    public static decimal For401KTotal(this Asset[] assets)
        => assets.For401K().Sum(x => x.AssetAmount);
    public static decimal ForPensionTotal(this Asset[] assets)
        => assets.ForPension().Sum(x => x.AssetAmount);
    public static decimal ForFixedTotal(this Asset[] assets)
        => assets.ForFixed().Sum(x => x.AssetAmount);

    public static Asset[] ForCash(this Asset[] assets)
        => assets.FilteredByType(AssetTypes.Cash);

    public static Asset[] For401K(this Asset[] assets)
        => assets.FilteredByType(AssetTypes.Retirement401K);

    public static List<Asset> ForCash(this List<Asset> assets)
        => assets.ToArray().FilteredByType(AssetTypes.Cash).ToList();

    public static List<Asset> For401K(this List<Asset> assets)
        => assets.ToArray().FilteredByType(AssetTypes.Retirement401K).ToList();

    public static Asset[] ForPension(this Asset[] assets)
        => assets.FilteredByType(AssetTypes.RetirementPension);

    public static Asset[] ForFixed(this Asset[] assets)
        => assets.FilteredByType(AssetTypes.Fixed);

    public static Asset[] FilteredByType(this Asset[] assets, string type)
        => assets == null
            ? Array.Empty<Asset>()
            : assets.Where(x => x.AssetType == type).ToArray();

    public static string GetTotalWithChange(decimal curVal, decimal preVal)
    {
        var changeYtd = preVal == 0 ? 0 : curVal - preVal;
        var percentYtd = preVal == 0 ? 0 : changeYtd / preVal;
        var sign = changeYtd > 0 ? "+" : "";
        var changed = preVal == 0 ? "(0k 0.0%)" : $"({sign}{changeYtd / 1000:N0}k {percentYtd:P1})";

        return $"{curVal / 1000000:N3}m {changed}";
    }

    public static string GetTotalWithChange(Asset[] curAssets, Asset[] preAssets)
        => GetTotalWithChange(curAssets.ForTotal(), preAssets.ForTotal());
}
