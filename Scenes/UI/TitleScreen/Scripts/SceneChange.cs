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
    }

    private void OnButtonPressed()
    {
        GetNode<StageProducer>("/root/StageProducer").TransitionStage(ScenePath);
    }
}
