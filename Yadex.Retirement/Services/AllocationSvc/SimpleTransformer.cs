namespace Yadex.Retirement.Services.AllocationSvc;

public class SimpleTransformer
{
    private readonly decimal _rate;
    private readonly decimal _saving401K;

    public SimpleTransformer(decimal rate, decimal saving401K = 0m)
    {
        _rate = rate;
        _saving401K = saving401K;
    }

    public List<Asset> Transform(DateTime assetDate, Asset[] preAssets)
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
                { AssetType: AssetTypes.Retirement401K } =>
                    x with
                    {
                        AssetDate = assetDate,
                        AssetAmount = (x.AssetAmount + _saving401K) * (1 + _rate)
                    },
                _ =>
                    x with
                    {
                        AssetDate = assetDate,
                        AssetAmount = x.AssetAmount * (1 + _rate)
                    },
            };

            return asset;
        }).ToList();
    }
}
