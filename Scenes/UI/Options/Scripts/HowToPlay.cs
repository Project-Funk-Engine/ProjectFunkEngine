using FunkEngine;
using Godot;

public partial class HowToPlay : Node2D, IFocusableMenu
{
    public static readonly string LoadPath = "res://Scenes/UI/Options/HowToPlay.tscn";

    [Export]
    private Button _returnButton;

    [Export]
    private Button _tutorialButton;

    public IFocusableMenu Prev { get; set; }

    public override void _Ready()
    {
        _returnButton.Pressed += ReturnToPrev;
        _tutorialButton.Pressed += DoTutorial;
        _tutorialButton.Visible = !StageProducer.IsInitialized;
    }

    public void ResumeFocus()
    {
        ProcessMode = ProcessModeEnum.Inherit;
        _returnButton.GrabFocus();
    }

    public void PauseFocus()
    {
        ProcessMode = ProcessModeEnum.Disabled;
    }

    public void OpenMenu(IFocusableMenu prev)
    {
        Prev = prev;
        Prev.PauseFocus();
        _returnButton.GrabFocus();
    }

    public void ReturnToPrev()
    {
        Prev.ResumeFocus();
        QueueFree();
    }

    private void DoTutorial()
    {
        SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.FirstTime, true);
        StageProducer.LiveInstance.TransitionStage(Stages.Map);
    }

    public override void _Input(InputEvent @event)
    {
        if (ControlSettings.IsOutOfFocus(this))
        {
            GetViewport().SetInputAsHandled();
            return;
        }
        if (@event.IsActionPressed("ui_cancel"))
        {
            ReturnToPrev();
            GetViewport().SetInputAsHandled();
        }
    }
}
