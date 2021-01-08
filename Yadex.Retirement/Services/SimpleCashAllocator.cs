using System.Collections.Generic;
using System.Linq;
using Yadex.Retirement.Models;

namespace Yadex.Retirement.Services
{
    public class SimpleCashAllocator
    {
        public static void Allocate(List<Asset> assets, decimal amount)
        {

            var cashAssets = assets.Where(x => x.AssetType == AssetTypes.Cash).ToList();
            foreach (var asset in cashAssets)
            {
                if (asset.AssetAmount >= amount)
                {
                    var assetModified = asset with { AssetAmount = asset.AssetAmount - amount };
                    assets.Remove(asset);
                    assets.Add(assetModified);
                    break;
                }

                var assetZero = asset with { AssetAmount = 0 };
                amount -= asset.AssetAmount;
                assets.Remove(asset);
                assets.Add(assetZero);
            }
        }
    }
}