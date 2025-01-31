using Godot;

/**
 * @class NoteArrow
 * @brief This class represents a visual note that scrolls across the screen to be played by the player. WIP
 */
public partial class NoteArrow : Sprite2D
{ //TextRect caused issues later :)
    public enum ArrowType
    {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3,
    }

    public float Bounds;

    public void Init(NoteManager.ArrowData parentArrowData)
    {
        ZIndex = 1;

        SelfModulate = parentArrowData.Color;
        Position += Vector2.Down * (parentArrowData.Node.GlobalPosition.Y);
        RotationDegrees = parentArrowData.Node.RotationDegrees;
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
            Visible = true;
        }
        Position = newPos;
    }

    public void NoteHit()
    {
        Visible = false;
    }
}
