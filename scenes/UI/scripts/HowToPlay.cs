using System;
using Godot;

public partial class HowToPlay : Node2D
{
    [Export]
    private Button _returnButton;

    private Node _previousScene;
    private ProcessModeEnum _previousProcessMode;

    public override void _Ready()
    {
        _returnButton.GrabFocus();
        _returnButton.Pressed += CloseMenu;
    }

    public override void _Process(double delta) { }

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
        if (@event.IsActionPressed("ui_cancel"))
        {
            CloseMenu();
            GetViewport().SetInputAsHandled();
        }
    }
}
