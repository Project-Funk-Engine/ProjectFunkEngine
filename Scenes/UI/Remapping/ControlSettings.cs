using System.Collections.Generic;
using FunkEngine;
using Godot;

public partial class ControlSettings : Node2D, IFocusableMenu
{
    public static readonly string LoadPath = "res://Scenes/UI/Remapping/Remap.tscn";

    [Export]
    public Sprite2D LeftKey;

    [Export]
    public Sprite2D RightKey;

    [Export]
    public Sprite2D UpKey;

    [Export]
    public Sprite2D DownKey;

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

    private Label _keyboardLeftLabel;
    private Label _keyboardRightLabel;
    private Label _keyboardUpLabel;
    private Label _keyboardDownLabel;
    private Label _keyboardSecondaryLabel;

    private Label _controllerLeftLabel;
    private Label _controllerRightLabel;
    private Label _controllerUpLabel;
    private Label _controllerDownLabel;
    private Label _controllerSecondaryLabel;

    private TabContainer _remapTabs;
    private Key _tempKeyboardKey;
    private JoyButton _tempJoyButton;
    private string _chosenKey = "";

    private string[] _inputMapKeys =
    [
        "Pause",
        "Inventory",
        "CONTROLLER_arrowUp",
        "CONTROLLER_arrowDown",
        "CONTROLLER_arrowLeft",
        "CONTROLLER_arrowRight",
        "CONTROLLER_secondaryPlacement",
        "WASD_arrowUp",
        "WASD_arrowDown",
        "WASD_arrowLeft",
        "WASD_arrowRight",
        "WASD_secondaryPlacement",
    ];

    public override void _Ready()
    {
        GetNode<Button>("Panel/TabContainer/Keyboard/LeftArrowButton")
            .Connect("pressed", Callable.From(OnLeftButtonPressed));
        GetNode<Button>("Panel/TabContainer/Keyboard/UpArrowButton")
            .Connect("pressed", Callable.From(OnUpButtonPressed));
        GetNode<Button>("Panel/TabContainer/Keyboard/DownArrowButton")
            .Connect("pressed", Callable.From(OnDownButtonPressed));
        GetNode<Button>("Panel/TabContainer/Keyboard/RightArrowButton")
            .Connect("pressed", Callable.From(OnRightButtonPressed));
        GetNode<Button>("Panel/TabContainer/Keyboard/SecondaryPlacementButton")
            .Connect("pressed", Callable.From(OnSecondaryPlacementButtonPressed));

        GetNode<Button>("Panel/TabContainer/Controller/LeftArrowButton")
            .Connect("pressed", Callable.From(OnLeftButtonPressed));
        GetNode<Button>("Panel/TabContainer/Controller/UpArrowButton")
            .Connect("pressed", Callable.From(OnUpButtonPressed));
        GetNode<Button>("Panel/TabContainer/Controller/DownArrowButton")
            .Connect("pressed", Callable.From(OnDownButtonPressed));
        GetNode<Button>("Panel/TabContainer/Controller/RightArrowButton")
            .Connect("pressed", Callable.From(OnRightButtonPressed));
        GetNode<Button>("Panel/TabContainer/Controller/SecondaryPlacementButton")
            .Connect("pressed", Callable.From(OnSecondaryPlacementButtonPressed));

        GetNode<Timer>("RemapPopup/Timer").Connect("timeout", Callable.From(OnTimerEnd));

        //TODO: redo implementation? currently the selected tab chooses keyboard or controller
        _remapTabs = GetNode<TabContainer>("Panel/TabContainer");
        _remapTabs.CurrentTab =
            SaveSystem.GetConfigValue(SaveSystem.ConfigSettings.InputType).ToString() == "WASD"
                ? 0
                : 1;
        _remapTabs.Connect("tab_changed", Callable.From((int _) => ChangeInputType(0)));

        InitLabels();

        _focused = GetNode<Button>(
            _remapTabs.CurrentTab == 0
                ? "Panel/TabContainer/Keyboard/LeftArrowButton"
                : "Panel/TabContainer/Controller/LeftArrowButton"
        );
        _focused.GrabFocus();

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
    }

    public void ReturnToPrev()
    {
        Prev.ResumeFocus();
        QueueFree();
    }

    private void InitLabels()
    {
        //TODO: revamp this function to incorporate art assets (maybe stretch goal)
        _keyboardLeftLabel = GetNode<Label>("Panel/TabContainer/Keyboard/TEMPLeftRemap");
        _keyboardRightLabel = GetNode<Label>("Panel/TabContainer/Keyboard/TEMPRightRemap");
        _keyboardUpLabel = GetNode<Label>("Panel/TabContainer/Keyboard/TEMPUpRemap");
        _keyboardDownLabel = GetNode<Label>("Panel/TabContainer/Keyboard/TEMPDownRemap");
        _keyboardSecondaryLabel = GetNode<Label>("Panel/TabContainer/Keyboard/TEMPSecondaryRemap");
        _controllerLeftLabel = GetNode<Label>("Panel/TabContainer/Controller/TEMPLeftRemap");
        _controllerRightLabel = GetNode<Label>("Panel/TabContainer/Controller/TEMPRightRemap");
        _controllerUpLabel = GetNode<Label>("Panel/TabContainer/Controller/TEMPUpRemap");
        _controllerDownLabel = GetNode<Label>("Panel/TabContainer/Controller/TEMPDownRemap");
        _controllerSecondaryLabel = GetNode<Label>(
            "Panel/TabContainer/Controller/TEMPSecondaryRemap"
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
        _keyboardUpLabel.Text = CleanKeyboardText(
            InputMap.ActionGetEvents("WASD_arrowUp")[0].AsText()
        );
        _keyboardDownLabel.Text = CleanKeyboardText(
            InputMap.ActionGetEvents("WASD_arrowDown")[0].AsText()
        );
        _keyboardLeftLabel.Text = CleanKeyboardText(
            InputMap.ActionGetEvents("WASD_arrowLeft")[0].AsText()
        );
        _keyboardRightLabel.Text = CleanKeyboardText(
            InputMap.ActionGetEvents("WASD_arrowRight")[0].AsText()
        );
        _keyboardSecondaryLabel.Text = CleanKeyboardText(
            InputMap.ActionGetEvents("WASD_secondaryPlacement")[0].AsText()
        );
        _controllerUpLabel.Text = InputMap.ActionGetEvents("CONTROLLER_arrowUp")[0].AsText();
        _controllerDownLabel.Text = InputMap.ActionGetEvents("CONTROLLER_arrowDown")[0].AsText();
        _controllerLeftLabel.Text = InputMap.ActionGetEvents("CONTROLLER_arrowLeft")[0].AsText();
        _controllerRightLabel.Text = InputMap.ActionGetEvents("CONTROLLER_arrowRight")[0].AsText();
        _controllerSecondaryLabel.Text = InputMap
            .ActionGetEvents("CONTROLLER_secondaryPlacement")[0]
            .AsText();
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
                {
                    return false;
                }
            }
        }
        return true;
    }
}
