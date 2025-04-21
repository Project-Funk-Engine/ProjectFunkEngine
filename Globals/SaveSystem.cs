using System;
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
    private const string DefaultInputType = "WASD";
    private const int DefaultInputKeyboardUp = 87; //W
    private const int DefaultInputKeyboardLeft = 65; //A
    private const int DefaultInputKeyboardDown = 83; //S
    private const int DefaultInputKeyboardRight = 68; //D
    private const int DefaultInputKeyboardSecondary = 4194325; //Shift
    private const int DefaultInputKeyboardInventory = 73; //I
    private const int DefaultInputControllerUp = 3; //Y
    private const int DefaultInputControllerLeft = 2; //X
    private const int DefaultInputControllerDown = 0; //A
    private const int DefaultInputControllerRight = 1; //B
    private const int DefaultInputControllerSecondary = 10; //right bumper
    private const int DefaultInputControllerInventory = 4; //back button
    private const string DefaultLanguage = "en";
    private const bool DefaultHighCon = false;

    public enum ConfigSettings
    {
        Volume,
        InputType,
        InputKeyboardUp,
        InputKeyboardLeft,
        InputKeyboardDown,
        InputKeyboardRight,
        InputKeyboardSecondary,
        InputKeyboardInventory,
        InputControllerUp,
        InputControllerLeft,
        InputControllerDown,
        InputControllerRight,
        InputControllerSecondary,
        InputControllerInventory,
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
        UpdateConfig(ConfigSettings.InputType, DefaultInputType);
        UpdateConfig(ConfigSettings.InputKeyboardUp, DefaultInputKeyboardUp);
        UpdateConfig(ConfigSettings.InputKeyboardLeft, DefaultInputKeyboardLeft);
        UpdateConfig(ConfigSettings.InputKeyboardDown, DefaultInputKeyboardDown);
        UpdateConfig(ConfigSettings.InputKeyboardRight, DefaultInputKeyboardRight);
        UpdateConfig(ConfigSettings.InputKeyboardSecondary, DefaultInputKeyboardSecondary);
        UpdateConfig(ConfigSettings.InputKeyboardInventory, DefaultInputKeyboardInventory);
        UpdateConfig(ConfigSettings.InputControllerUp, DefaultInputControllerUp);
        UpdateConfig(ConfigSettings.InputControllerLeft, DefaultInputControllerLeft);
        UpdateConfig(ConfigSettings.InputControllerDown, DefaultInputControllerDown);
        UpdateConfig(ConfigSettings.InputControllerRight, DefaultInputControllerRight);
        UpdateConfig(ConfigSettings.InputControllerSecondary, DefaultInputControllerSecondary);
        UpdateConfig(ConfigSettings.InputControllerInventory, DefaultInputControllerInventory);
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
            case ConfigSettings.InputType:
                _curConfigData.SetValue("Options", "InputKey", value);
                break;
            case ConfigSettings.InputKeyboardUp:
                _curConfigData.SetValue("Options", "InputKeyboardUp", value);
                break;
            case ConfigSettings.InputKeyboardLeft:
                _curConfigData.SetValue("Options", "InputKeyboardLeft", value);
                break;
            case ConfigSettings.InputKeyboardDown:
                _curConfigData.SetValue("Options", "InputKeyboardDown", value);
                break;
            case ConfigSettings.InputKeyboardRight:
                _curConfigData.SetValue("Options", "InputKeyboardRight", value);
                break;
            case ConfigSettings.InputKeyboardSecondary:
                _curConfigData.SetValue("Options", "InputKeyboardSecondary", value);
                break;
            case ConfigSettings.InputKeyboardInventory:
                _curConfigData.SetValue("Options", "InputKeyboardInventory", value);
                break;
            case ConfigSettings.InputControllerUp:
                _curConfigData.SetValue("Options", "InputControllerUp", value);
                break;
            case ConfigSettings.InputControllerLeft:
                _curConfigData.SetValue("Options", "InputControllerLeft", value);
                break;
            case ConfigSettings.InputControllerDown:
                _curConfigData.SetValue("Options", "InputControllerDown", value);
                break;
            case ConfigSettings.InputControllerRight:
                _curConfigData.SetValue("Options", "InputControllerRight", value);
                break;
            case ConfigSettings.InputControllerSecondary:
                _curConfigData.SetValue("Options", "InputControllerSecondary", value);
                break;
            case ConfigSettings.InputControllerInventory:
                _curConfigData.SetValue("Options", "InputControllerInventory", value);
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
            ApplySavedInputBindings();
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
            case ConfigSettings.InputType:
                return _curConfigData.GetValue("Options", "InputKey", DefaultInputType);
            case ConfigSettings.InputKeyboardUp:
                return _curConfigData.GetValue(
                    "Options",
                    "InputKeyboardUp",
                    DefaultInputKeyboardUp
                );
            case ConfigSettings.InputKeyboardLeft:
                return _curConfigData.GetValue(
                    "Options",
                    "InputKeyboardLeft",
                    DefaultInputKeyboardLeft
                );
            case ConfigSettings.InputKeyboardDown:
                return _curConfigData.GetValue(
                    "Options",
                    "InputKeyboardDown",
                    DefaultInputKeyboardDown
                );
            case ConfigSettings.InputKeyboardRight:
                return _curConfigData.GetValue(
                    "Options",
                    "InputKeyboardRight",
                    DefaultInputKeyboardRight
                );
            case ConfigSettings.InputKeyboardSecondary:
                return _curConfigData.GetValue(
                    "Options",
                    "InputKeyboardSecondary",
                    DefaultInputKeyboardSecondary
                );
            case ConfigSettings.InputKeyboardInventory:
                return _curConfigData.GetValue(
                    "Options",
                    "InputKeyboardInventory",
                    DefaultInputKeyboardInventory
                );
            case ConfigSettings.InputControllerUp:
                return _curConfigData.GetValue(
                    "Options",
                    "InputControllerUp",
                    DefaultInputControllerUp
                );
            case ConfigSettings.InputControllerLeft:
                return _curConfigData.GetValue(
                    "Options",
                    "InputControllerLeft",
                    DefaultInputControllerLeft
                );
            case ConfigSettings.InputControllerDown:
                return _curConfigData.GetValue(
                    "Options",
                    "InputControllerDown",
                    DefaultInputControllerDown
                );
            case ConfigSettings.InputControllerRight:
                return _curConfigData.GetValue(
                    "Options",
                    "InputControllerRight",
                    DefaultInputControllerRight
                );
            case ConfigSettings.InputControllerSecondary:
                return _curConfigData.GetValue(
                    "Options",
                    "InputControllerSecondary",
                    DefaultInputControllerSecondary
                );
            case ConfigSettings.InputControllerInventory:
                return _curConfigData.GetValue(
                    "Options",
                    "InputControllerInventory",
                    DefaultInputControllerInventory
                );
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
        public ulong RngSeed { get; init; }
        public ulong RngState { get; init; }
        public int LastRoomIdx { get; init; }

        public int[] NoteIds { get; init; }
        public int[] RelicIds { get; init; }
        public int PlayerHealth { get; init; }

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

    public static void ApplySavedInputBindings()
    {
        InputMap.ActionEraseEvents("WASD_arrowUp");
        InputMap.ActionEraseEvents("WASD_arrowDown");
        InputMap.ActionEraseEvents("WASD_arrowRight");
        InputMap.ActionEraseEvents("WASD_arrowLeft");
        InputMap.ActionEraseEvents("WASD_secondaryPlacement");
        InputMap.ActionEraseEvents("WASD_inventory");
        InputMap.ActionEraseEvents("CONTROLLER_arrowUp");
        InputMap.ActionEraseEvents("CONTROLLER_arrowDown");
        InputMap.ActionEraseEvents("CONTROLLER_arrowLeft");
        InputMap.ActionEraseEvents("CONTROLLER_arrowRight");
        InputMap.ActionEraseEvents("CONTROLLER_secondaryPlacement");
        InputMap.ActionEraseEvents("CONTROLLER_inventory");

        // Keyboard bindings
        AddKeyBinding("WASD_arrowUp", GetConfigValue(ConfigSettings.InputKeyboardUp).ToString());
        AddKeyBinding(
            "WASD_arrowDown",
            GetConfigValue(ConfigSettings.InputKeyboardDown).ToString()
        );
        AddKeyBinding(
            "WASD_arrowLeft",
            GetConfigValue(ConfigSettings.InputKeyboardLeft).ToString()
        );
        AddKeyBinding(
            "WASD_arrowRight",
            GetConfigValue(ConfigSettings.InputKeyboardRight).ToString()
        );
        AddKeyBinding(
            "WASD_secondaryPlacement",
            GetConfigValue(ConfigSettings.InputKeyboardSecondary).ToString()
        );
        AddKeyBinding(
            "WASD_inventory",
            GetConfigValue(ConfigSettings.InputKeyboardInventory).ToString()
        );

        // Controller bindings
        AddJoypadBinding(
            "CONTROLLER_arrowUp",
            GetConfigValue(ConfigSettings.InputControllerUp).ToString()
        );
        AddJoypadBinding(
            "CONTROLLER_arrowDown",
            GetConfigValue(ConfigSettings.InputControllerDown).ToString()
        );
        AddJoypadBinding(
            "CONTROLLER_arrowLeft",
            GetConfigValue(ConfigSettings.InputControllerLeft).ToString()
        );
        AddJoypadBinding(
            "CONTROLLER_arrowRight",
            GetConfigValue(ConfigSettings.InputControllerRight).ToString()
        );
        AddJoypadBinding(
            "CONTROLLER_secondaryPlacement",
            GetConfigValue(ConfigSettings.InputControllerSecondary).ToString()
        );
        AddJoypadBinding(
            "CONTROLLER_inventory",
            GetConfigValue(ConfigSettings.InputControllerInventory).ToString()
        );
    }

    private static void AddKeyBinding(string action, string keyString)
    {
        Key key = (Key)Enum.Parse(typeof(Key), keyString, ignoreCase: true);
        InputEventKey inputEvent = new InputEventKey { PhysicalKeycode = key };
        InputMap.ActionAddEvent(action, inputEvent);
    }

    private static void AddJoypadBinding(string action, string buttonString)
    {
        if (Enum.TryParse<JoyButton>(buttonString, true, out JoyButton button))
        {
            InputEventJoypadButton inputEvent = new InputEventJoypadButton { ButtonIndex = button };
            InputMap.ActionAddEvent(action, inputEvent);
        }
        else
        {
            GD.PushWarning($"Could not parse joypad button: {buttonString}");
        }
    }

    #endregion
}
