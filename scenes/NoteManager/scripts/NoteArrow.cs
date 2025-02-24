using FunkEngine;
using Godot;

/**
 * @class NoteArrow
 * @brief This class represents a visual note that scrolls across the screen to be played by the player. WIP
 */
public partial class NoteArrow : Sprite2D
{ //TextRect caused issues later :)
    public ArrowType Type;
    public int Beat;
    public float Bounds;
    public bool IsActive;
    public Note NoteRef;

    [Export]
    public Sprite2D OutlineSprite;

    public void Init(ArrowData parentArrowData, int beat, string noteName)
    {
        ZIndex = 1;

        Type = parentArrowData.Type;
        Beat = beat;

        Position += Vector2.Down * (parentArrowData.Node.GlobalPosition.Y);
        RotationDegrees = parentArrowData.Node.RotationDegrees;

        if (noteName == "EnemyBase")
            return;
        Sprite2D child = GetNode<Sprite2D>(noteName);
        if (child != null)
        {
            child.Visible = true;
            child.RotationDegrees = -parentArrowData.Node.RotationDegrees;
        }
    }

    public override void _Process(double delta)
    {
        Vector2 newPos = Position;
        newPos.X =
            (float)(
                (-TimeKeeper.CurrentTime / TimeKeeper.LoopLength * TimeKeeper.ChartLength)
                % TimeKeeper.ChartLength
                / 2
            ) + Bounds;
        if (newPos.X > Position.X)
        {
            OnLoop();
        }
        Position = newPos;
    }

    public void OnLoop()
    {
        Visible = true;
        IsActive = true;
    }

    public void NoteHit()
    {
        Visible = false;
        IsActive = false;
    }
}
