using System;
using System.Collections.Generic;
using FunkEngine;
using Godot;

public partial class ControlSettings : Node2D, IFocusableMenu
{
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

    private Sprite2D _keyboardLeftIcon;
    private Sprite2D _keyboardRightIcon;
    private Sprite2D _keyboardUpIcon;
    private Sprite2D _keyboardDownIcon;
    private Sprite2D _keyboardSecondaryIcon;
    private Sprite2D _keyboardInventoryIcon;

    private Sprite2D _controllerLeftLabel;
    private Sprite2D _controllerRightLabel;
    private Sprite2D _controllerUpLabel;
    private Sprite2D _controllerDownLabel;
    private Sprite2D _controllerSecondaryLabel;
    private Sprite2D _controllerInventoryLabel;

    private TabContainer _remapTabs;
    private Key _tempKeyboardKey;
    private JoyButton _tempJoyButton;
    private string _chosenKey = "";

    private string[] _inputMapKeys =
    [
        "Pause",
        "CONTROLLER_arrowUp",
        "CONTROLLER_arrowDown",
        "CONTROLLER_arrowLeft",
        "CONTROLLER_arrowRight",
        "CONTROLLER_secondaryPlacement",
        "CONTROLLER_inventory",
        "WASD_arrowUp",
        "WASD_arrowDown",
        "WASD_arrowLeft",
        "WASD_arrowRight",
        "WASD_secondaryPlacement",
        "WASD_inventory",
    ];

    private HashSet<Key> _invalidKeys = new HashSet<Key>
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

    public override void _Ready()
    {
        var remapButtons = new Dictionary<string, string>
        {
            { "LeftArrowButton", "_arrowLeft" },
            { "RightArrowButton", "_arrowRight" },
            { "UpArrowButton", "_arrowUp" },
            { "DownArrowButton", "_arrowDown" },
            { "SecondaryPlacementButton", "_secondaryPlacement" },
            { "InventoryButton", "_inventory" },
        };

        foreach (var pair in remapButtons)
        {
            string keyboardPath = "Panel/TabContainer/CONTROLS_KEYBOARD/" + pair.Key;
            GetNode<Button>(keyboardPath).Pressed += () => OnRemapButtonPressed(pair.Value);

            string controllerPath = "Panel/TabContainer/CONTROLS_CONTROLLER/" + pair.Key;
            GetNode<Button>(controllerPath).Pressed += () => OnRemapButtonPressed(pair.Value);
        }

        GetNode<Timer>("RemapPopup/Timer").Connect("timeout", Callable.From(OnTimerEnd));

        _remapTabs = GetNode<TabContainer>("Panel/TabContainer");
        _remapTabs.CurrentTab =
            SaveSystem.GetConfigValue(SaveSystem.ConfigSettings.InputType).ToString() == "WASD"
                ? 0
                : 1;
        _remapTabs.Connect("tab_changed", Callable.From((int _) => ChangeInputType(0)));

        InitLabels();

        _closeButton.Pressed += ReturnToPrev;
    }

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

        _focused = GetNode<Button>(
            _remapTabs.CurrentTab == 0
                ? "Panel/TabContainer/CONTROLS_KEYBOARD/LeftArrowButton"
                : "Panel/TabContainer/CONTROLS_CONTROLLER/LeftArrowButton"
        );
        _focused.GrabFocus();
    }

    public void ReturnToPrev()
    {
        Prev.ResumeFocus();
        QueueFree();
    }

    private void InitLabels()
    {
        var keyboardPaths = new Dictionary<string, Action<Sprite2D>>
        {
            { "LeftArrowKey", node => _keyboardLeftIcon = node },
            { "RightArrowKey", node => _keyboardRightIcon = node },
            { "UpArrowKey", node => _keyboardUpIcon = node },
            { "DownArrowKey", node => _keyboardDownIcon = node },
            { "SecondaryKey", node => _keyboardSecondaryIcon = node },
            { "InventoryKey", node => _keyboardInventoryIcon = node },
        };

        var controllerPaths = new Dictionary<string, Action<Sprite2D>>
        {
            { "LeftArrowKey", node => _controllerLeftLabel = node },
            { "RightArrowKey", node => _controllerRightLabel = node },
            { "UpArrowKey", node => _controllerUpLabel = node },
            { "DownArrowKey", node => _controllerDownLabel = node },
            { "SecondaryKey", node => _controllerSecondaryLabel = node },
            { "InventoryKey", node => _controllerInventoryLabel = node },
        };

        foreach (var pair in keyboardPaths)
        {
            var path = "Panel/TabContainer/CONTROLS_KEYBOARD/" + pair.Key;
            var sprite = GetNode<Sprite2D>(path);
            pair.Value(sprite);
        }

        foreach (var pair in controllerPaths)
        {
            var path = "Panel/TabContainer/CONTROLS_CONTROLLER/" + pair.Key;
            var sprite = GetNode<Sprite2D>(path);
            pair.Value(sprite);
        }

        UpdateKeyLabels();
    }

    public override void _Input(InputEvent @event)
    {
        if (_remapPopup.Visible)
        {
            if (
                (@event is InputEventKey || @event is InputEventJoypadButton)
                && ValidInputEvent(@event.AsText())
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

    private void HandleRemapInput(InputEvent @event)
    {
        bool isKeyboard = _remapTabs.CurrentTab == 0;

        switch (isKeyboard)
        {
            case true when @event is InputEventKey keyEvent:
            {
                if (_invalidKeys.Contains(keyEvent.Keycode))
                    return;

                string action = "WASD" + _chosenKey;
                InputMap.ActionEraseEvents(action);
                InputMap.ActionAddEvent(action, keyEvent);

                SaveKeyInput(_chosenKey, keyEvent);
                break;
            }
            case false when @event is InputEventJoypadButton joyEvent:
            {
                string action = "CONTROLLER" + _chosenKey;
                InputMap.ActionEraseEvents(action);
                InputMap.ActionAddEvent(action, joyEvent);

                SaveKeyInput(_chosenKey, joyEvent);
                break;
            }
            default:
                return;
        }

        UpdateKeyLabels();
        _remapPopup.Visible = false;
    }

    private void AnyButtonsPressed()
    {
        BeginTimer(5);
        _tempJoyButton = JoyButton.Invalid;
        _tempKeyboardKey = Key.None;
        _remapPopup.Visible = true;
    }

    private void BeginTimer(float time)
    {
        _remapLabel.Text = time.ToString();
        _remapTimer.Start(time);
    }

    private void OnTimerEnd()
    {
        _remapPopup.Visible = false;
    }

    private void UpdateKeyLabels()
    {
        var keyMappings = new Dictionary<Sprite2D, string>
        {
            { _keyboardUpIcon, "WASD_arrowUp" },
            { _keyboardDownIcon, "WASD_arrowDown" },
            { _keyboardLeftIcon, "WASD_arrowLeft" },
            { _keyboardRightIcon, "WASD_arrowRight" },
            { _keyboardSecondaryIcon, "WASD_secondaryPlacement" },
            { _keyboardInventoryIcon, "WASD_inventory" },
            { _controllerUpLabel, "CONTROLLER_arrowUp" },
            { _controllerDownLabel, "CONTROLLER_arrowDown" },
            { _controllerLeftLabel, "CONTROLLER_arrowLeft" },
            { _controllerRightLabel, "CONTROLLER_arrowRight" },
            { _controllerSecondaryLabel, "CONTROLLER_secondaryPlacement" },
            { _controllerInventoryLabel, "CONTROLLER_inventory" },
        };

        foreach (var (sprite, actionName) in keyMappings)
        {
            var events = InputMap.ActionGetEvents(actionName);
            if (events.Count > 0)
            {
                string textureName = events[0].AsText();

                // Clean up the texture name
                if (actionName.StartsWith("WASD"))
                    textureName = CleanKeyboardText(textureName);
                else
                    textureName = textureName.Replace("/", "");

                sprite.Texture = GD.Load<Texture2D>($"{IconPath}{textureName}.png");
            }
        }
    }

    string CleanKeyboardText(string text)
    {
        return text.Replace(" (Physical)", "");
    }

    private void OnRemapButtonPressed(string keyName)
    {
        _chosenKey = keyName;
        AnyButtonsPressed();
    }

    private void SaveKeyInput(string button, InputEvent key)
    {
        var configMap = new Dictionary<
            string,
            (SaveSystem.ConfigSettings keyboard, SaveSystem.ConfigSettings controller)
        >
        {
            {
                "_arrowUp",
                (
                    SaveSystem.ConfigSettings.InputKeyboardUp,
                    SaveSystem.ConfigSettings.InputControllerUp
                )
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

        if (!configMap.TryGetValue(button, out var configPair))
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

    private void ChangeInputType(int tabIndex)
    {
        SaveSystem.UpdateConfig(
            SaveSystem.ConfigSettings.InputType,
            _remapTabs.CurrentTab == 0 ? "WASD" : "CONTROLLER"
        );
    }

    private bool ValidInputEvent(string keyText)
    {
        //nested loops bad, but theoretically this should act as a single loop
        //since each action only has 1 event (keybinding)
        foreach (string action in _inputMapKeys)
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
