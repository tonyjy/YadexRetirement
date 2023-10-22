namespace Yadex.Retirement.Services.AllocationSvc;

public class SimpleAllocator
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="assets"></param>
    /// <param name="amount">The amount required</param>
    /// <returns>Total withdrawal </returns>
    public static decimal Allocate(List<Asset> assets, List<Asset> targetAssets, decimal amount)
    {
        var totalAllocated = 0m;
        foreach (var asset in targetAssets)
        {
            if (asset.AssetAmount >= amount)
            {
                totalAllocated += amount;
                var assetModified = asset with { AssetAmount = asset.AssetAmount - amount };
                assets.Remove(asset);
                assets.Add(assetModified);
                break;
            }

            totalAllocated += asset.AssetAmount;
            var assetZero = asset with { AssetAmount = 0 };
            amount -= asset.AssetAmount;
            assets.Remove(asset);
            assets.Add(assetZero);
        }

        return totalAllocated;
    }
}
