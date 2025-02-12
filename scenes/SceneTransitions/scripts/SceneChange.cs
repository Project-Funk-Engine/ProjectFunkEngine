using System;
using FunkEngine;
using Godot;

public partial class SceneChange : Button
{
    [Export]
    public Stages ScenePath;

    [Export]
    private bool _startFocused = false;

    public override void _Ready()
    {
        if (_startFocused)
        {
            GrabFocus();
        }
        Pressed += OnButtonPressed;
        GD.Print($"[DEBUG] Scene Path: '{ScenePath}'");
    }

    private void OnButtonPressed()
    {
        GD.Print($"✅ Loading scene: {ScenePath}");
        GetNode<StageProducer>("/root/StageProducer").TransitionStage(ScenePath);
    }
}
