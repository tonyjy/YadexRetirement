namespace Yadex.Retirement.Models;

public class AssetAudit
{
    public AssetAudit(string actionName, Asset oldAsset, Asset newAsset, Asset[] oldAssets, Asset[] newAssets)
    {
        ActionName = actionName;
        OldAsset = oldAsset;
        NewAsset = newAsset;
        OldAssets = oldAssets;
        NewAssets = newAssets;
    }

    public string ActionName { get; }

    public DateTime LastUpdatedTime { get; } = DateTime.Now;

    public Asset OldAsset { get; }

    public Asset NewAsset { get; }

    public Asset[] OldAssets { get; }

    public Asset[] NewAssets { get; }
}
