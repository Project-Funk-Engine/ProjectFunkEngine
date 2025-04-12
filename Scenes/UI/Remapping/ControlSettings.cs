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

    [Export]
    private Button _closeButton;

    [Export]
    private Panel _remapPopup;

    [Export]
    private Label _remapLabel;

    [Export]
    private Timer _remapTimer;

    private Label _leftLabel;
    private Label _rightLabel;
    private Label _upLabel;
    private Label _downLabel;
    private Label _secondaryPlacementLabel;

    private Key _tempKeyboardKey;
    private JoyButton _tempJoyButton;
    private string _chosenKey = "";

    public override void _Ready()
    {
        GetNode<Button>("Panel/LeftArrowButton")
            .Connect("pressed", Callable.From(OnLeftButtonPressed));
        GetNode<Button>("Panel/UpArrowButton").Connect("pressed", Callable.From(OnUpButtonPressed));
        GetNode<Button>("Panel/DownArrowButton")
            .Connect("pressed", Callable.From(OnDownButtonPressed));
        GetNode<Button>("Panel/RightArrowButton")
            .Connect("pressed", Callable.From(OnRightButtonPressed));
        GetNode<Button>("Panel/SecondaryPlacementButton")
            .Connect("pressed", Callable.From(OnSecondaryPlacementButtonPressed));

        GetNode<Timer>("RemapPopup/Timer").Connect("timeout", Callable.From(OnTimerEnd));

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
    }

    public void ReturnToPrev()
    {
        Prev.ResumeFocus();
        QueueFree();
    }

    private void InitLabels()
    {
        _leftLabel = GetNode<Label>("Panel/Control/TEMPLeftRemap");
        _rightLabel = GetNode<Label>("Panel/Control/TEMPRightRemap");
        _upLabel = GetNode<Label>("Panel/Control/TEMPUpRemap");
        _downLabel = GetNode<Label>("Panel/Control/TEMPDownRemap");
        _secondaryPlacementLabel = GetNode<Label>("Panel/Control/TEMPSecondaryRemap");

        _leftLabel.Text = SaveSystem
            .GetConfigValue(SaveSystem.ConfigSettings.InputKeyboardLeft)
            .ToString();
        _rightLabel.Text = SaveSystem
            .GetConfigValue(SaveSystem.ConfigSettings.InputKeyboardRight)
            .ToString();
        _upLabel.Text = SaveSystem
            .GetConfigValue(SaveSystem.ConfigSettings.InputKeyboardUp)
            .ToString();
        _downLabel.Text = SaveSystem
            .GetConfigValue(SaveSystem.ConfigSettings.InputKeyboardDown)
            .ToString();
        _secondaryPlacementLabel.Text = InputMap
            .ActionGetEvents("WASD_secondaryPlacement")[0]
            .AsText();
    }

    public override void _Input(InputEvent @event)
    {
        if (_remapPopup.Visible)
        {
            if (@event is InputEventKey eventKeyboardKey)
            {
                _tempKeyboardKey = eventKeyboardKey.Keycode;
                InputMap.ActionEraseEvents(_chosenKey);
                InputMap.ActionAddEvent(_chosenKey, @event);
                SaveKeyInput(_chosenKey, @event.AsText());
                UpdateKeyLabels();
                _remapPopup.Visible = false;
            }
            if (@event is InputEventJoypadButton eventControllerKey)
            {
                _tempJoyButton = eventControllerKey.ButtonIndex;
                InputMap.ActionEraseEvents(_chosenKey);
                InputMap.ActionAddEvent(_chosenKey, @event);
                UpdateKeyLabels();
                _remapPopup.Visible = false;
            }
        }
        else if (@event.IsActionPressed("ui_cancel"))
        {
            ReturnToPrev();
            GetViewport().SetInputAsHandled();
        }
    }

    private void AnyButtonsPressed()
    {
        BeginTimer();
        _tempJoyButton = JoyButton.Invalid;
        _tempKeyboardKey = Key.None;
        _remapPopup.Visible = true;
    }

    private void BeginTimer()
    {
        _remapLabel.Text = "5";
        _remapTimer.Start(5.0f);
    }

    private void OnTimerEnd()
    {
        _remapPopup.Visible = false;
    }

    private void UpdateKeyLabels()
    {
        _upLabel.Text = InputMap.ActionGetEvents("WASD_arrowUp")[0].AsText();
        _downLabel.Text = InputMap.ActionGetEvents("WASD_arrowDown")[0].AsText();
        _leftLabel.Text = InputMap.ActionGetEvents("WASD_arrowLeft")[0].AsText();
        _rightLabel.Text = InputMap.ActionGetEvents("WASD_arrowRight")[0].AsText();
        _secondaryPlacementLabel.Text = InputMap
            .ActionGetEvents("WASD_secondaryPlacement")[0]
            .AsText();
    }

    private void OnUpButtonPressed()
    {
        _chosenKey = "WASD_arrowUp";
        AnyButtonsPressed();
    }

    private void OnLeftButtonPressed()
    {
        _chosenKey = "WASD_arrowLeft";
        AnyButtonsPressed();
    }

    private void OnRightButtonPressed()
    {
        _chosenKey = "WASD_arrowRight";
        AnyButtonsPressed();
    }

    private void OnDownButtonPressed()
    {
        _chosenKey = "WASD_arrowDown";
        AnyButtonsPressed();
    }

    private void OnSecondaryPlacementButtonPressed()
    {
        _chosenKey = "WASD_secondaryPlacement";
        AnyButtonsPressed();
    }

    private void SaveKeyInput(string button, string key)
    {
        switch (button[10])
        {
            case 'U':
                SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.InputKeyboardUp, key);
                break;
            case 'L':
                SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.InputKeyboardLeft, key);
                break;
            case 'D':
                SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.InputKeyboardDown, key);
                break;
            case 'R':
                SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.InputKeyboardRight, key);
                break;
        }
    }
}
