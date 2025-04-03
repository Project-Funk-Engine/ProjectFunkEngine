using FunkEngine;
using Godot;

public partial class HowToPlay : Node2D, IFocusableMenu
{
    public static readonly string LoadPath = "res://Scenes/UI/Options/HowToPlay.tscn";

    [Export]
    private Button _returnButton;

    public IFocusableMenu Prev { get; set; }

    public override void _Ready()
    {
        _returnButton.Pressed += ReturnToPrev;
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

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            ReturnToPrev();
            GetViewport().SetInputAsHandled();
        }
    }
}
