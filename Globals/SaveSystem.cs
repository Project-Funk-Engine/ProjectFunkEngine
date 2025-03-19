using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Godot;
using FileAccess = Godot.FileAccess;

/**
 * <summary>SaveSystem: Manages FileI/O of configs and save files.</summary>
 */
public static class SaveSystem
{
    #region Config
    private const string UserConfigPath = "user://Options.cfg";
    private static ConfigFile _curConfigData;

    private const float DefaultVolume = 1f;
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

    /**
     * <remarks>Overwrites any file at <c>UserConfigPath</c></remarks>
     */
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
            default:
                GD.PushError("SaveSystem.UpdateConfig: Invalid config setting passed. " + setting);
                break;
        }
        SaveConfig();
    }

    /**<remarks>Either returns, loads current config, or if no config found inits one.</remarks>
    */
    private static void AssertConfigFile()
    {
        if (_curConfigData == null)
        {
            LoadConfigData();
        }
    }

    /**Really naive approach to verifying config integrity, could I have just changed back to JSON? yes. But I'm a real programmer.
    In theory ConfigFiles should be more stable across any version changes.*/
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

    private static void LoadConfigData()
    {
        _curConfigData = new ConfigFile();
        VerifyConfig();
        if (_curConfigData.Load(UserConfigPath) == Error.Ok)
            return;
        GD.PushWarning("Safe. No config could be found, creating a new one.");
        InitConfig();
        SaveConfig();
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
                GD.PushError("Invalid config setting passed. " + setting);
                return float.MinValue;
        }
    }
    #endregion

    #region Save

    private const string UserSavePath = "user://MidnighRiff.save";

    public class SaveFile
    {
        public ulong RngSeed { get; set; }
        public ulong RngState { get; set; }
        public int LastRoomIdx { get; set; }

        public int[] NoteIds { get; set; }
        public int[] RelicIds { get; set; }
        public int PlayerHealth { get; set; }

        public SaveFile(
            ulong rngSeed,
            ulong rngState,
            int lastRoomIdx,
            int[] noteIds,
            int[] relicIds,
            int playerHealth
        )
        {
            RngSeed = rngSeed;
            RngState = rngState;
            LastRoomIdx = lastRoomIdx;
            NoteIds = noteIds;
            RelicIds = relicIds;
            PlayerHealth = playerHealth;
        }
    }

    public static void SaveGame()
    {
        int[] relicIds = StageProducer.PlayerStats.CurRelics.Select(r => r.Id).ToArray();
        int[] noteIds = StageProducer.PlayerStats.CurNotes.Select(r => r.Id).ToArray();
        SaveFile sv = new SaveFile(
            StageProducer.GlobalRng.Seed,
            StageProducer.GlobalRng.State,
            StageProducer.CurRoom,
            noteIds,
            relicIds,
            StageProducer.PlayerStats.CurrentHealth
        );
        string json = JsonSerializer.Serialize(sv);

        FileAccess file = FileAccess.Open(UserSavePath, FileAccess.ModeFlags.Write);

        file.StoreLine(json);
        file.Close();
    }

    /**
     * <remarks>Returns null if invalid save or save 404's.</remarks>
     */
    public static SaveFile LoadGame()
    {
        if (!FileAccess.FileExists(UserSavePath))
            return null;
        FileAccess file = FileAccess.Open(UserSavePath, FileAccess.ModeFlags.Read);
        string json = file.GetAsText();

        file.Close();
        SaveFile sv;
        try
        {
            sv = JsonSerializer.Deserialize<SaveFile>(json);
        }
        catch (JsonException)
        {
            GD.PushWarning("Cannot deserialize save file, returning null.");
            return null;
        }
        return sv;
    }

    public static void ClearSave()
    {
        DirAccess.RemoveAbsolute(UserSavePath);
    }

    #endregion
}
