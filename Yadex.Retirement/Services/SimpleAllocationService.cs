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


            // calculate estimated years
            // var maxEstimateYear = birthYear + 90;


            return new MsgResult<AllocationDto[]>(true, string.Empty, AllocationDict.Values.ToArray());
        }

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
                    AssetTotalChanged = AssetsHelper.GetTotalWithChange(curTotal, preTotal)
                };
                
                //var cash = assetsForYear
                //    .Where(x => x.AssetType == AssetTypes.Cash)
                //    .Sum(x => x.AssetAmount);

                //var r401 = assetsForYear
                //    .Where(x => x.AssetType == AssetTypes.Retirement401K || x.AssetType == AssetTypes.Retirement401K)
                //    .Sum(x => x.AssetAmount);

                //var fixedAsset = assetsForYear
                //    .Where(x => x.AssetType == AssetTypes.Fixed)
                //    .Sum(x => x.AssetAmount);

                //_birthYear = birthYear;
                //_allocationYear = allocationYear;


                //var cash = assetsActual
                //    .Where(x => x.AssetType == AssetTypes.Cash)
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

                AllocationDict.Add(year, dto);
            }
        }
    }
}