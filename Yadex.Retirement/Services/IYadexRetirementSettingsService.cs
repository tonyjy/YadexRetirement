using System;
using Yadex.Retirement.Common;
using Yadex.Retirement.Models;

namespace Yadex.Retirement.Services
{
    public interface IYadexRetirementSettingsService
    {
        MsgResult<string> UpdateYadexRetirementSettings(YadexRetirementSettings settings);

        MsgResult<YadexRetirementSettings> GetYadexRetirementSettings();
    }
}