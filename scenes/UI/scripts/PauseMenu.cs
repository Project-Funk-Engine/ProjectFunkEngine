using System;
using FunkEngine;
using Godot;

public partial class PauseMenu : Control
{
    [Export]
    public Button[] pauseButtons;

    public override void _Ready()
    {
        pauseButtons[0].Pressed += Resume;
        pauseButtons[1].Pressed += OpenOptions;
        pauseButtons[2].Pressed += Quit;
        pauseButtons[3].Pressed += QuitToMainMenu;
        pauseButtons[0].GrabFocus();
    }

    public override void _Process(double delta)
    {
        if (GetViewport().GuiGetFocusOwner() == null) //TODO: Better method for returning focus
        {
            pauseButtons[0].GrabFocus();
        }
    }

    private void OpenOptions()
    {
        OptionsMenu optionsMenu = GD.Load<PackedScene>("res://scenes/Options/OptionsMenu.tscn")
            .Instantiate<OptionsMenu>();
        AddChild(optionsMenu);
        optionsMenu.OpenMenu(this);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("Pause"))
        {
            Resume();
            GetViewport().SetInputAsHandled();
        }
    }

    private void Resume()
    {
        GetTree().Paused = false;
        QueueFree(); //Hacky and shortsighted (probably?)
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
