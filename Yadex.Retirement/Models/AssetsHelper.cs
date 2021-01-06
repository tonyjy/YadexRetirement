using System.Linq;

namespace Yadex.Retirement.Models
{
    public class AssetsHelper
    {
        public static Asset[] GetAssetsForYear(Asset[] assets, int year)
        {
            var filteredList = assets.Where(x => x.AssetDate.Year <= year);

            return filteredList
                .GroupBy(x => x.AssetName)
                .Select(grouping => grouping.First())
                .ToArray();
        }

        public static string GetTotalWithChange(decimal curVal, decimal preVal)
        {
            var changeYtd = preVal == 0 ? 0 : curVal - preVal;
            var percentYtd = preVal == 0 ? 0 : changeYtd / preVal;
            var sign = changeYtd > 0 ? "+" : "";
            var changed = preVal == 0 ? "(0k 0.0%)" : $"({sign}{changeYtd / 1000:N0}k {percentYtd:P1})";
            
            return $"{curVal / 1000000:N3}m {changed}";
        }
    }
}