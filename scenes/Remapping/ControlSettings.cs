using System;
using Godot;

public partial class ControlSettings : Node2D
{
    [Export]
    public Sprite2D leftKey;

    [Export]
    public Sprite2D rightKey;

    [Export]
    public Sprite2D upKey;

    [Export]
    public Sprite2D downKey;

    private Node _previousScene;
    private ProcessModeEnum _previousProcessMode;

    [Export]
    private Button _closeButton;

    public override void _Ready()
    {
        GetNode<Button>("Panel/WASDButton").Connect("pressed", Callable.From(OnWASDButtonPressed));
        GetNode<Button>("Panel/ArrowButton")
            .Connect("pressed", Callable.From(OnArrowButtonPressed));
        GetNode<Button>("Panel/QWERTButton")
            .Connect("pressed", Callable.From(OnQWERTButtonPressed));

        string scheme = ProjectSettings.HasSetting("game/input_scheme")
            ? (string)ProjectSettings.GetSetting("game/input_scheme")
            : "ARROWS";
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
        }

        _closeButton.Pressed += CloseMenu;
    }

    public void OpenMenu(Node prevScene)
    {
        _previousScene = prevScene;
        _previousProcessMode = _previousScene.GetProcessMode();
        prevScene.ProcessMode = ProcessModeEnum.Disabled;
    }

    private void CloseMenu()
    {
        _previousScene.ProcessMode = _previousProcessMode;
        QueueFree();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("Pause"))
        {
            CloseMenu();
            GetViewport().SetInputAsHandled();
        }
    }

    private void OnWASDButtonPressed()
    {
        GetNode<Label>("Panel/Label").Text =
            Tr("CONTROLS_TITLE_TYPE_WASD") + " " + Tr("CONTROLS_TITLE_SELECTED");
        ProjectSettings.SetSetting("game/input_scheme", "WASD");
        ProjectSettings.Save();
        ChangeKeySprites("WASD");
    }

    private void OnArrowButtonPressed()
    {
        GetNode<Label>("Panel/Label").Text =
            Tr("CONTROLS_TITLE_TYPE_ARROW") + " " + Tr("CONTROLS_TITLE_SELECTED");
        ProjectSettings.SetSetting("game/input_scheme", "ARROWS");
        ProjectSettings.Save();
        ChangeKeySprites("ARROWS");
    }

    private void OnQWERTButtonPressed()
    {
        GetNode<Label>("Panel/Label").Text =
            Tr("CONTROLS_TITLE_TYPE_QWER") + " " + Tr("CONTROLS_TITLE_SELECTED");
        ProjectSettings.SetSetting("game/input_scheme", "QWERT");
        ProjectSettings.Save();
        ChangeKeySprites("QWERT");
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
