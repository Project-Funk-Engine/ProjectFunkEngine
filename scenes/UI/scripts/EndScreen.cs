using System;
using FunkEngine;
using Godot;

public partial class EndScreen : CanvasLayer
{
    public static readonly string LoadPath = "res://Scenes/UI/EndScreen.tscn";

    [Export]
    private Button[] buttons;

    [Export]
    public Label TopLabel;

    public override void _Ready()
    {
        buttons[0].Pressed += Restart;
        buttons[1].Pressed += QuitToMainMenu;
        buttons[2].Pressed += Quit;
        buttons[0].GrabFocus();
    }

    private void Restart()
    {
        GetTree().Paused = false;
        StageProducer.IsInitialized = false;
        StageProducer.LiveInstance.TransitionStage(Stages.Map);
    }

    private void Quit()
    {
        StageProducer.LiveInstance.TransitionStage(Stages.Quit);
    }

    private void QuitToMainMenu()
    {
        GetTree().Paused = false;
        StageProducer.LiveInstance.TransitionStage(Stages.Title);
    }
}
