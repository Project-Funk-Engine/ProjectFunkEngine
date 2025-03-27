using System;
using Godot;

/**
 * <summary>A general class of textures on the chart which should have a position at which point they loop around.
 * Mostly a study for modulo arithmetic.</summary>
 */
public partial class Loopable : Sprite2D
{
    [Export]
    public float LoopOffset = 700f; //px Pos to loop or do something at.

    public override void _Process(double delta)
    {
        UpdatePos();
    }

    //Loop position over the course of time across a loop
    private void UpdatePos()
    {
        Vector2 newPos = Position;
        float interval = TimeKeeper.ChartLength;
        double relativePosition =
            TimeKeeper.CurrentTime / TimeKeeper.LoopLength * TimeKeeper.ChartLength;
        newPos.X =
            //Yes I know. Hard to parse math. https://www.desmos.com/calculator/fkmoqi50ee
            //(velocity*Pos - (horizontal shift)) % interval - (vertical shift)
            //We want something to be at x=0 at a specific time/beat and having it drift cleanly off and on screen
            (float)TimeKeeper.PosMod(-relativePosition - interval / 2, interval)
            - interval / 2
            + LoopOffset;
        if (!float.IsNaN(newPos.X))
            Position = newPos;
    }
}
