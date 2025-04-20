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
        GetNode<Button>("Panel/TabContainer/CONTROLS_KEYBOARD/LeftArrowButton")
            .Connect("pressed", Callable.From(OnLeftButtonPressed));
        GetNode<Button>("Panel/TabContainer/CONTROLS_KEYBOARD/UpArrowButton")
            .Connect("pressed", Callable.From(OnUpButtonPressed));
        GetNode<Button>("Panel/TabContainer/CONTROLS_KEYBOARD/DownArrowButton")
            .Connect("pressed", Callable.From(OnDownButtonPressed));
        GetNode<Button>("Panel/TabContainer/CONTROLS_KEYBOARD/RightArrowButton")
            .Connect("pressed", Callable.From(OnRightButtonPressed));
        GetNode<Button>("Panel/TabContainer/CONTROLS_KEYBOARD/SecondaryPlacementButton")
            .Connect("pressed", Callable.From(OnSecondaryPlacementButtonPressed));
        GetNode<Button>("Panel/TabContainer/CONTROLS_KEYBOARD/InventoryButton")
            .Connect("pressed", Callable.From(OnInventoryButtonPressed));

        GetNode<Button>("Panel/TabContainer/CONTROLS_CONTROLLER/LeftArrowButton")
            .Connect("pressed", Callable.From(OnLeftButtonPressed));
        GetNode<Button>("Panel/TabContainer/CONTROLS_CONTROLLER/UpArrowButton")
            .Connect("pressed", Callable.From(OnUpButtonPressed));
        GetNode<Button>("Panel/TabContainer/CONTROLS_CONTROLLER/DownArrowButton")
            .Connect("pressed", Callable.From(OnDownButtonPressed));
        GetNode<Button>("Panel/TabContainer/CONTROLS_CONTROLLER/RightArrowButton")
            .Connect("pressed", Callable.From(OnRightButtonPressed));
        GetNode<Button>("Panel/TabContainer/CONTROLS_CONTROLLER/SecondaryPlacementButton")
            .Connect("pressed", Callable.From(OnSecondaryPlacementButtonPressed));
        GetNode<Button>("Panel/TabContainer/CONTROLS_CONTROLLER/InventoryButton")
            .Connect("pressed", Callable.From(OnInventoryButtonPressed));

        GetNode<Timer>("RemapPopup/Timer").Connect("timeout", Callable.From(OnTimerEnd));

        //TODO: redo implementation? currently the selected tab chooses keyboard or controller
        _remapTabs = GetNode<TabContainer>("Panel/TabContainer");
        _remapTabs.CurrentTab =
            SaveSystem.GetConfigValue(SaveSystem.ConfigSettings.InputType).ToString() == "WASD"
                ? 0
                : 1;
        _remapTabs.Connect("tab_changed", Callable.From((int _) => ChangeInputType(0)));

        InitLabels();

        _remapPopup.ProcessMode = ProcessModeEnum.Always;
        _remapLabel.ProcessMode = ProcessModeEnum.Always;
        _remapTimer.ProcessMode = ProcessModeEnum.Always;

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
        //TODO: revamp this function to incorporate art assets (maybe stretch goal)
        _keyboardLeftIcon = GetNode<Sprite2D>("Panel/TabContainer/CONTROLS_KEYBOARD/LeftArrowKey");
        _keyboardRightIcon = GetNode<Sprite2D>(
            "Panel/TabContainer/CONTROLS_KEYBOARD/RightArrowKey"
        );
        _keyboardUpIcon = GetNode<Sprite2D>("Panel/TabContainer/CONTROLS_KEYBOARD/UpArrowKey");
        _keyboardDownIcon = GetNode<Sprite2D>("Panel/TabContainer/CONTROLS_KEYBOARD/DownArrowKey");
        _keyboardSecondaryIcon = GetNode<Sprite2D>(
            "Panel/TabContainer/CONTROLS_KEYBOARD/SecondaryKey"
        );
        _keyboardInventoryIcon = GetNode<Sprite2D>(
            "Panel/TabContainer/CONTROLS_KEYBOARD/InventoryKey"
        );
        _controllerLeftLabel = GetNode<Sprite2D>(
            "Panel/TabContainer/CONTROLS_CONTROLLER/LeftArrowKey"
        );
        _controllerRightLabel = GetNode<Sprite2D>(
            "Panel/TabContainer/CONTROLS_CONTROLLER/RightArrowKey"
        );
        _controllerUpLabel = GetNode<Sprite2D>("Panel/TabContainer/CONTROLS_CONTROLLER/UpArrowKey");
        _controllerDownLabel = GetNode<Sprite2D>(
            "Panel/TabContainer/CONTROLS_CONTROLLER/DownArrowKey"
        );
        _controllerSecondaryLabel = GetNode<Sprite2D>(
            "Panel/TabContainer/CONTROLS_CONTROLLER/SecondaryKey"
        );
        _controllerInventoryLabel = GetNode<Sprite2D>(
            "Panel/TabContainer/CONTROLS_CONTROLLER/InventoryKey"
        );

        UpdateKeyLabels();
    }

    public override void _Input(InputEvent @event)
    {
        if (_remapPopup.Visible)
        {
            if (ValidInputEvent(@event.AsText()))
            {
                switch (@event)
                {
                    case InputEventKey eventKeyboardKey when _remapTabs.CurrentTab == 0:
                        if (_invalidKeys.Contains(eventKeyboardKey.Keycode))
                            break;

                        _tempKeyboardKey = eventKeyboardKey.Keycode;
                        InputMap.ActionEraseEvents("WASD" + _chosenKey);
                        InputMap.ActionAddEvent("WASD" + _chosenKey, @event);
                        SaveKeyInput(_chosenKey, @event);
                        UpdateKeyLabels();
                        _remapPopup.Visible = false;
                        break;
                    case InputEventJoypadButton eventControllerKey when _remapTabs.CurrentTab == 1:
                        _tempJoyButton = eventControllerKey.ButtonIndex;
                        InputMap.ActionEraseEvents("CONTROLLER" + _chosenKey);
                        InputMap.ActionAddEvent("CONTROLLER" + _chosenKey, @event);
                        SaveKeyInput(_chosenKey, @event);
                        UpdateKeyLabels();
                        _remapPopup.Visible = false;
                        break;
                }

                GetViewport().SetInputAsHandled();
            }
            GetViewport().SetInputAsHandled();
        }
        else if (@event.IsActionPressed("ui_cancel"))
        {
            ReturnToPrev();
            GetViewport().SetInputAsHandled();
        }
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
        _keyboardUpIcon.Texture = GD.Load<Texture2D>(
            IconPath
                + CleanKeyboardText(InputMap.ActionGetEvents("WASD_arrowUp")[0].AsText())
                + ".png"
        );
        _keyboardDownIcon.Texture = GD.Load<Texture2D>(
            IconPath
                + CleanKeyboardText(InputMap.ActionGetEvents("WASD_arrowDown")[0].AsText())
                + ".png"
        );
        _keyboardLeftIcon.Texture = GD.Load<Texture2D>(
            IconPath
                + CleanKeyboardText(InputMap.ActionGetEvents("WASD_arrowLeft")[0].AsText())
                + ".png"
        );
        _keyboardRightIcon.Texture = GD.Load<Texture2D>(
            IconPath
                + CleanKeyboardText(InputMap.ActionGetEvents("WASD_arrowRight")[0].AsText())
                + ".png"
        );
        _keyboardSecondaryIcon.Texture = GD.Load<Texture2D>(
            IconPath
                + CleanKeyboardText(InputMap.ActionGetEvents("WASD_secondaryPlacement")[0].AsText())
                + ".png"
        );
        _keyboardInventoryIcon.Texture = GD.Load<Texture2D>(
            IconPath
                + CleanKeyboardText(InputMap.ActionGetEvents("WASD_inventory")[0].AsText())
                + ".png"
        );
        _controllerUpLabel.Texture = GD.Load<Texture2D>(
            IconPath
                + InputMap.ActionGetEvents("CONTROLLER_arrowUp")[0].AsText().Replace("/", "")
                + ".png"
        );
        _controllerDownLabel.Texture = GD.Load<Texture2D>(
            IconPath
                + InputMap.ActionGetEvents("CONTROLLER_arrowDown")[0].AsText().Replace("/", "")
                + ".png"
        );
        _controllerLeftLabel.Texture = GD.Load<Texture2D>(
            IconPath
                + InputMap.ActionGetEvents("CONTROLLER_arrowLeft")[0].AsText().Replace("/", "")
                + ".png"
        );
        _controllerRightLabel.Texture = GD.Load<Texture2D>(
            IconPath
                + InputMap.ActionGetEvents("CONTROLLER_arrowRight")[0].AsText().Replace("/", "")
                + ".png"
        );
        _controllerSecondaryLabel.Texture = GD.Load<Texture2D>(
            IconPath
                + InputMap
                    .ActionGetEvents("CONTROLLER_secondaryPlacement")[0]
                    .AsText()
                    .Replace("/", "")
                + ".png"
        );
        _controllerInventoryLabel.Texture = GD.Load<Texture2D>(
            IconPath
                + InputMap.ActionGetEvents("CONTROLLER_inventory")[0].AsText().Replace("/", "")
                + ".png"
        );
    }

    string CleanKeyboardText(string text)
    {
        return text.Replace(" (Physical)", "");
    }

    private void OnUpButtonPressed()
    {
        _chosenKey = "_arrowUp";
        AnyButtonsPressed();
    }

    private void OnLeftButtonPressed()
    {
        _chosenKey = "_arrowLeft";
        AnyButtonsPressed();
    }

    private void OnRightButtonPressed()
    {
        _chosenKey = "_arrowRight";
        AnyButtonsPressed();
    }

    private void OnDownButtonPressed()
    {
        _chosenKey = "_arrowDown";
        AnyButtonsPressed();
    }

    private void OnSecondaryPlacementButtonPressed()
    {
        _chosenKey = "_secondaryPlacement";
        AnyButtonsPressed();
    }

    private void OnInventoryButtonPressed()
    {
        _chosenKey = "_inventory";
        AnyButtonsPressed();
    }

    private void SaveKeyInput(string button, InputEvent key)
    {
        switch (button[6])
        {
            case 'U':
                if (key is InputEventKey key1)
                {
                    int keycode = (int)key1.PhysicalKeycode;
                    SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.InputKeyboardUp, keycode);
                }
                else if (key is InputEventJoypadButton key2)
                {
                    int keycode = (int)key2.ButtonIndex;
                    SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.InputControllerUp, keycode);
                }
                break;
            case 'L':
                if (key is InputEventKey key3)
                {
                    int keycode = (int)key3.PhysicalKeycode;
                    SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.InputKeyboardLeft, keycode);
                }
                else if (key is InputEventJoypadButton key4)
                {
                    int keycode = (int)key4.ButtonIndex;
                    SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.InputControllerLeft, keycode);
                }
                break;
            case 'D':
                if (key is InputEventKey key5)
                {
                    int keycode = (int)key5.PhysicalKeycode;
                    SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.InputKeyboardDown, keycode);
                }
                else if (key is InputEventJoypadButton key6)
                {
                    int keycode = (int)key6.ButtonIndex;
                    SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.InputControllerDown, keycode);
                }
                break;
            case 'R':
                if (key is InputEventKey key7)
                {
                    int keycode = (int)key7.PhysicalKeycode;
                    SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.InputKeyboardRight, keycode);
                }
                else if (key is InputEventJoypadButton key8)
                {
                    int keycode = (int)key8.ButtonIndex;
                    SaveSystem.UpdateConfig(
                        SaveSystem.ConfigSettings.InputControllerRight,
                        keycode
                    );
                }
                break;
            case 'd':
                if (key is InputEventKey key9)
                {
                    int keycode = (int)key9.PhysicalKeycode;
                    SaveSystem.UpdateConfig(
                        SaveSystem.ConfigSettings.InputKeyboardSecondary,
                        keycode
                    );
                }
                else if (key is InputEventJoypadButton key10)
                {
                    int keycode = (int)key10.ButtonIndex;
                    SaveSystem.UpdateConfig(
                        SaveSystem.ConfigSettings.InputControllerSecondary,
                        keycode
                    );
                }
                break;
            case 't':
                if (key is InputEventKey key11)
                {
                    int keycode = (int)key11.PhysicalKeycode;
                    SaveSystem.UpdateConfig(
                        SaveSystem.ConfigSettings.InputKeyboardInventory,
                        keycode
                    );
                }
                else if (key is InputEventJoypadButton key12)
                {
                    int keycode = (int)key12.ButtonIndex;
                    SaveSystem.UpdateConfig(
                        SaveSystem.ConfigSettings.InputControllerInventory,
                        keycode
                    );
                }
                break;
        }
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
