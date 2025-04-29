using System;
using Godot;

public partial class P_Parasifly : EnemyPuppet
{
    public static new readonly string LoadPath =
        "res://Scenes/Puppets/Enemies/Parasifly/Parasifly.tscn";

    public override void _Ready()
    {
        CurrentHealth = 50;
        MaxHealth = 50;
        BaseMoney = 10;
        base._Ready();
        var enemTween = CreateTween();
        enemTween.TweenProperty(Sprite, "position", Vector2.Down * 2, 2f).AsRelative();
        enemTween.TweenProperty(Sprite, "position", Vector2.Up * 2, 2f).AsRelative();
        enemTween.SetTrans(Tween.TransitionType.Linear);
        enemTween.SetEase(Tween.EaseType.In);
        enemTween.SetLoops();
        enemTween.Play();
    }
}
