using System;
using Godot;

/**
 * <summary>A general class of textures on the chart which should have a position at which point they loop.</summary>
 */
public partial class Loopable : Sprite2D
{
    [Export]
    public float LoopOffset = 700f; //px Pos to loop or do something at.

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
        if (!float.IsNaN(newPos.X))
            Position = newPos;
    }
}
