using System;
using Godot;

public partial class P_TheGWS : EnemyPuppet
{
    public static new readonly string LoadPath = "res://Scenes/Puppets/Enemies/TheGWS/GWS.tscn";

    public override void _Ready()
    {
        MaxHealth = 300;
        CurrentHealth = MaxHealth;
        BaseMoney = 10;
        base._Ready();
        var enemTween = CreateTween();
        enemTween.TweenProperty(Sprite, "position", Vector2.Down * 10, 3f).AsRelative();
        enemTween.TweenProperty(Sprite, "position", Vector2.Up * 10, 3f).AsRelative();
        enemTween.SetTrans(Tween.TransitionType.Quad);
        enemTween.SetEase(Tween.EaseType.InOut);
        enemTween.SetLoops();
        enemTween.Play();
    }
}
