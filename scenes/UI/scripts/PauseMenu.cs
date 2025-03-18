using System;
using FunkEngine;
using Godot;

public partial class PauseMenu : Control, IFocusableMenu
{
    [Export]
    public Button[] pauseButtons;

    public IFocusableMenu Prev { get; set; }
    private Control _lastFocus;

    public override void _Ready()
    {
        pauseButtons[0].Pressed += ReturnToPrev;
        pauseButtons[1].Pressed += OpenOptions;
        pauseButtons[2].Pressed += Quit;
        pauseButtons[3].Pressed += QuitToMainMenu;
    }

    private void OpenOptions()
    {
        OptionsMenu optionsMenu = GD.Load<PackedScene>("res://Scenes/UI/Options/OptionsMenu.tscn")
            .Instantiate<OptionsMenu>();
        AddChild(optionsMenu);
        optionsMenu.OpenMenu(this);
    }

    public override void _Input(InputEvent @event)
    {
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
        pauseButtons[0].GrabFocus();
    }

    public void ReturnToPrev()
    {
        Prev.ResumeFocus();
        QueueFree();
    }

    private void Quit()
    {
        GetNode<StageProducer>("/root/StageProducer").TransitionStage(Stages.Quit);
    }

    private void QuitToMainMenu()
    {
        GetNode<StageProducer>("/root/StageProducer").TransitionStage(Stages.Title);
    }
}
