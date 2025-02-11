using System;
using FunkEngine;
using Godot;

public partial class SceneChange : Button
{
    [Export]
    public Stages ScenePath;

    public override void _Ready()
    {
        Pressed += OnButtonPressed;
        GD.Print($"[DEBUG] Scene Path: '{ScenePath}'");
    }

    private void OnButtonPressed()
    {
        if (ScenePath == Stages.Quit)
        {
            GD.Print("Exiting game");
            GetTree().Quit();
            return;
        }

        GD.Print($"✅ Loading scene: {ScenePath}");
        GetNode<StageProducer>("/root/StageProducer").TransitionStage(ScenePath);
    }
}
