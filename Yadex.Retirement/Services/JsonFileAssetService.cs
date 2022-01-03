using System.IO;
using System.Text.Json;

namespace Yadex.Retirement.Services;

/// <summary>
///     Json file implementation for <see cref="IAssetService" />.
/// </summary>
public class JsonFileAssetService : IAssetService
{
    private const string AddAssetAction = "AddAssetAction";
    private const string UpdateAssetAction = "UpdateAssetAction";
    private const string DeleteAssetAction = "DeleteAssetAction";

    // Json file's root folder
    private readonly string _rootPath;

    /// <summary>
    ///     Constructor with rootPath
    /// </summary>
    /// <param name="rootPath"></param>
    public JsonFileAssetService(string rootPath)
    {
        _rootPath = rootPath;
    }

    private string CurrentFilePath
    {
        get
        {
            CheckRootDir();

            // Default file name is Asset.json
            var filePath = Path.Combine(_rootPath, "Asset.json");

            // if this is the first time, we need to create an empty file
            if (!File.Exists(filePath))
                File.WriteAllText(filePath, JsonSerializer.Serialize(Array.Empty<Asset>()));

            return filePath;
        }
    }

    private string AuditFilePath
    {
        get
        {
            CheckRootDir();

            // Default file name is Asset.json
            var filePath = Path.Combine(_rootPath, $"Asset_Audit_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json");

            // if this is the first time, we need to create an empty file
            if (!File.Exists(filePath))
                File.WriteAllText(filePath, JsonSerializer.Serialize(Array.Empty<Asset>()));

            return filePath;
        }
    }

    /// <summary>
    ///     Add an asset <see cref="Asset" />.
    /// </summary>
    /// <param name="asset"></param>
    /// <returns></returns>
    public MsgResult<string> AddAsset(Asset asset)
    {
        try
        {
            var (succeeded, errorMessage, oldAssets) = GetAllAssets();
            if (!succeeded)
                throw new Exception($"Get all assets failed. {errorMessage}");


            // validate 
            var existing = oldAssets.SingleOrDefault(x => x.AssetId == asset.AssetId);
            if (existing != null)
                throw new Exception($"Record has been found for {asset.AssetId.ToString()}");

            // act
            SaveAssets(oldAssets.ToList(), null, asset, AddAssetAction);

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
    /// <param name="updatedAsset"></param>
    /// <returns></returns>
    public MsgResult<string> UpdateAsset(Asset updatedAsset)
    {
        try
        {
            var (succeeded, errorMessage, oldAssets) = GetAllAssets();
            if (!succeeded)
                throw new Exception($"Get all assets failed. {errorMessage}");

            // validate 
            var asset = oldAssets.SingleOrDefault(x => x.AssetId == updatedAsset.AssetId);
            if (asset == null)
                throw new Exception($"Record cannot be found for {updatedAsset.AssetId}");

            // act
            SaveAssets(oldAssets.ToList(), asset, updatedAsset, UpdateAssetAction);

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
    /// <param name="assetId"></param>
    /// <returns></returns>
    public MsgResult<string> DeleteAsset(Guid assetId)
    {
        try
        {
            var (succeeded, errorMessage, oldAssets) = GetAllAssets();
            if (!succeeded)
                throw new Exception($"Get all assets failed. {errorMessage}");

            // validate 
            var asset = oldAssets.SingleOrDefault(x => x.AssetId == assetId);
            if (asset == null)
                throw new Exception($"Record cannot be found for {assetId.ToString()}");

            // act
            SaveAssets(oldAssets.ToList(), asset, null, DeleteAssetAction);

            return new MsgResult<string>();
        }
        catch (Exception e)
        {
            return new MsgResult<string>($"Error happened to delete. \n{e.Message}\n{e.StackTrace}");
        }
    }

    /// <summary>
    ///     Get all assets from Json file.
    /// </summary>
    /// <returns></returns>
    public MsgResult<Asset[]> GetAllAssets()
    {
        try
        {
            var assets = JsonSerializer.Deserialize<Asset[]>(File.ReadAllText(CurrentFilePath))?
                .OrderBy(x => x.AssetType)
                .ThenBy(x => x.AssetName)
                .ThenByDescending(x => x.AssetDate)
                .ThenByDescending(x => x.LastUpdatedTime)
                .ToArray();

            return new(true, "", assets);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void SaveAssets(List<Asset> allAssets, Asset oldAsset, Asset newAsset, string actionName)
    {
        var oldAssets = allAssets.ToArray();

        var lastUpdatedTime = DateTime.Now;
        switch (actionName)
        {
            case AddAssetAction:
                // set timestamp
                newAsset.LastUpdatedTime = lastUpdatedTime;
                allAssets.Add(newAsset);
                break;
            case UpdateAssetAction:
                allAssets.Remove(oldAsset);

                // set timestamp
                newAsset.LastUpdatedTime = lastUpdatedTime;
                allAssets.Add(newAsset);

                // check if AssetName has been changed
                if (oldAsset.AssetName != newAsset.AssetName)
                {
                    allAssets
                        .Where(x => x.AssetName == oldAsset.AssetName)
                        .ToList()
                        .ForEach(asset =>
                        {
                            var updatedAsset = asset with
                            {
                                AssetName = newAsset.AssetName,
                                LastUpdatedTime = lastUpdatedTime
                            };
                            allAssets.Remove(asset);
                            allAssets.Add(updatedAsset);
                        });
                }

                break;
            case DeleteAssetAction:
                allAssets.Remove(oldAsset);
                break;
            default:
                throw new Exception($"Action is not supported - {actionName}");
        }

        var newAssets =
            allAssets
                .OrderBy(x => x.AssetType)
                .ThenBy(x => x.AssetName)
                .ThenByDescending(x => x.AssetDate)
                .ThenByDescending(x => x.LastUpdatedTime)
                .ToArray();

        // Backup the changes 
        var assetAudit = new AssetAudit(actionName, oldAsset, newAsset, oldAssets, newAssets);
        File.WriteAllText(AuditFilePath, JsonSerializer.Serialize(assetAudit));

        // Save to the current file path
        File.WriteAllText(CurrentFilePath, JsonSerializer.Serialize(newAssets));
    }

    /// <summary>
    ///     Check if root folder exists
    /// </summary>
    /// <exception cref="FileNotFoundException"></exception>
    private void CheckRootDir()
    {
        if (!Directory.Exists(_rootPath))
            throw new FileNotFoundException($"Directory is not found - {_rootPath}");
    }
}
