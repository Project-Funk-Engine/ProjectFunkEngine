using FunkEngine;
using Godot;

/**
*<summary>This class represents a visual note that scrolls across the screen to be played by the player.</summary>
 */
public partial class NoteArrow : Sprite2D
{ //TextRect caused issues later :)
    public static readonly string LoadPath = "res://Scenes/NoteManager/NoteArrow.tscn";
    private const float LeftBound = -200f;

    public Beat Beat;
    public ArrowType Type;
    public float LoopOffset;
    public float BeatTime;
    public bool IsActive = true;
    public Note NoteRef;

    [Export]
    public Sprite2D OutlineSprite;

    [Export]
    public Sprite2D IconSprite;

    public override void _Ready()
    {
        ZIndex = 1;
    }

    public void Init(ArrowData parentArrowData, Beat beat, Note note)
    {
        Beat = beat;
        Type = parentArrowData.Type;

        Position = new Vector2(GetNewPosX(), parentArrowData.Node.GlobalPosition.Y);
        RotationDegrees = parentArrowData.Node.RotationDegrees;
        IconSprite.Texture = note.Texture;
        IconSprite.Rotation = -Rotation;
    }

    public override void _Process(double delta)
    {
        CheckMissed();
        Vector2 newPos = Position;
        newPos.X = GetNewPosX();
        if (!float.IsNaN(newPos.X))
            Position = newPos;
        if (Position.X < LeftBound)
            ReEnterPool();
    }

    public void NoteHit()
    {
        if (!IsActive)
            return;
        Modulate *= .7f;
        IsActive = false;
    }

    public delegate void MissedEventHandler(NoteArrow note);
    public event MissedEventHandler Missed;

    private void CheckMissed()
    {
        if (!IsActive || !(TimeKeeper.LastBeat - Beat > Beat.One))
            return;
        NoteHit();
        Missed?.Invoke(this);
    }

    private float GetNewPosX()
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
        if (!IsActive)
        {
            Modulate /= .7f;
        }
        IsActive = true;
    }
}
