namespace Yadex.Retirement.Services;

public interface IYadexRetirementSettingsService
{
    MsgResult<string> UpdateYadexRetirementSettings(YadexRetirementSettings settings);

    MsgResult<YadexRetirementSettings> GetYadexRetirementSettings();
}
