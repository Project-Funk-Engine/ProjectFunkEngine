using System;
using FunkEngine;
using Godot;

/**
 * <summary>Generic button to initiate scene transition on press.</summary>
 */
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
        StageProducer.LiveInstance.TransitionStage(ScenePath);
    }
}
