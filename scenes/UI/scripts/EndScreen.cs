using System;
using FunkEngine;
using Godot;

public partial class EndScreen : CanvasLayer
{
    [Export]
    private Button[] buttons;

    [Export] public Label TopLabel;

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
        GetNode<StageProducer>("/root/StageProducer").TransitionStage(Stages.Map);
    }

    private void Quit()
    {
        GetNode<StageProducer>("/root/StageProducer").TransitionStage(Stages.Quit);
    }

    private void QuitToMainMenu()
    {
        GetTree().Paused = false;
        GetNode<StageProducer>("/root/StageProducer").TransitionStage(Stages.Title);
    }
}
