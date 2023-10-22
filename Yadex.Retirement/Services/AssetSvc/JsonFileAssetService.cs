using System.IO;
using System.Text.Json;

namespace Yadex.Retirement.Services.AssetSvc;

/// <summary>
///     Json file implementation for <see cref="IAssetService" />.
/// </summary>
public class JsonFileAssetService : IAssetService
{
    // Json file's root folder
    private readonly string _rootPath;
    
    // Json file's search pattern
    private const string SearchPattern = "Asset_????.json";
    
    /// <summary>
    ///     Constructor with rootPath
    /// </summary>
    /// <param name="rootPath"></param>
    public JsonFileAssetService(string rootPath)
    {
        _rootPath = rootPath;
    }

    /// <summary>
    ///     Add an asset <see cref="Asset" />.
    /// </summary>
    /// <param name="year"></param>
    /// <param name="asset"></param>
    /// <returns></returns>
    public MsgResult<string> AddAsset(int year, Asset asset)
    {
        try
        {
            var (succeeded, errorMessage, assets) = GetAssetsByYear(year);
            if (!succeeded)
                throw new Exception($"Get assets failed | year={year}. {errorMessage}");


            // validate 
            var existing = assets.SingleOrDefault(x => x.AssetId == asset.AssetId);
            if (existing != null)
                throw new Exception($"Record has been found for {asset.AssetId.ToString()}");

            // act
            var savingAssets = new List<Asset>(assets) { asset };

            SaveAssets(year, savingAssets);

            return new MsgResult<string>();
        }
        catch (Exception e)
        {
            return new MsgResult<string>($"Error happened while add asset. \n{e.Message}\n{e.StackTrace}");
        }
    }


    /// <summary>
    ///     Update an asset <see cref="Asset" />.
    /// </summary>
    /// <param name="year"></param>
    /// <param name="updatedAsset"></param>
    /// <returns></returns>
    public MsgResult<string> UpdateAsset(int year, Asset updatedAsset)
    {
        try
        {
            var (succeeded, errorMessage, assets) = GetAssetsByYear(year);
            if (!succeeded)
                throw new Exception($"Get assets failed | year={year}. {errorMessage}");

            // validate 
            var assetId = updatedAsset.AssetId;
            var asset = assets.SingleOrDefault(x => x.AssetId == assetId);
            if (asset == null)
                throw new Exception($"Record cannot be found for {assetId}");

            // remove the existing one and add updated one
            var savingAssets = assets.Where(x => x.AssetId != assetId).ToList();
            updatedAsset.LastUpdatedTime = DateTime.Now;
            savingAssets.Add(updatedAsset);
            
            SaveAssets(year, savingAssets);
            return new MsgResult<string>();
        }
        catch (Exception e)
        {
            return new MsgResult<string>($"Error happened to update asset. \n{e.Message}\n{e.StackTrace}");
        }
    }

    /// <summary>
    ///     Delete an asset <see cref="Asset" />.
    /// </summary>
    /// <param name="year"></param>
    /// <param name="assetId"></param>
    /// <returns></returns>
    public MsgResult<string> DeleteAsset(int year, Guid assetId)
    {
        try
        {
            var (succeeded, errorMessage, assets) = GetAssetsByYear(year);
            if (!succeeded)
                throw new Exception($"Get assets failed | year={year}. {errorMessage}");

            // validate 
            var asset = assets.SingleOrDefault(x => x.AssetId == assetId);
            if (asset == null)
                throw new Exception($"Record cannot be found for {assetId.ToString()}");

            // remove the asset from array
            var savingAssets = assets.Where(x => x.AssetId != asset.AssetId);
            SaveAssets(year, savingAssets);

            return new MsgResult<string>();
        }
        catch (Exception e)
        {
            return new MsgResult<string>($"Error happened to delete. \n{e.Message}\n{e.StackTrace}");
        }
    }

    private void SaveAssets(int year, IEnumerable<Asset> assets)
    {
        // Get file path for the year
        var filePath = GetFilePath(year);

        // Save to the file path
        var content = assets.OrderBy(x => x.AssetType).ThenBy(x => x.AssetName).ToArray();
        File.WriteAllText(filePath, JsonSerializer.Serialize(content));
    }
    
    /// <summary>
    ///     Check if root folder exists
    /// </summary>
    /// <exception cref="FileNotFoundException"></exception>
    private void CheckRootDir()
    {
        // If directory is not existing, create it. 
        if (!Directory.Exists(_rootPath))
            Directory.CreateDirectory(_rootPath);

        // Get current year as yyyy
        var currentYear = DateTime.Now.Year;

        // If no file found, create one then return.
        var files = Directory.GetFiles(_rootPath, SearchPattern);
        if (files.Length == 0)
        {            
            var filePath = Path.Combine(_rootPath, $"Asset_{currentYear}.json");
            File.WriteAllText(filePath, JsonSerializer.Serialize(Array.Empty<Asset>()));                        
            return;
        }

        // If found more than one, get the earliest year
        var earliestYear = files
            .Select(GetYearFromFilePath)
            .MinBy(year => year);
        
        // Copy the previous year as the starting point
        for (int year = earliestYear + 1; year <= currentYear; year++)
        {
            var prevAssetPath = $"{_rootPath}\\Asset_{year - 1}.json";
            var currentAssetPath = $"{_rootPath}\\Asset_{year}.json";
            if (!File.Exists(currentAssetPath))            
                File.Copy(prevAssetPath, currentAssetPath);            
        }
    }

    public MsgResult<Asset[]> GetAssetsByYear(int year)
    {
        try
        {
            var path = GetFilePath(year);
            
            var assets = JsonSerializer
                .Deserialize<Asset[]>(File.ReadAllText(path))?
                .OrderBy(x => x.AssetType)
                .ThenBy(x => x.AssetName)
                .ThenByDescending(x => x.AssetDate)
                .ThenByDescending(x => x.LastUpdatedTime)
                .ToArray();

            return new(true, "", assets);
        }
        catch (Exception e)
        {
            // If file doesn't exist, return empty.
            return new MsgResult<Asset[]>(false, $"Error load year {year} - {e.Message}", Array.Empty<Asset>());
        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public MsgResult<Dictionary<int, Asset[]>> GetYearAssetsDict()
    {
        // If no file found, create one then return.
        var files = Directory.GetFiles(_rootPath, SearchPattern);
        if (files.Length == 0)
        {
            var emptyDict = new Dictionary<int, Asset[]>();
            return new (true, "", emptyDict);
        }

        // If found more than one file, create the dictionary
        var actualDict = files
            .Select(GetYearFromFilePath)
            .ToDictionary(y => y, y => GetAssetsByYear(y).Result);
        
        return new (true, "", actualDict);;
    }

    /// <summary>
    /// Parse the year in the file name with the pattern of "Asset_yyyy.json", such as Asset_2023.json
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns>The year in the file name.</returns>
    private static int GetYearFromFilePath(string filePath)
    {
    
        var fileInfo = new FileInfo(filePath);
        var parts = fileInfo.Name.Split('_', '.');
        return Convert.ToInt32(parts[1]);
    }
    
    private string GetFilePath(int year)
    {
        CheckRootDir();
        
        var filePath = $"{_rootPath}\\Asset_{year}.json";

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File is not found - {filePath}");

        return filePath;
    }

}
