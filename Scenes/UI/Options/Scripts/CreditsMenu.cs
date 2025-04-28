using FunkEngine;
using Godot;

public partial class CreditsMenu : Control, IFocusableMenu
{
    public static readonly string LoadPath = "res://Scenes/UI/Options/Credits.tscn";

    [Export]
    public Label CreditsText;

    [Export]
    public float ScrollSpeed = 50f;

    [Export]
    public float FadeStartY = 200f;

    [Export]
    public float FadeEndY = 50f;

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
        }
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

    public override void _Process(double delta)
    {
        if (CreditsText == null)
            return;

        Vector2 position = CreditsText.Position;
        position.Y -= (float)(ScrollSpeed * delta);
        CreditsText.Position = position;

        float alpha = 1.0f;
        if (CreditsText.GlobalPosition.Y < FadeStartY)
        {
            alpha = Mathf.Clamp(
                (CreditsText.GlobalPosition.Y - FadeEndY) / (FadeStartY - FadeEndY),
                0,
                1
            );
        }
        CreditsText.Modulate = new Color(1, 1, 1, alpha);

        float bottomScreenY = RestartPositionY;
        float topScreenY = -780;
        float t = Mathf.Clamp(
            (CreditsText.GlobalPosition.Y - topScreenY) / (bottomScreenY - topScreenY),
            0,
            1
        );

        float scaleValue = Mathf.Lerp(0.05f, 1.0f, t);
        CreditsText.Scale = new Vector2(scaleValue, scaleValue);

        if (CreditsText.GlobalPosition.Y + CreditsText.Size.Y * CreditsText.Scale.Y < 0)
        {
            CreditsText.Position = new Vector2(CreditsText.Position.X, RestartPositionY);
        }
    }
}
