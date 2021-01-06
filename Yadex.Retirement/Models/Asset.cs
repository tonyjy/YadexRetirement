using System;

namespace Yadex.Retirement.Models
{
    public record Asset (Guid AssetId, string AssetName, decimal AssetAmount, string AssetType, DateTime AssetDate)
    {
        public DateTime LastUpdatedTime { get; set; } = DateTime.Now;
    }
}
