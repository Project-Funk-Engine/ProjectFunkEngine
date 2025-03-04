using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;
using FileAccess = Godot.FileAccess;

// TODO: implement saving

public static class SaveSystem
{
    public static string UserConfigPath = "user://Options.cfg";
    private static ConfigFile _curConfigData;

    public enum ConfigSettings
    {
        Volume,
        InputKey,
        LanguageKey,
    }

    private static void InitConfig()
    {
        _curConfigData = new ConfigFile();
        UpdateConfig(ConfigSettings.Volume, 80f);
        UpdateConfig(ConfigSettings.InputKey, "WASD");
        UpdateConfig(ConfigSettings.LanguageKey, "en");
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
    private static ConfigFile LoadConfigData()
    {
        _curConfigData = new ConfigFile();
        if (_curConfigData.Load(UserConfigPath) == Error.Ok)
            return _curConfigData;
        GD.Print("No config could be found, creating a new one.");
        InitConfig();
        SaveConfig();
        return _curConfigData;
    }

    public static Variant GetConfigValue(ConfigSettings setting)
    {
        AssertConfigFile();
        switch (setting)
        {
            case ConfigSettings.Volume:
                return _curConfigData.GetValue("Options", "Volume");
            case ConfigSettings.InputKey:
                return _curConfigData.GetValue("Options", "InputKey");
            case ConfigSettings.LanguageKey:
                return _curConfigData.GetValue("Options", "LanguageKey");
            default:
                GD.PushError(
                    "SaveSystem.GetConfigValue: Invalid config setting passed. " + setting
                );
                return float.MinValue;
        }
    }
}
