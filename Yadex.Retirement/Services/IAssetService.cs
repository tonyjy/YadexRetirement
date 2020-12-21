using System;
using Yadex.Retirement.Models;

namespace Yadex.Retirement.Services
{
    public interface IAssetService
    {
        MsgResult AddAsset(Asset asset);
        
        MsgResult UpdateAsset(Asset updatedAsset);
        
        MsgResult DeleteAsset(Guid assetId);

        Asset[] GetAllAssets();
    }
}