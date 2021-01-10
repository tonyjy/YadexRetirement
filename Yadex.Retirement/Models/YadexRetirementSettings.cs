using System;

namespace Yadex.Retirement.Models
{
    public record YadexRetirementSettings (string AssetRootFolder)
    {
        /// <summary>
        ///  This is 4 digits birth year.
        /// </summary>
        public int BirthYear { get; set; } = DateTime.Now.Year - 45;
        
        /// <summary>
        /// This is planned retirement age, e.g. 55
        /// </summary>
        public int RetirementAge { get; set; } = 55;
        
        /// <summary>
        /// This is planned retirement income total (annually)
        /// </summary>
        public decimal RetirementIncome { get; set; } = 75000m;

        /// <summary>
        /// This is estimated income from SocialSecurity (annually)
        /// </summary>
        public decimal SocialSecurityIncome { get; set; }

        /// <summary>
        /// This is estimated income from Pension (annually)
        /// </summary>
        public decimal PensionIncome { get; set; }

        /// <summary>
        /// This is one time reduction for the estimate model to adjust the model
        /// </summary>
        public decimal RiskFactor { get; set; }
        
        /// <summary>
        /// This is the annual ROI rate for the estimate model
        /// </summary>
        public decimal InvestmentReturnRate { get; set; } = .04m;

        /// <summary>
        /// This is the retirement income adjustment rate for the estimate model
        /// </summary>
        public decimal RetirementIncomeAdjustmentRate { get; set; } = .01m;

        /// <summary>
        /// This is for 401K increase for transitions years
        /// </summary>
        public decimal TransitionYear401KSaving { get; set; }

    }
}