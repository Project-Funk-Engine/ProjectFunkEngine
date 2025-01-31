using System;
using Godot;

/**
 * @class Loopable
 * @brief A general class fo textures on the chart which should have a position at which point they loop. WIP
 */
public partial class Loopable : TextureRect
{
    [Export]
    public float Bounds = 700f; //px Pos to loop or do something at.
    public float Speed = 5; //px/s

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (Position.X <= -Bounds)
        {
            //Loop();
        }
        Vector2 newPos = Position;
        newPos.X =
            (float)(
                (-TimeKeeper.CurrentTime / TimeKeeper.LoopLength * TimeKeeper.ChartLength)
                % TimeKeeper.ChartLength
                / 2
            ) + Bounds;
        Position = newPos;
    }

    public void Loop()
    {
        Position = new Vector2(Bounds, Position.Y);
    }
}
