using System;
using System.Collections.Generic;
using System.Globalization;
using FunkEngine;
using Godot;

public partial class ControlSettings : Node2D, IFocusableMenu
{ //TODO: Add messages when an invalid key is attempted to be set.
    public static readonly string LoadPath = "res://Scenes/UI/Remapping/Remap.tscn";

    public IFocusableMenu Prev { get; set; }
    private Control _focused;

    [Export]
    private Button _closeButton;

    [Export]
    private Panel _remapPopup;

    [Export]
    private Label _remapLabel;

    [Export]
    private Timer _remapTimer;

    private static readonly string IconPath = "res://Scenes/UI/Remapping/Assets/";

    [Export]
    private TabContainer _remapTabs;
    private Key _tempKeyboardKey;
    private JoyButton _tempJoyButton;
    private string _chosenKey = "";

    //These just don't play well with inputs
    private readonly HashSet<Key> _invalidKeys = new HashSet<Key>
    {
        Key.Ctrl,
        Key.Meta,
        Key.Alt,
        Key.Insert,
        Key.Home,
        Key.End,
        Key.Delete,
        Key.Pagedown,
        Key.Pageup,
        Key.Print,
        Key.Scrolllock,
        Key.Pause,
        Key.Escape,
        Key.Numlock,
    };

    #region String Definition Spam
    //The below arrays should be aligned, e.g. all things representing left arrow should be at 0
    private const int InputOptions = 6; //1 for each button on an input tab.
    private readonly Sprite2D[] _inputSprites = new Sprite2D[InputOptions * 2];

    private readonly string[] _inputNodeNames =
    [
        "LeftArrow",
        "RightArrow",
        "UpArrow",
        "DownArrow",
        "SecondaryPlacement",
        "Inventory",
    ];

    private readonly string[] _inputSuffixes =
    [
        "_arrowLeft",
        "_arrowRight",
        "_arrowUp",
        "_arrowDown",
        "_secondaryPlacement",
        "_inventory",
    ];

    private readonly string[] _inputMapNames =
    [
        "WASD_arrowLeft",
        "WASD_arrowRight",
        "WASD_arrowUp",
        "WASD_arrowDown",
        "WASD_secondaryPlacement",
        "WASD_inventory",
        "CONTROLLER_arrowLeft",
        "CONTROLLER_arrowRight",
        "CONTROLLER_arrowUp",
        "CONTROLLER_arrowDown",
        "CONTROLLER_secondaryPlacement",
        "CONTROLLER_inventory",
        "Pause",
    ];

    private const string KeyboardPrefix = "WASD";
    private const string JoyPrefix = "CONTROLLER";
    private const string KeyboardTabPath = "Panel/TabContainer/CONTROLS_KEYBOARD/";
    private const string JoyTabPath = "Panel/TabContainer/CONTROLS_CONTROLLER/";
    private const string ButtonNameSuffix = "Button";
    private const string SpriteNameSuffix = "Key";
    #endregion

    #region Initialization
    //To think about: Setting things in export arrays.
    public override void _Ready()
    {
        //Sets up buttons in scene, for each button, sets its pressed event
        for (int i = 0; i < _inputNodeNames.Length; i++)
        {
            string keyboardPath = KeyboardTabPath + _inputNodeNames[i] + ButtonNameSuffix;
            string controllerPath = JoyTabPath + _inputNodeNames[i] + ButtonNameSuffix;
            var i1 = i; //Lambda functions capture changes to the value of i, use extra vars
            GetNode<Button>(keyboardPath).Pressed += () => OnRemapButtonPressed(_inputSuffixes[i1]);
            var i2 = i;
            GetNode<Button>(controllerPath).Pressed += () =>
                OnRemapButtonPressed(_inputSuffixes[i2]);
        }

        _remapTabs.CurrentTab =
            SaveSystem.GetConfigValue(SaveSystem.ConfigSettings.InputType).ToString()
            == KeyboardPrefix
                ? 0
                : 1;

        _remapTimer.Timeout += OnTimerEnd;
        _remapTabs.TabChanged += (_) => ChangeInputType();
        _closeButton.Pressed += ReturnToPrev;

        InitInputSprites();
    }

    private void InitInputSprites()
    {
        for (int i = 0; i < InputOptions; i++)
        {
            var path = KeyboardTabPath + _inputNodeNames[i] + SpriteNameSuffix;
            var sprite = GetNode<Sprite2D>(path);
            _inputSprites[i] = sprite;

            var jPath = JoyTabPath + _inputNodeNames[i] + SpriteNameSuffix;
            var jSprite = GetNode<Sprite2D>(jPath);
            _inputSprites[i + InputOptions] = jSprite;
        }

        UpdateKeySprites();
    }
    #endregion

    #region Focus and Menus
    public override void _Process(double delta)
    {
        if (_remapPopup.Visible)
            _remapLabel.Text = ((int)_remapTimer.TimeLeft + 1).ToString();
    }

    public void ResumeFocus()
    {
        ProcessMode = ProcessModeEnum.Inherit;
    }

    public void PauseFocus()
    {
        ProcessMode = ProcessModeEnum.Disabled;
    }

    public void OpenMenu(IFocusableMenu prev)
    {
        Prev = prev;
        Prev.PauseFocus();

        _remapTabs.GetTabControl(_remapTabs.CurrentTab).GetChild<Control>(0).GrabFocus();
    }

    public void ReturnToPrev()
    {
        Prev.ResumeFocus();
        QueueFree();
    }

    #endregion

    /// <summary>
    /// Called from changing tabs, sets correct input type based on selected tab.
    /// </summary>
    private void ChangeInputType()
    {
        SaveSystem.UpdateConfig(
            SaveSystem.ConfigSettings.InputType,
            _remapTabs.CurrentTab == 0 ? KeyboardPrefix : JoyPrefix
        );
    }

    /// <summary>
    /// Updates all the key sprites to the correct sprite, based on what input is set.
    /// </summary>
    private void UpdateKeySprites()
    {
        for (int i = 0; i < InputOptions * 2; i++)
        {
            var events = InputMap.ActionGetEvents(_inputMapNames[i]);
            if (events.Count <= 0)
                continue;
            string textureName = events[0].AsText();

            // Clean up the texture name
            if (_inputMapNames[i].StartsWith(KeyboardPrefix))
                textureName = CleanKeyboardText(textureName);
            else
                textureName = textureName.Replace("/", "");

            GD.Print(textureName);
            _inputSprites[i].Texture = GD.Load<Texture2D>($"{IconPath}{textureName}.png");
        }
    }

    /// <summary>
    /// Should exclusively be called from pressing a remap button.
    /// </summary>
    /// <param name="keyName">The input suffix of the button.</param>
    private void OnRemapButtonPressed(string keyName)
    {
        _chosenKey = keyName;
        StartInputCountdown();
    }

    private const int TimeForInput = 5;

    /// <summary>
    /// Start the input countdown window.
    /// </summary>
    private void StartInputCountdown()
    {
        _remapLabel.Text = TimeForInput.ToString();
        _remapTimer.Start(TimeForInput);

        _tempJoyButton = JoyButton.Invalid;
        _tempKeyboardKey = Key.None;
        _remapPopup.Visible = true;
    }

    private void OnTimerEnd()
    {
        _remapPopup.Visible = false;
    }

    public override void _Input(InputEvent @event)
    {
        if (_remapPopup.Visible)
        {
            if (@event.IsActionPressed("ui_cancel"))
            {
                _remapTimer.Stop();
                OnTimerEnd();
                GetViewport().SetInputAsHandled();
                return;
            }
            if (
                (@event is InputEventKey || @event is InputEventJoypadButton)
                && IsUniqueKey(@event.AsText())
            )
            {
                HandleRemapInput(@event);
                GetViewport().SetInputAsHandled();
            }
            else
            {
                // when popup is visible we ignore everything except valid remaps
                GetViewport().SetInputAsHandled();
            }
        }
        else if (@event.IsActionPressed("ui_cancel"))
        {
            ReturnToPrev();
            GetViewport().SetInputAsHandled();
        }
    }

    /// <summary>
    /// Parses if the input is a valid button to set, and whether it is a controller input or keyboard.
    /// </summary>
    /// <param name="event">The recent input event.</param>
    private void HandleRemapInput(InputEvent @event)
    {
        bool isKeyboard = _remapTabs.CurrentTab == 0;

        switch (isKeyboard)
        {
            case true when @event is InputEventKey keyEvent:
            {
                if (_invalidKeys.Contains(keyEvent.Keycode))
                    return;

                string action = KeyboardPrefix + _chosenKey;
                InputMap.ActionEraseEvents(action);
                InputMap.ActionAddEvent(action, keyEvent);

                SaveKeyInput(_chosenKey, keyEvent);
                break;
            }
            case false when @event is InputEventJoypadButton joyEvent:
            {
                string action = JoyPrefix + _chosenKey;
                InputMap.ActionEraseEvents(action);
                InputMap.ActionAddEvent(action, joyEvent);

                SaveKeyInput(_chosenKey, joyEvent);
                break;
            }
            default:
                return;
        }

        UpdateKeySprites();
        _remapPopup.Visible = false;
    }

    /// <summary>
    /// Dictionary of button input suffix and the config settings that correspond. This feels fine enough.
    /// </summary>
    private static readonly Dictionary<
        string,
        (SaveSystem.ConfigSettings keyboard, SaveSystem.ConfigSettings controller)
    > ConfigMap = new()
    {
        {
            "_arrowUp",
            (SaveSystem.ConfigSettings.InputKeyboardUp, SaveSystem.ConfigSettings.InputControllerUp)
        },
        {
            "_arrowDown",
            (
                SaveSystem.ConfigSettings.InputKeyboardDown,
                SaveSystem.ConfigSettings.InputControllerDown
            )
        },
        {
            "_arrowLeft",
            (
                SaveSystem.ConfigSettings.InputKeyboardLeft,
                SaveSystem.ConfigSettings.InputControllerLeft
            )
        },
        {
            "_arrowRight",
            (
                SaveSystem.ConfigSettings.InputKeyboardRight,
                SaveSystem.ConfigSettings.InputControllerRight
            )
        },
        {
            "_secondaryPlacement",
            (
                SaveSystem.ConfigSettings.InputKeyboardSecondary,
                SaveSystem.ConfigSettings.InputControllerSecondary
            )
        },
        {
            "_inventory",
            (
                SaveSystem.ConfigSettings.InputKeyboardInventory,
                SaveSystem.ConfigSettings.InputControllerInventory
            )
        },
    };

    /// <summary>
    /// Saves the key to the input based on its input name into the correct config setting.
    /// </summary>
    /// <param name="button">buttone name string.</param>
    /// <param name="key">The input key to be saved.</param>
    private void SaveKeyInput(string button, InputEvent key)
    {
        //Just checks if the button is a button we account for, and gets the correct config setting
        if (!ConfigMap.TryGetValue(button, out var configPair))
            return;

        int keycode = key switch
        {
            InputEventKey keyEvent => (int)keyEvent.PhysicalKeycode,
            InputEventJoypadButton joyEvent => (int)joyEvent.ButtonIndex,
            _ => -1,
        };

        if (keycode < 0)
            return;

        var config = key is InputEventKey ? configPair.keyboard : configPair.controller;
        SaveSystem.UpdateConfig(config, keycode);
    }

    /// <summary>
    /// Keyboard input key as text standardization.
    /// </summary>
    /// <param name="text">An input event as text.</param>
    /// <returns></returns>
    private static string CleanKeyboardText(string text)
    {
        return text.Replace(" (Physical)", "");
    }

    /// <summary>
    /// Returns true if the key is not already mapped to one of our concerned input maps. E.g. returns false if this key is already set on the menu
    /// </summary>
    /// <param name="keyText">An input event as text.</param>
    /// <returns></returns>
    private bool IsUniqueKey(string keyText)
    {
        //nested loops bad, but theoretically this should act as a single loop
        //since each action only has 1 event (keybinding)
        foreach (string action in _inputMapNames)
        {
            foreach (InputEvent evt in InputMap.ActionGetEvents(action))
            {
                if (
                    (
                        evt is InputEventKey keyEvent
                        && CleanKeyboardText(keyEvent.AsText()) == keyText
                    ) || (evt is InputEventJoypadButton padEvent && padEvent.AsText() == keyText)
                )
                    return false;
            }
        }
        return true;
    }
}
