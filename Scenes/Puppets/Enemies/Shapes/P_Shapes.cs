using System;
using FunkEngine;
using Godot;

public partial class P_Shapes : EnemyPuppet
{
    public static new readonly string LoadPath = "res://Scenes/Puppets/Enemies/Shapes/Shapes.tscn";

    public override void _Ready()
    {
        MaxHealth = 150;
        CurrentHealth = MaxHealth;
        BaseMoney = 10;
        base._Ready();
        var enemTween = CreateTween();
        enemTween.TweenProperty(Sprite, "position", Vector2.Right * 10, 3f).AsRelative();
        enemTween.TweenProperty(Sprite, "position", Vector2.Left * 10, 3f).AsRelative();
        enemTween.SetTrans(Tween.TransitionType.Quad);
        enemTween.SetEase(Tween.EaseType.InOut);
        enemTween.SetLoops();
        enemTween.Play();
    }
}
