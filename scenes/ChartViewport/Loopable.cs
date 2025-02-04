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

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        Vector2 newPos = Position;
        //Loop position over the course of time across a loop
        newPos.X =
            (float)(
                (-TimeKeeper.CurrentTime / TimeKeeper.LoopLength * TimeKeeper.ChartLength)
                % TimeKeeper.ChartLength
                / 2
            ) + Bounds;
        Position = newPos;
    }
}
