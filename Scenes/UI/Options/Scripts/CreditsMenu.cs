using System;
using FunkEngine;
using Godot;

public partial class CreditsMenu : Control, IFocusableMenu
{
    public static readonly string LoadPath = "res://Scenes/UI/Options/Credits.tscn";

    [Export]
    public Control CreditsText;

    [Export]
    public float ScrollSpeed = 60f;

    public float FadeStartY = -500f;
    public float FadeEndY = -1200f;

    [Export]
    public float RestartPositionY = 800f;

    [Export]
    private Button _returnButton;

    public IFocusableMenu Prev { get; set; }

    public override void _Ready()
    {
        if (CreditsText != null)
        {
            CreditsText.Position = new Vector2(CreditsText.Position.X, RestartPositionY);
            FadeEndY = -CreditsText.Size.Y;
        }
        _returnButton.Pressed += ReturnToPrev;
        _returnButton.GrabFocus();
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

    public override void _Process(double delta)
    {
        if (CreditsText == null)
            return;

        CreditsText.Position += Vector2.Up * (float)(ScrollSpeed * delta);

        float alpha = Mathf.Clamp(
            1 - (CreditsText.GlobalPosition.Y - FadeStartY) / (FadeEndY - FadeStartY),
            0,
            1
        );
        CreditsText.Modulate = new Color(1, 1, 1, alpha);

        if (CreditsText.GlobalPosition.Y < -CreditsText.Size.Y)
        {
            CreditsText.Position = new Vector2(CreditsText.Position.X, RestartPositionY);
        }
    }
}
