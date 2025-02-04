using System;
using Godot;

public partial class TextParticle : Label
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Tween tween = GetTree().CreateTween();
        Position += Vector2.Left * (GD.Randf() * 40 - 20);
        tween.SetTrans(Tween.TransitionType.Quad);
        tween.SetEase(Tween.EaseType.Out);
        tween.TweenProperty(this, "position", Position + Vector2.Up * 10, .5f);
        tween.TweenProperty(this, "position", Position + Vector2.Down * 20, .5f);
        tween.SetParallel();
        tween.TweenProperty(this, "modulate:a", 0, 1f);
        tween.SetParallel(false);
        tween.TweenCallback(Callable.From(QueueFree));
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }
}
