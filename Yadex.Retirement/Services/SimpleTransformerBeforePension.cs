using System;
using System.Collections.Generic;
using System.Linq;
using Yadex.Retirement.Models;

namespace Yadex.Retirement.Services
{
    public class SimpleTransformerBeforePension
    {
        public static List<Asset> Transform(DateTime assetDate, Asset[] preAssets)
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
                    _ =>
                        x with
                            {
                            AssetDate = assetDate,
                            AssetAmount = x.AssetAmount * 1.02m
                            },
                };

                return asset;
            }).ToList();
        }
    }
}