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
                var assets = SimpleTransformerBeforeRetired.Transform(assetDate, preAssets);
                
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

            var r401KAge = BirthYear + 60;
            var pensionAge = BirthYear + 65;
            var maxAge = BirthYear + 95;

            // Retired Before 401K (age 60)
            AllocateBefore401K(minYr, r401KAge);

            // Retired Before Pension (age 65)
            AllocateBeforePension(r401KAge, pensionAge, maxAge);

            // Add social security and pension (age > 65)
            AllocateAfterPension(pensionAge, maxAge);
        }

        private void AllocateAfterPension(int pensionAge, int maxAge)
        {
            for (var year = pensionAge; year < maxAge; year++)
            {
                var preDto = AllocationDict[year - 1];
                var preAssets = preDto.Assets;

                var assetDate = new DateTime(year, 12, 31);
                var assets = SimpleTransformerBeforePension.Transform(assetDate, preAssets);

                // 401K
                var r401Amount = SimpleR401KAllocator.Allocate(assets, maxAge, year);

                // Cash
                var totalWithdrawal = GetTotalWithdrawal(preDto);
                var cashPortion = (totalWithdrawal - r401Amount - PensionIncome - SocialSecurityIncome) ;
                var cashAmount = cashPortion >= 0 ? cashPortion : 0m;
                SimpleCashAllocator.Allocate(assets, cashAmount);

                var preTotal = preDto.AssetTotal;
                var curTotal = assets.Sum(x => x.AssetAmount);

                var dto = new AllocationDto(year, AllocationStatusTypes.RetiredEstimated)
                {
                    AgeYear = new RetirementAge(year - BirthYear, year).ToString(),
                    CashAmount = cashAmount,
                    R401KAmount = r401Amount,
                    SocialSecurityAmount = SocialSecurityIncome,
                    PensionAmount = PensionIncome,
                    Assets = assets.ToArray(),
                    AssetTotalChanged = AssetsHelper.GetTotalWithChange(curTotal, preTotal)
                };
                AllocationDict.Add(year, dto);
            }
        }

        private void AllocateBeforePension(int r401KAge, int pensionAge, int maxAge)
        {
            for (var year = r401KAge; year < pensionAge; year++)
            {
                var preDto = AllocationDict[year - 1];
                var preAssets = preDto.Assets;

                var assetDate = new DateTime(year, 12, 31);
                var assets = SimpleTransformerBeforePension.Transform(assetDate, preAssets);

                // 401K
                var r401Amount = SimpleR401KAllocator.Allocate(assets, maxAge, year);

                // Cash
                var totalWithdrawal = GetTotalWithdrawal(preDto);
                var cashAmount = r401Amount >= totalWithdrawal ? 0m : totalWithdrawal - r401Amount;
                SimpleCashAllocator.Allocate(assets, cashAmount);

                var preTotal = preDto.AssetTotal;
                var curTotal = assets.Sum(x => x.AssetAmount);

                var dto = new AllocationDto(year, AllocationStatusTypes.RetiredEstimated)
                {
                    AgeYear = new RetirementAge(year - BirthYear, year).ToString(),
                    CashAmount = cashAmount,
                    R401KAmount = r401Amount,
                    Assets = assets.ToArray(),
                    AssetTotalChanged = AssetsHelper.GetTotalWithChange(curTotal, preTotal)
                };
                AllocationDict.Add(year, dto);
            }
        }

        private void AllocateBefore401K(int minYr, int r401KAge)
        {
            for (var year = minYr; year < r401KAge; year++)
            {
                var preDto = AllocationDict[year - 1];
                var preAssets = preDto.Assets;

                var assetDate = new DateTime(year, 12, 31);
                var assets = SimpleTransformerRetiredBefore401K.TransformAssets(assetDate, preAssets);

                var amount = _settings.RetirementIncome;
                SimpleCashAllocator.Allocate(assets, amount);

                var preTotal = preDto.AssetTotal;
                var curTotal = assets.Sum(x => x.AssetAmount);

                var totalWithdrawal = GetTotalWithdrawal(preDto);
                var dto = new AllocationDto(year, AllocationStatusTypes.RetiredEstimated)
                {
                    AgeYear = new RetirementAge(year - BirthYear, year).ToString(),
                    CashAmount = totalWithdrawal,
                    Assets = assets.ToArray(),
                    AssetTotalChanged = AssetsHelper.GetTotalWithChange(curTotal, preTotal)
                };
                AllocationDict.Add(year, dto);
            }
        }

        private decimal GetTotalWithdrawal(AllocationDto preDto)
        {
            return preDto.TotalWithdrawal == 0
                ? _settings.RetirementIncome
                : preDto.TotalWithdrawal * 1.01m;
        }
    }
}