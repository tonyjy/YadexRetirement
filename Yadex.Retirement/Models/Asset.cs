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

    public class AssetTypes
    {
        public static readonly string Cash = "Cash";
        public static readonly string Fixed = "Fixed";
        public static readonly string Retirement401K = "Retirement 401K USA";
        public static readonly string RetirementRrsp = "Retirement RRSP CAN";
        public static readonly string RetirementPension = "Retirement Pension";
    }
}
