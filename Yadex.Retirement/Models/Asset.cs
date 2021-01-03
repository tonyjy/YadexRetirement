using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Yadex.Retirement.Models
{
    public record Asset (Guid AssetId, string AssetName, decimal AssetAmount, string AssetType, DateTime AssetDate)
    {
        public DateTime LastUpdatedTime { get; set; } = DateTime.Now;
    }
}
