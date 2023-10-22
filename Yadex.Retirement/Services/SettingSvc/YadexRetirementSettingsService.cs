using System.IO;
using System.Text.Json;

namespace Yadex.Retirement.Services.SettingSvc;

/// <summary>
///     Json file implementation for <see cref="IYadexRetirementSettingsService" />.
/// </summary>
public class YadexRetirementSettingsService : IYadexRetirementSettingsService
{
    private const string FolderName = "Yadex";
    private const string FileName = "YadexRetirementSettings.json";
    public string AppLocalFolder => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    public string FolderPath => Path.Combine(AppLocalFolder, FolderName);

    private string CurrentFilePath => Path.Combine(FolderPath, FileName);


    public MsgResult<string> UpdateYadexRetirementSettings(YadexRetirementSettings settings)
    {
        try
        {
            // Save to the current file path
            File.WriteAllText(CurrentFilePath, JsonSerializer.Serialize(settings));

            return new MsgResult<string>();
        }
        catch (Exception e)
        {
            return new MsgResult<string>($"Error happened to delete. \n{e.Message}\n{e.StackTrace}");
        }
    }

    public MsgResult<YadexRetirementSettings> GetYadexRetirementSettings()
    {
        try
        {
            // if this is the first time, we need to create folder
            if (!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);

            // if this is the first time, we need to create an empty file
            if (!File.Exists(CurrentFilePath))
                File.WriteAllText(CurrentFilePath,
                    JsonSerializer.Serialize(new YadexRetirementSettings(FolderPath)));

            // Save to the current file path
            var settings = JsonSerializer.Deserialize<YadexRetirementSettings>(File.ReadAllText(CurrentFilePath));

            return new MsgResult<YadexRetirementSettings>(true, string.Empty, settings);
        }
        catch (Exception e)
        {
            return new MsgResult<YadexRetirementSettings>(
                $"Error happened to read file {CurrentFilePath}. {e.Message}");
        }
    }
}
