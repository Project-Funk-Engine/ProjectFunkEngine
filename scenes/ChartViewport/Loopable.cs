using System;
using Godot;

/**
 * @class Loopable
 * @brief A general class fo textures on the chart which should have a position at which point they loop. WIP
 */
public partial class Loopable : TextureRect
{
    public float Bounds = 700f; //px Pos to loop or do something at.
    public float Speed = 5; //px/s

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (Position.X <= -Bounds)
        {
            Loop();
        }
        Position += Speed * Vector2.Left * (float)delta;
    }

    public virtual void Loop()
    {
        Position = new Vector2(Bounds, Position.Y);
    }
}
