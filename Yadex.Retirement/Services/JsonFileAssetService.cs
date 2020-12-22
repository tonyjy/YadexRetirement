using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Yadex.Retirement.Common;
using Yadex.Retirement.Models;

namespace Yadex.Retirement.Services
{
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
        public MsgResult AddAsset(Asset asset)
        {
            try
            {
                var oldAssets = GetAllAssets().ToList();

                // validate 
                var oldAsset = oldAssets.SingleOrDefault(x => x.AssetId == asset.AssetId);
                if (oldAsset != null)
                    throw new Exception($"Record has been found for {asset.AssetId.ToString()}");

                // act
                SaveAssets(oldAssets, null, asset, AddAssetAction);

                return new MsgResult();
            }
            catch (Exception e)
            {
                return new MsgResult($"Error happened while add asset. \n{e.Message}\n{e.StackTrace}");
            }
        }


        /// <summary>
        ///     Update an asset <see cref="Asset" />.
        /// </summary>
        /// <param name="updatedAsset"></param>
        /// <returns></returns>
        public MsgResult UpdateAsset(Asset updatedAsset)
        {
            try
            {
                var oldAssets = GetAllAssets().ToList();

                // validate 
                var oldAsset = oldAssets.SingleOrDefault(x => x.AssetId == updatedAsset.AssetId);
                if (oldAsset == null)
                    throw new Exception($"Record cannot be found for {updatedAsset.AssetId.ToString()}");

                // act
                SaveAssets(oldAssets, oldAsset, updatedAsset, UpdateAssetAction);

                return new MsgResult();
            }
            catch (Exception e)
            {
                return new MsgResult($"Error happened to update asset. \n{e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>
        ///     Delete an asset <see cref="Asset" />.
        /// </summary>
        /// <param name="assetId"></param>
        /// <returns></returns>
        public MsgResult DeleteAsset(Guid assetId)
        {
            try
            {
                var oldAssets = GetAllAssets().ToList();

                // validate 
                var oldAsset = oldAssets.SingleOrDefault(x => x.AssetId == assetId);
                if (oldAsset == null)
                    throw new Exception($"Record cannot be found for {assetId.ToString()}");

                // act
                SaveAssets(oldAssets, oldAsset, null, DeleteAssetAction);

                return new MsgResult();
            }
            catch (Exception e)
            {
                return new MsgResult($"Error happened to delete. \n{e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>
        ///     Get all assets from Json file.
        /// </summary>
        /// <returns></returns>
        public Asset[] GetAllAssets()
        {
            return JsonSerializer.Deserialize<Asset[]>(File.ReadAllText(CurrentFilePath)) ?
                .OrderBy(x => x.AssetType)
                .ThenBy(x => x.AssetName)
                .ThenByDescending(x => x.AssetDate)
                .ThenByDescending(x => x.LastUpdatedTime)
                .ToArray();
        }

        private void SaveAssets(List<Asset> allAssets, Asset oldAsset, Asset newAsset, string actionName)
        {
            var oldAssets = allAssets.ToArray();

            switch (actionName)
            {
                case AddAssetAction:
                    allAssets.Add(newAsset);
                    break;
                case UpdateAssetAction:
                    allAssets.Remove(oldAsset);
                    allAssets.Add(newAsset);
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
}