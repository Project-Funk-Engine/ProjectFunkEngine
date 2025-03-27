using FunkEngine;
using Godot;

/**
*<summary>This class represents a visual note that scrolls across the screen to be played by the player.</summary>
 */
public partial class NoteArrow : Sprite2D
{ //TextRect caused issues later :)
    public static readonly string LoadPath = "res://Scenes/NoteManager/NoteArrow.tscn";
    private const float LeftBound = -200f;
    public ArrowType Type;
    public int Beat;
    public float LoopOffset;
    public float BeatTime;
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
            OnLoop();
        if (!float.IsNaN(newPos.X))
            Position = newPos;
        if (Position.X < LeftBound)
            ReEnterPool();
    }

    private float GetNewPos()
    {
        float interval = TimeKeeper.ChartLength;
        double relativePosition =
            (TimeKeeper.CurrentTime - BeatTime) / TimeKeeper.LoopLength * TimeKeeper.ChartLength;

        return (float)TimeKeeper.PosMod(-relativePosition - interval / 2, interval)
            - interval / 2
            + LoopOffset;
    }

    private void ReEnterPool()
    {
        Visible = false;
        ProcessMode = ProcessModeEnum.Disabled;
    }

    public void Recycle()
    {
        Visible = true;
        ProcessMode = ProcessModeEnum.Inherit;
        OnLoop();
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
        if (!IsActive)
            return;
        Modulate *= .7f;
        IsActive = false;
    }
}
