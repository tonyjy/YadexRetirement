namespace Yadex.Retirement.Services;

/// <summary>
/// Service to retrieve the settings for Yadex application.
/// </summary>
public interface IYadexRetirementSettingsService
{
    MsgResult<string> UpdateYadexRetirementSettings(YadexRetirementSettings settings);

    /// <summary>
    /// Get the settings. For the first time, it will generate a default setting file.
    /// </summary>
    /// <returns><see cref="YadexRetirementSettings"/></returns>
    MsgResult<YadexRetirementSettings> GetYadexRetirementSettings();
}
