using System;

namespace Yadex.Retirement.Models
{
    public record YadexRetirementSettings (string AssetRootFolder)
    {
        public int BirthYear { get; set; } = DateTime.Now.Year - 45;
        
        public int RetirementAge { get; set; } = 55;

        public decimal RetirementIncome { get; set; } = 75000m;

        public decimal AppreciationRate { get; set; } = .04m;

        public decimal SocialSecurityIncome { get; set; }
        
        public decimal PensionIncome { get; set; }
    }
}