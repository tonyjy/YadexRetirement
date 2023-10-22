namespace Yadex.Retirement.Services.AssetSvc;

public interface IAssetService
{
    /// <summary>
    /// Add asset
    /// </summary>
    /// <param name="year"></param>
    /// <param name="asset"></param>
    /// <returns>(bool Succeeded, string ErrorMessage, T Result)</returns>
    MsgResult<string> AddAsset(int year, Asset asset);

    /// <summary>
    /// Update the asset
    /// </summary>
    /// <param name="year"></param>
    /// <param name="updatedAsset"></param>
    /// <returns>(bool Succeeded, string ErrorMessage, T Result)</returns>
    MsgResult<string> UpdateAsset(int year, Asset updatedAsset);

    /// <summary>
    /// Remove the asset by ID
    /// </summary>
    /// <param name="year"></param>
    /// <param name="assetId"></param>
    /// <returns>(bool Succeeded, string ErrorMessage, T Result)</returns>
    MsgResult<string> DeleteAsset(int year, Guid assetId);

    /// <summary>
    /// Get the assets by year.
    /// </summary>
    /// <param name="year">4 digits integer as yyyy, e.g.like 2023</param>
    /// <returns>(bool Succeeded, string ErrorMessage, T Result)</returns>
    MsgResult<Asset[]> GetAssetsByYear(int year);
    
    /// <summary>
    /// return a Dictionary with key as year, value as Asset[] for all years.
    /// </summary>
    /// <param name="year"></param>
    /// <returns></returns>
    MsgResult<Dictionary<int,Asset[]>> GetYearAssetsDict();
}
