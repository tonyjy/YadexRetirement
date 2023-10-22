namespace Yadex.Retirement.Services;

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
    public MsgResult<AllocationDto[]> GetAllAllocations(Dictionary<int, Asset[]> assets)
    {
        Guard.NotNull(nameof(assets), assets);

        // if assets are empty, return now
        if (assets.Count == 0)
            return new MsgResult<AllocationDto[]>(true, string.Empty, Array.Empty<AllocationDto>());

        // calc the actual assets, 
        GetActualYears(assets);

        // Apply Risk Factor
        ApplyRiskFactor();

        // estimate until retirement year inclusively, e.g. 55 
        GetTransitionYears();

        // estimate after retirement year, e.g. starting 56 to 95
        GetRetirementYears();

        return new MsgResult<AllocationDto[]>(true, string.Empty, AllocationDict.Values.ToArray());
    }

    private void ApplyRiskFactor()
    {
        var dto = AllocationDict[AllocationDict.Keys.Max()];

        var assets = dto.Assets.ToList();
        SimpleAllocator.Allocate(assets, assets.ForCash(), _settings.RiskFactor);

        dto.Assets = assets.ToArray();
    }

    /// <summary>
    /// Calculate the actual for assets. Add into the dictionary
    /// </summary>
    /// <param name="assets">All assets over the years</param>
    /// <returns>The max year in the assets </returns>
    private void GetActualYears(Dictionary<int, Asset[]> assets)
    {
        // calculate the actual allocations
        foreach(var kv in assets)
        {
            var year = kv.Key;
            var curAssets = kv.Value;
            var curTotal = curAssets.Sum(x => x.AssetAmount);
            var preDto = AllocationDict.ContainsKey(year - 1) ? AllocationDict[year - 1] : null;
            var preTotal = preDto?.AssetTotal ?? 0m;

            var dto = new AllocationDto(year, AllocationStatusTypes.Actual, _settings.RetirementIncome)
            {
                AgeYear = new RetirementAge(year - BirthYear, year).ToString(),
                AssetTotal = curTotal,
                Assets = curAssets,
                PreAssets = preDto?.Assets ?? new Asset[0],
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
            var transformer = new SimpleTransformer(_settings.InvestmentReturnRate, _settings.TransitionYear401KSaving);
            var assets = transformer.Transform(assetDate, preAssets);

            var preTotal = preDto.AssetTotal;
            var curTotal = assets.Sum(x => x.AssetAmount);

            var dto = new AllocationDto(year, AllocationStatusTypes.Estimated, _settings.RetirementIncome)
            {
                AgeYear = new RetirementAge(year - BirthYear, year).ToString(),
                Assets = assets.ToArray(),
                PreAssets = preDto.Assets,
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
        var r401KAge = BirthYear + 60;
        var pensionAge = BirthYear + 65;
        var maxAge = BirthYear + 95;

        // Retired Before 401K (age 60)
        var minYr = AllocationDict.Keys.Max() + 1;
        AllocateRetiredEarlyBefore401K(minYr, r401KAge);

        // Retired Before Pension (age 65)
        minYr = AllocationDict.Keys.Max() + 1;
        AllocateRetiredEarlyBeforeSocialAndPension(minYr, pensionAge, maxAge);

        // Add social security and pension (age > 65)
        minYr = AllocationDict.Keys.Max() + 1;
        AllocateRetiredFully(minYr, maxAge);
    }

    #region Retired

    /// <summary>
    /// Retired but before 60 years old (401K). Cash only.
    /// </summary>
    /// <param name="minYr"></param>
    /// <param name="r401KAge"></param>
    private void AllocateRetiredEarlyBefore401K(int minYr, int r401KAge)
    {
        for (var year = minYr; year < r401KAge; year++)
        {
            var preDto = AllocationDict[year - 1];
            var preAssets = preDto.Assets;

            var assetDate = new DateTime(year, 12, 31);

            var transformer = new SimpleTransformer(_settings.InvestmentReturnRate);
            var assets = transformer.Transform(assetDate, preAssets);

            var target = GetTarget(preDto);
            var cashWithdrawal = SimpleAllocator.Allocate(assets, assets.ForCash(), target);

            var preTotal = preDto.AssetTotal;
            var curTotal = assets.Sum(x => x.AssetAmount);

            var dto = new AllocationDto(year, AllocationStatusTypes.RetiredEstimated, target)
            {
                AgeYear = new RetirementAge(year - BirthYear, year).ToString(),
                CashAmount = cashWithdrawal,
                Assets = assets.ToArray(),
                PreAssets = preDto.Assets,
                AssetTotalChanged = AssetsHelper.GetTotalWithChange(curTotal, preTotal)
            };
            AllocationDict.Add(year, dto);
        }
    }

    private void AllocateRetiredEarlyBeforeSocialAndPension(int r401KAge, int pensionAge, int maxAge)
    {
        for (var year = r401KAge; year < pensionAge; year++)
        {
            var preDto = AllocationDict[year - 1];
            var preAssets = preDto.Assets;

            var assetDate = new DateTime(year, 12, 31);
            var transformer = new SimpleTransformer(_settings.InvestmentReturnRate);
            var assets = transformer.Transform(assetDate, preAssets);

            // 401K
            var r401Amount = SimpleR401KAllocator.Allocate(assets, maxAge, year);

            // Cash
            var target = GetTarget(preDto);
            var cashAmount = r401Amount >= target ? 0m : target - r401Amount;
            var cashWithdrawal = SimpleAllocator.Allocate(assets, assets.ForCash(), cashAmount);

            var preTotal = preDto.AssetTotal;
            var curTotal = assets.Sum(x => x.AssetAmount);

            var dto = new AllocationDto(year, AllocationStatusTypes.RetiredEstimated, target)
            {
                AgeYear = new RetirementAge(year - BirthYear, year).ToString(),
                CashAmount = cashWithdrawal,
                R401KAmount = r401Amount,
                Assets = assets.ToArray(),
                PreAssets = preDto.Assets,
                AssetTotalChanged = AssetsHelper.GetTotalWithChange(curTotal, preTotal)
            };
            AllocationDict.Add(year, dto);
        }
    }

    private void AllocateRetiredFully(int pensionAge, int maxAge)
    {
        for (var year = pensionAge; year < maxAge; year++)
        {
            var preDto = AllocationDict[year - 1];
            var preAssets = preDto.Assets;

            var assetDate = new DateTime(year, 12, 31);
            var transformer = new SimpleTransformer(_settings.InvestmentReturnRate);
            var assets = transformer.Transform(assetDate, preAssets);

            // 401K
            var r401Amount = SimpleR401KAllocator.Allocate(assets, maxAge, year);

            // Cash
            var target = GetTarget(preDto);
            var cashPortion = (target - r401Amount - PensionIncome - SocialSecurityIncome);
            var cashAmount = cashPortion >= 0 ? cashPortion : 0m;
            var cashWithdrawal = SimpleAllocator.Allocate(assets, assets.ForCash(), cashAmount);

            // Still missing target 
            var shortAmount = target - cashWithdrawal - r401Amount - SocialSecurityIncome - PensionIncome;
            if (shortAmount > 0)
            {
                r401Amount += SimpleAllocator.Allocate(assets, assets.For401K(), shortAmount);
            }

            var preTotal = preDto.AssetTotal;
            var curTotal = assets.Sum(x => x.AssetAmount);

            var dto = new AllocationDto(year, AllocationStatusTypes.RetiredEstimated, target)
            {
                AgeYear = new RetirementAge(year - BirthYear, year).ToString(),
                CashAmount = cashWithdrawal,
                R401KAmount = r401Amount,
                SocialSecurityAmount = SocialSecurityIncome,
                PensionAmount = PensionIncome,
                Assets = assets.ToArray(),
                PreAssets = preDto.Assets,
                AssetTotalChanged = AssetsHelper.GetTotalWithChange(curTotal, preTotal)
            };
            AllocationDict.Add(year, dto);
        }
    }
    private decimal GetTarget(AllocationDto preDto)
    {
        return preDto.Target == 0
            ? _settings.RetirementIncome
            : preDto.Target * (1 + _settings.RetirementIncomeAdjustmentRate);
    }

    #endregion
}
