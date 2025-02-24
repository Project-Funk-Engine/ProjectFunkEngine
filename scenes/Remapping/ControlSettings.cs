using System;
using Godot;

public partial class ControlSettings : Node2D
{
    public override void _Ready()
    {
        GetNode<Button>("Panel/WASDButton").Connect("pressed", Callable.From(OnWASDButtonPressed));
        GetNode<Button>("Panel/ArrowButton")
            .Connect("pressed", Callable.From(OnArrowButtonPressed));
        GetNode<Button>("Panel/QWERTButton")
            .Connect("pressed", Callable.From(OnQWERTButtonPressed));
    }

    private void OnWASDButtonPressed()
    {
        GetNode<Label>("Panel/Label").Text = "WASD Selected";
        ProjectSettings.SetSetting("game/input_scheme", "WASD");
        ProjectSettings.Save();
    }

    private void OnArrowButtonPressed()
    {
        GetNode<Label>("Panel/Label").Text = "Arrow Selected";
        ProjectSettings.SetSetting("game/input_scheme", "ARROWS");
        ProjectSettings.Save();
    }

    private void OnQWERTButtonPressed()
    {
        GetNode<Label>("Panel/Label").Text = "QWERT Selected";
        ProjectSettings.SetSetting("game/input_scheme", "QWERT");
        ProjectSettings.Save();
    }
}
