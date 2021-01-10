using System.Collections.Generic;
using System.Linq;
using Yadex.Retirement.Models;

namespace Yadex.Retirement.Services
{
    public class SimpleCashAllocator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="assets"></param>
        /// <param name="amount">The amount required</param>
        /// <returns>Total withdrawal </returns>
        public static decimal Allocate(List<Asset> assets, decimal amount)
        {
            var totalAllocated = 0m;
            
            var preCash = assets.Where(x => x.AssetType == AssetTypes.Cash).ToList();
            foreach (var asset in preCash)
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
}