using FunkEngine;
using Godot;

public partial class PauseMenu : Control, IFocusableMenu
{
    public static readonly string LoadPath = "res://Scenes/UI/Pause.tscn";

    [Export]
    public Button[] PauseButtons;

    public IFocusableMenu Prev { get; set; }
    private Control _lastFocus;

    public override void _Ready()
    {
        PauseButtons[0].Pressed += ReturnToPrev;
        PauseButtons[1].Pressed += OpenOptions;
        PauseButtons[2].Pressed += Quit;
        PauseButtons[3].Pressed += QuitToMainMenu;
    }

    private void OpenOptions()
    {
        OptionsMenu optionsMenu = GD.Load<PackedScene>(OptionsMenu.LoadPath)
            .Instantiate<OptionsMenu>();
        AddChild(optionsMenu);
        optionsMenu.OpenMenu(this);
    }

    public override void _Input(InputEvent @event)
    {
        if (!GetWindow().HasFocus())
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

    public void ResumeFocus()
    {
        ProcessMode = ProcessModeEnum.Pausable;
        _lastFocus.GrabFocus();
    }

    public void PauseFocus()
    {
        _lastFocus = GetViewport().GuiGetFocusOwner();
        ProcessMode = ProcessModeEnum.Disabled;
    }

    public void OpenMenu(IFocusableMenu prev)
    {
        Prev = prev;
        Prev.PauseFocus();
        PauseButtons[0].GrabFocus();
    }

    public void ReturnToPrev()
    {
        Prev.ResumeFocus();
        QueueFree();
    }

    private void Quit()
    {
        StageProducer.LiveInstance.TransitionStage(Stages.Quit);
    }

    private void QuitToMainMenu()
    {
        StageProducer.LiveInstance.TransitionStage(Stages.Title);
    }
}
