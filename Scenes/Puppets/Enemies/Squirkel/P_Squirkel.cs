using System;
using FunkEngine;
using Godot;

public partial class P_Squirkel : EnemyPuppet
{
    public static new readonly string LoadPath =
        "res://Scenes/Puppets/Enemies/Squirkel/Squirkel.tscn";

    public override void _Ready()
    {
        MaxHealth = 90;
        CurrentHealth = MaxHealth;
        BaseMoney = 10;
        base._Ready();

        var tween = CreateTween();
        tween.TweenCallback(Callable.From(() => Sprite.FlipH = true));
        tween.TweenProperty(Sprite, "position", Vector2.Right * 50, 0.5f).AsRelative();
        tween.TweenCallback(Callable.From(() => Sprite.RotationDegrees = -90));
        tween.TweenProperty(Sprite, "position", Vector2.Up * 50, 1f).AsRelative();
        tween.TweenCallback(Callable.From(() => Sprite.FlipH = false));
        tween.TweenCallback(Callable.From(() => Sprite.RotationDegrees = -90));
        tween.TweenProperty(Sprite, "position", Vector2.Down * 50, 0.5f).AsRelative();
        tween.TweenCallback(Callable.From(() => Sprite.RotationDegrees = 0));
        tween.TweenProperty(Sprite, "position", Vector2.Left * 50, 0.5f).AsRelative();
        tween.SetTrans(Tween.TransitionType.Bounce);
        tween.SetEase(Tween.EaseType.InOut);
        tween.SetLoops();
        tween.Play();
    }
}
