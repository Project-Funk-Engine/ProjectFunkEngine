using FunkEngine;
using Godot;

/**
*<summary>This class represents a visual note that scrolls across the screen to be played by the player.</summary>
 */
public partial class NoteArrow : Sprite2D
{ //TextRect caused issues later :)
    public ArrowType Type;
    public int Beat;
    public float Bounds;
    public bool IsActive = true;
    public Note NoteRef;

    [Export]
    public Sprite2D OutlineSprite;

    [Export]
    public Sprite2D IconSprite;

    public void Init(ArrowData parentArrowData, int beat, Note note)
    {
        ZIndex = 1;

        Type = parentArrowData.Type;
        Beat = beat;

        Position = new Vector2(GetNewPos(), parentArrowData.Node.GlobalPosition.Y);
        RotationDegrees = parentArrowData.Node.RotationDegrees;
        IconSprite.Texture = note.Texture;
        IconSprite.Rotation = -Rotation;
    }

    public override void _Process(double delta)
    {
        Vector2 newPos = Position;
        newPos.X = GetNewPos();
        if (newPos.X > Position.X)
        {
            OnLoop();
        }
        Position = newPos;
    }

    private float GetNewPos()
    {
        return (float)(
                (-TimeKeeper.CurrentTime / TimeKeeper.LoopLength * TimeKeeper.ChartLength)
                % TimeKeeper.ChartLength
                / 2
            ) + Bounds;
    }

    private void OnLoop()
    {
        if (!IsActive)
        {
            Modulate /= .7f;
        }
        IsActive = true;
    }

    public void NoteHit()
    {
        Modulate *= .7f;
        IsActive = false;
    }
}
