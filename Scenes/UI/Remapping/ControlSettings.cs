using System;
using FunkEngine;
using Godot;

public partial class ControlSettings : Node2D, IFocusableMenu
{
    [Export]
    public Sprite2D leftKey;

    [Export]
    public Sprite2D rightKey;

    [Export]
    public Sprite2D upKey;

    [Export]
    public Sprite2D downKey;

    public IFocusableMenu Prev { get; set; }

    [Export]
    private Button _closeButton;

    private Button _controllerButton;

    public override void _Ready()
    {
        GetNode<Button>("Panel/WASDButton").Connect("pressed", Callable.From(OnWASDButtonPressed));
        GetNode<Button>("Panel/ArrowButton")
            .Connect("pressed", Callable.From(OnArrowButtonPressed));
        GetNode<Button>("Panel/QWERTButton")
            .Connect("pressed", Callable.From(OnQWERTButtonPressed));
        _controllerButton = GetNode<Button>("Panel/ControllerButton");
        _controllerButton.Connect("pressed", Callable.From(OnControllerButtonPressed));

        ControllerConnectionChanged(-1, false);
        Input.JoyConnectionChanged += ControllerConnectionChanged;

        _closeButton.Pressed += ReturnToPrev;
    }

    public void ResumeFocus()
    {
        ProcessMode = ProcessModeEnum.Inherit;
        GetCurrentSelection();
    }

    public void PauseFocus()
    {
        ProcessMode = ProcessModeEnum.Disabled;
    }

    public void OpenMenu(IFocusableMenu prev)
    {
        Prev = prev;
        Prev.PauseFocus();

        GetCurrentSelection();
    }

    public void ReturnToPrev()
    {
        Prev.ResumeFocus();
        QueueFree();
    }

    private void GetCurrentSelection()
    {
        string scheme = SaveSystem.GetConfigValue(SaveSystem.ConfigSettings.InputKey).As<string>();
        switch (scheme)
        {
            case "ARROWS":
                OnArrowButtonPressed();
                GetNode<Button>("Panel/ArrowButton").GrabFocus();
                break;
            case "QWERT":
                OnQWERTButtonPressed();
                GetNode<Button>("Panel/QWERTButton").GrabFocus();
                break;
            case "WASD":
                OnWASDButtonPressed();
                GetNode<Button>("Panel/WASDButton").GrabFocus();
                break;
            case "CONTROLLER":
                OnControllerButtonPressed();
                GetNode<Button>("Panel/ControllerButton").GrabFocus();
                break;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            ReturnToPrev();
            GetViewport().SetInputAsHandled();
        }
    }

    private void OnWASDButtonPressed()
    {
        GetNode<Label>("Panel/Label").Text =
            Tr("CONTROLS_TITLE_TYPE_WASD") + " " + Tr("CONTROLS_TITLE_SELECTED");
        SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.InputKey, "WASD");
        ChangeKeySprites("WASD");
    }

    private void OnArrowButtonPressed()
    {
        GetNode<Label>("Panel/Label").Text =
            Tr("CONTROLS_TITLE_TYPE_ARROW") + " " + Tr("CONTROLS_TITLE_SELECTED");
        SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.InputKey, "ARROWS");
        ChangeKeySprites("ARROWS");
    }

    private void OnQWERTButtonPressed()
    {
        GetNode<Label>("Panel/Label").Text =
            Tr("CONTROLS_TITLE_TYPE_QWER") + " " + Tr("CONTROLS_TITLE_SELECTED");
        SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.InputKey, "QWERT");
        ChangeKeySprites("QWERT");
    }

    private void OnControllerButtonPressed()
    {
        GetNode<Label>("Panel/Label").Text = "Controller Selected";
        SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.InputKey, "CONTROLLER");
        ChangeKeySprites("CONTROLLER");
    }

    private void ControllerConnectionChanged(long id, bool connected)
    {
        _controllerButton.Visible = Input.GetConnectedJoypads().Count > 0;
        if (
            (string)SaveSystem.GetConfigValue(SaveSystem.ConfigSettings.InputKey) == "CONTROLLER"
            && !_closeButton.Visible
        )
        {
            OnArrowButtonPressed();
        }
    }

    private void ChangeKeySprites(string scheme)
    {
        var selectedScheme = ControlSchemes.SpriteMappings[scheme];
        leftKey.Texture = GD.Load<Texture2D>(selectedScheme["left"]);
        rightKey.Texture = GD.Load<Texture2D>(selectedScheme["right"]);
        upKey.Texture = GD.Load<Texture2D>(selectedScheme["up"]);
        downKey.Texture = GD.Load<Texture2D>(selectedScheme["down"]);
    }
}
