using FunkEngine;
using Godot;

public partial class EndScreen : CanvasLayer
{
    public static readonly string LoadPath = "res://Scenes/UI/EndScreen.tscn";

    [Export]
    private Button[] _buttons;

    [Export]
    public Label TopLabel;

    public override void _Ready()
    {
        _buttons[0].Pressed += Restart;
        _buttons[1].Pressed += QuitToMainMenu;
        _buttons[2].Pressed += Quit;
        _buttons[0].GrabFocus();
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
