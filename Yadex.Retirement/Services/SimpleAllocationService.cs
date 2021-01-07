using System;
using System.Collections.Generic;
using System.Linq;
using Yadex.Retirement.Common;
using Yadex.Retirement.Dtos;
using Yadex.Retirement.Models;

namespace Yadex.Retirement.Services
{
    public class SimpleAllocationService : IAllocationService
    {
        public SimpleAllocationService(YadexRetirementSettings settings)
        {
            _settings = Guard.NotNull(nameof(settings), settings);
        }

        private readonly YadexRetirementSettings _settings;

        private Dictionary<int, AllocationDto> AllocationDict { get; } = new Dictionary<int, AllocationDto>();

        private int BirthYear => _settings.BirthYear;
        private int RetirementAge => _settings.RetirementAge;
        private decimal PensionIncome => _settings.PensionIncome;
        private decimal SocialSecurityIncome => _settings.SocialSecurityIncome;

        // Range 0 - 100, set to 5.0 percent as default
        private decimal IncreasePercent = 4.0m;
        /// <summary>
        /// This is the main entry point for calculate the allocations
        /// </summary>
        public MsgResult<AllocationDto[]> GetAllAllocations(Asset[] assets)
        {
            Guard.NotNull(nameof(assets), assets);

            // if assets are empty, return now
            if (assets.Length == 0)
                return new MsgResult<AllocationDto[]>(true, string.Empty, Array.Empty<AllocationDto>());

            GetActualYears(assets);

            GetTransitionYears();

            GetRetirementYears();

            return new MsgResult<AllocationDto[]>(true, string.Empty, AllocationDict.Values.ToArray());
        }

        /// <summary>
        /// Calculate the actual for assets. Add into the dictionary
        /// </summary>
        /// <param name="assets">All assets over the years</param>
        /// <returns>The max year in the assets </returns>
        private void GetActualYears(Asset[] assets)
        {
            // calculate the actual allocations
            var years = assets.Select(x => x.AssetDate.Year).Distinct().ToArray();
            var minYr = years.Min();
            var maxYr = years.Max();
            
            for (var year = minYr ; year <= maxYr; year++)
            {
                var curAssets = AssetsHelper.GetAssetsForYear(assets, year);
                var curTotal = curAssets.Sum(x => x.AssetAmount);
                var preTotal = AllocationDict.ContainsKey(year - 1)
                    ? AllocationDict[year - 1].AssetTotal
                    : 0;

                var dto = new AllocationDto(year, AllocationStatusTypes.Actual)
                {
                    AgeYear = new RetirementAge(year - BirthYear, year).ToString(),
                    AssetTotal = curTotal,
                    Assets = curAssets,
                    AssetTotalChanged = AssetsHelper.GetTotalWithChange(curTotal, preTotal)
                };
                

                AllocationDict.Add(year, dto);
            }
        }

        /// <summary>
        /// Calculate transition years. Assume 4% increase and self-sustained.
        /// </summary>
        private void GetTransitionYears()
        {
            var minYr = AllocationDict.Keys.Max() + 1;
            var maxYr = BirthYear + RetirementAge;
            for (var year = minYr; year <= maxYr; year++)
            {
                var preDto = AllocationDict[year - 1];
                var preAssets = preDto.Assets;

                var assetDate = new DateTime(year, 12, 31);
                var assets = preAssets.Select(x =>
                {
                    var asset = x switch
                    {
                       { AssetType: AssetTypes.Fixed } a => 
                           x with 
                               {
                                    AssetDate = assetDate,
                                    AssetAmount = x.AssetAmount * 1.02m
                               },
                       { AssetType: AssetTypes.RetirementPension } a =>
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
                
                var preTotal = preDto.AssetTotal;
                var curTotal = assets.Sum(x => x.AssetAmount);

                var dto = new AllocationDto(year, AllocationStatusTypes.Estimated)
                {
                    AgeYear = new RetirementAge(year - BirthYear, year).ToString(),
                    Assets = assets,
                    AssetTotalChanged = AssetsHelper.GetTotalWithChange(curTotal, preTotal)
                };
                AllocationDict.Add(year, dto);
            }
        }

        /// <summary>
        /// Calculate retirement years.
        ///    - If less than 60 years, 
        ///    - 60+ years, 401K withdrawal is allowed
        ///    - 65+ years, SS and pension is typically allowed
        /// </summary>
        private void GetRetirementYears()
        {
            var preYear = AllocationDict.Keys.Max();

            var minYr = preYear + 1;

            var r401kAge = BirthYear + 60;
            var pensionAge = BirthYear + 65;
            var maxAge = BirthYear + 95;

            // cash is allowed
            for (var year = minYr; year < r401kAge; year++)
            {
                var preDto = AllocationDict[year - 1];
                var preAssets = preDto.Assets;
                
                var assetDate = new DateTime(year, 12, 31);
                var assets = preAssets.Select(x =>
                {
                    var asset = x switch
                    {
                        { AssetType: AssetTypes.Fixed } a =>
                            x with
                                {
                                AssetDate = assetDate,
                                AssetAmount = x.AssetAmount * 1.02m
                                },
                        { AssetType: AssetTypes.RetirementPension } a =>
                            x with
                                {
                                AssetDate = assetDate,
                                AssetAmount = x.AssetAmount * 1.04m
                            },
                        _ =>
                            x with
                                {
                                AssetDate = assetDate,
                                AssetAmount = x.AssetAmount * 1.04m
                                },
                    };

                    return asset;
                }).ToList();


                var cashAssets = assets.Where(x => x.AssetType == AssetTypes.Cash).ToList();

                var amount = _settings.RetirementIncome;
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

                var preTotal = preDto.AssetTotal;
                var curTotal = assets.Sum(x => x.AssetAmount);

                var dto = new AllocationDto(year, AllocationStatusTypes.RetiredEstimated)
                {
                    AgeYear = new RetirementAge(year - BirthYear, year).ToString(),
                    CashAmount = _settings.RetirementIncome,
                    Assets = assets.ToArray(),
                    AssetTotalChanged = AssetsHelper.GetTotalWithChange(curTotal, preTotal)
                };
                AllocationDict.Add(year, dto);
            }

            // cash, 401K, and RRSP are allowed
            for (var year = r401kAge; year < pensionAge; year++)
            {
            }
            
            // Add social security and pension 
            for (var year = pensionAge; year < maxAge; year++)
            {
            }

            //    .Sum(x => x.AssetAmount);

            //var r401 = assetsActual
            //    .Where(x => x.AssetType == AssetTypes.Retirement401K || x.AssetType == AssetTypes.Retirement401K)
            //    .Sum(x => x.AssetAmount); ;

            //var fixedAsset = assetsActual
            //    .Where(x => x.AssetType == AssetTypes.Fixed)
            //    .Sum(x => x.AssetAmount); ;

            //_cashAmount = cash;
            //_r401Amount = r401;
            //_fixedAsset = fixedAsset;
            //_socialSecurity = socialSecurity;
            //_pension = pension;
            //_totalAllocationAmount = _cashAmount + _r401Amount + _fixedAsset + _socialSecurity + _pension;

            //// Update the 

            //AllocationCash = FormalizeNumber(_cashAmount);
            //Allocation401K = FormalizeNumber(_r401Amount);
            //SocialSecurityAmount = FormalizeNumber(_socialSecurity);
            //PensionAmount = FormalizeNumber(_pension);
            //TotalAllocation = FormalizeNumber(_totalAllocationAmount);
        }

    }
}