using System.Collections.Generic;
using System.Linq;
using Yadex.Retirement.Models;

namespace Yadex.Retirement.Services
{
    public class SimpleR401KAllocator
    {
        public static decimal Allocate(List<Asset> assets, int maxAge, int year)
        {
            var r401Assets = assets.Where(x =>
                x.AssetType == AssetTypes.Retirement401K ||
                x.AssetType == AssetTypes.Retirement401K).ToList(); ;

            foreach (var asset in r401Assets)
            {
                var assetModified = asset with
                    {
                    AssetAmount = (maxAge > year)
                        ? asset.AssetAmount - asset.AssetAmount / (maxAge - year)
                        : 0
                    };
                assets.Remove(asset);
                assets.Add(assetModified);
            }

            var r401KTotal = r401Assets.Sum(x => x.AssetAmount);
            return r401KTotal / (maxAge - year);
        }
    }
}