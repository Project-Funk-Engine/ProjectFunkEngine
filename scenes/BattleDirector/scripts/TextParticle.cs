using System;
using Godot;

public partial class TextParticle : Label
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Tween tween = CreateTween();
        ZIndex = 2;
        Position += Vector2.Left * (GD.Randf() * 40 - 20);
        tween.SetTrans(Tween.TransitionType.Elastic);
        tween.SetEase(Tween.EaseType.Out);
        tween.TweenProperty(this, "position", Position + Vector2.Up * 10, .25f).AsRelative();
        tween.TweenProperty(this, "position", Position + Vector2.Down * 20, .1f).AsRelative();
        tween.SetParallel();
        tween.SetTrans(Tween.TransitionType.Quad);
        tween.TweenProperty(this, "modulate:a", 0, .15f);
        tween.SetParallel(false);
        tween.TweenCallback(Callable.From(QueueFree));
    }
}
