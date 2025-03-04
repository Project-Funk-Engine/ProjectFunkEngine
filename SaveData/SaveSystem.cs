using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;
using FileAccess = Godot.FileAccess;

// TODO: implement saving

public static class SaveSystem
{
    public static string UserConfigPath = "user://Options.cfg";
    private static ConfigData _curConfigData = null;

    private static void SaveConfig()
    {
        _curConfigData = GetConfigData();
        string json = JsonSerializer.Serialize(_curConfigData);
        GD.Print(json);
        FileAccess file = FileAccess.Open(UserConfigPath, FileAccess.ModeFlags.Write);
        file.StoreLine(json);
        file.Close();
    }

    public static void UpdateConfig(string setting, Variant value)
    {
        _curConfigData = GetConfigData();
        switch (setting)
        {
            case (nameof(ConfigData.Volume)):
                _curConfigData.Volume = (float)value;
                break;
            case (nameof(ConfigData.InputKey)):
                _curConfigData.InputKey = (string)value;
                break;
            case nameof(ConfigData.LanguageKey):
                _curConfigData.LanguageKey = (string)value;
                break;
        }
        SaveConfig();
    }

    public static ConfigData GetConfigData()
    {
        return _curConfigData ?? LoadConfigData();
    }

    // This method loads the entire save data
    private static ConfigData LoadConfigData()
    {
        if (!FileAccess.FileExists(UserConfigPath))
        {
            GD.Print("No config could be found, creating a new one.");
            _curConfigData = new ConfigData();
            SaveConfig();
            return _curConfigData;
        }

        string json = FileAccess.Open(UserConfigPath, FileAccess.ModeFlags.Read).GetAsText();
        ConfigData data = JsonSerializer.Deserialize<ConfigData>(json);
        return data;
    }
}

public class ConfigData
{
    public float Volume { get; set; }
    public string InputKey { get; set; }
    public string LanguageKey { get; set; }

    public ConfigData(float volume = 80, string inputKey = "WASD", string languageKey = "en")
    {
        Volume = volume;
        InputKey = inputKey;
        LanguageKey = languageKey;
    }
}
