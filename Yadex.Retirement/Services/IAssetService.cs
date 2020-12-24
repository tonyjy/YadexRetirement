using System;
using Yadex.Retirement.Common;
using Yadex.Retirement.Models;

namespace Yadex.Retirement.Services
{
    public interface IAssetService
    {
        MsgResult<string> AddAsset(Asset asset);
        
        MsgResult<string> UpdateAsset(Asset updatedAsset);
        
        MsgResult<string> DeleteAsset(Guid assetId);

        MsgResult<Asset[]> GetAllAssets();
    }
}