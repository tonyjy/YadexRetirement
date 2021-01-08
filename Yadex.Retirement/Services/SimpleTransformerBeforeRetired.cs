using System;
using System.Linq;
using Yadex.Retirement.Models;

namespace Yadex.Retirement.Services
{
    public class SimpleTransformerBeforeRetired
    {
        public static Asset[] Transform(DateTime assetDate, Asset[] preAssets)
        {
            return preAssets.Select(x =>
            {
                var asset = x switch
                {
                    { AssetType: AssetTypes.Fixed } => 
                        x with 
                            {
                            AssetDate = assetDate,
                            AssetAmount = x.AssetAmount * 1.02m
                            },
                    { AssetType: AssetTypes.RetirementPension } =>
                        x with
                            {
                            AssetDate = assetDate,
                            AssetAmount = x.AssetAmount + 10000
                            },
                    _ =>
                        x with
                            {
                            AssetDate = assetDate,
                            AssetAmount = x.AssetAmount * 1.04m
                            },
                };
                    
                return asset;
            }).ToArray();
        }
    }
}