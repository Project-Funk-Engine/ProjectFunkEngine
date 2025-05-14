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
        BaseMoney = 5;
        base._Ready();

        var tween = CreateTween();

        // 1. Flip to face right
        tween.TweenCallback(Callable.From(() => Sprite.FlipH = true));

        // 2. Move right
        tween.TweenProperty(Sprite, "position", Vector2.Right * 50, 0.5f).AsRelative();

        // 3. Rotate to face up (-90 degrees from right-facing)
        tween.TweenCallback(Callable.From(() => Sprite.RotationDegrees = -90));

        // 4. Move up
        tween.TweenProperty(Sprite, "position", Vector2.Up * 50, 1f).AsRelative();

        // 8. Flip to face left
        tween.TweenCallback(Callable.From(() => Sprite.FlipH = false));

        // 5. Rotate to face down (90 degrees from right-facing)
        tween.TweenCallback(Callable.From(() => Sprite.RotationDegrees = -90));

        // 6. Move down
        tween.TweenProperty(Sprite, "position", Vector2.Down * 50, 1f).AsRelative();

        // 7. Rotate to face right again
        tween.TweenCallback(Callable.From(() => Sprite.RotationDegrees = 0));

        // 9. Move left
        tween.TweenProperty(Sprite, "position", Vector2.Left * 50, 0.5f).AsRelative();

        // Optional: Loop
        tween.SetTrans(Tween.TransitionType.Bounce);
        tween.SetEase(Tween.EaseType.InOut);
        tween.SetLoops(); // Loops infinitely
    }
}
