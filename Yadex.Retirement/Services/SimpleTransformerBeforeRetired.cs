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
                            },
                    { AssetType: AssetTypes.RetirementPension } =>
                        x with
                            {
                            AssetDate = assetDate,
                            AssetAmount = x.AssetAmount + 10000
                            },
                    { AssetType: AssetTypes.Retirement401K } =>
                        x with
                            {
                            AssetDate = assetDate,
                            AssetAmount = x.AssetAmount + (19500 * 1.07m)
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