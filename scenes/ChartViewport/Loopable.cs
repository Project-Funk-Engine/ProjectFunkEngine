using System;
using Godot;

/**
 * @class Loopable
 * @brief A general class fo textures on the chart which should have a position at which point they loop. WIP
 */
public partial class Loopable : Sprite2D
{
    [Export]
    public float LoopOffset = 700f; //px Pos to loop or do something at.

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        Vector2 newPos = Position;
        float interval = TimeKeeper.ChartLength;
        double relativePosition =
            TimeKeeper.CurrentTime / TimeKeeper.LoopLength * TimeKeeper.ChartLength;
        //Loop position over the course of time across a loop
        newPos.X =
            (float)( //Yes I know. https://www.desmos.com/calculator/fkmoqi50ee
                (TimeKeeper.PosMod(-relativePosition - interval / 2, interval) - interval / 2) / 2
            ) + LoopOffset;
        Position = newPos;
    }
}
