using Godot;

/**
 * @class NoteArrow
 * @brief This class represents a visual note that scrolls across the screen to be played by the player. WIP
 */
public partial class NoteArrow : Loopable
{ //TextRect makes it easier to resize to something specific, but could cause issues later. :)
    public enum ArrowType
    {
        Up,
        Down,
        Left,
        Right,
    }

    //TODO: This is for a time based implementation. We need to find a way to make this exact.
    //private float _expectedHitTime = 0;

    public void Init(NoteManager.ArrowData parentArrowData, float speed, float hitTime)
    {
        ZIndex = 1;
        PivotOffset = Size / 2;

        Speed = speed;
        SelfModulate = parentArrowData.Color;
        SetPosition(new Vector2(0, parentArrowData.Node.GlobalPosition.Y - Size.Y / 2));
        RotationDegrees = parentArrowData.Node.RotationDegrees;
    }

    public override void _Process(double delta)
    {
        SetPosition(new Vector2(Position.X - Speed * (float)delta, Position.Y));

        if (Position.X >= Bounds)
            Loop();
    }

    public void NoteHit()
    {
        Visible = false;
    }

    public override void Loop()
    {
        GD.Print("Queue Looped Note Here");
    }
}
