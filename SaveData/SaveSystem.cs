using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Godot;
using FileAccess = Godot.FileAccess;

// TODO: implement saving

public static class SaveSystem
{
    public static string UserConfigPath = "user://Options.cfg";
    private static ConfigFile _curConfigData;

    private const float DefaultVolume = 80f;
    private const string DefaultInput = "WASD";
    private const string DefaultLanguage = "en";
    private const bool DefaultHighCon = false;

    public enum ConfigSettings
    {
        Volume,
        InputKey,
        LanguageKey,
        HighContrast,
    }

    private static void InitConfig()
    {
        _curConfigData = new ConfigFile();
        UpdateConfig(ConfigSettings.Volume, DefaultVolume);
        UpdateConfig(ConfigSettings.InputKey, DefaultInput);
        UpdateConfig(ConfigSettings.LanguageKey, DefaultLanguage);
        UpdateConfig(ConfigSettings.HighContrast, DefaultHighCon);
    }

    private static void SaveConfig()
    {
        AssertConfigFile();
        _curConfigData.Save(UserConfigPath);
    }

    public static void UpdateConfig(ConfigSettings setting, Variant value)
    {
        AssertConfigFile();
        switch (setting)
        {
            case ConfigSettings.Volume:
                _curConfigData.SetValue("Options", "Volume", value);
                break;
            case ConfigSettings.InputKey:
                _curConfigData.SetValue("Options", "InputKey", value);
                break;
            case ConfigSettings.LanguageKey:
                _curConfigData.SetValue("Options", "LanguageKey", value);
                break;
            case ConfigSettings.HighContrast:
                _curConfigData.SetValue("Options", "HighContrast", value);
                break;
        }
        SaveConfig();
    }

    public static void AssertConfigFile()
    {
        if (_curConfigData == null)
        {
            LoadConfigData();
        }
    }

    // This method loads the entire save data
    private static void LoadConfigData()
    {
        _curConfigData = new ConfigFile();
        VerifyConfig();
        if (_curConfigData.Load(UserConfigPath) == Error.Ok)
            return;
        GD.Print("No config could be found, creating a new one.");
        InitConfig();
        SaveConfig();
    }

    //Really naive approach to verifying config integrity, could I have just changed back to JSON? yes. But I'm a real programmer.
    //In theory ConfigFiles should be more stable across any version changes.
    private static void VerifyConfig()
    {
        if (!FileAccess.FileExists(UserConfigPath))
            return;
        string[] sus = new[]
        {
            "init",
            "object",
            "script",
            "source",
            "extends",
            "RefCounted",
            "sus",
        };
        FileAccess file = FileAccess.Open(UserConfigPath, FileAccess.ModeFlags.Read);
        if (!sus.Any(s => file.GetAsText().Contains(s)))
            return;
        file.Close();
        InitConfig();
    }

    public static Variant GetConfigValue(ConfigSettings setting)
    {
        AssertConfigFile();
        switch (setting)
        {
            case ConfigSettings.Volume:
                return _curConfigData.GetValue("Options", "Volume", DefaultVolume);
            case ConfigSettings.InputKey:
                return _curConfigData.GetValue("Options", "InputKey", DefaultInput);
            case ConfigSettings.LanguageKey:
                return _curConfigData.GetValue("Options", "LanguageKey", DefaultLanguage);
            case ConfigSettings.HighContrast:
                return _curConfigData.GetValue("Options", "HighContrast", DefaultHighCon);
            default:
                GD.PushError(
                    "SaveSystem.GetConfigValue: Invalid config setting passed. " + setting
                );
                return float.MinValue;
        }
    }
}
