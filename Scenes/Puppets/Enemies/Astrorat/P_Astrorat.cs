using FunkEngine;
using Godot;

public partial class P_Astrorat : EnemyPuppet
{
    public static new readonly string LoadPath =
        "res://Scenes/Puppets/Enemies/Astrorat/Astrorat.tscn";

    public override void _Ready()
    {
        MaxHealth = 150;
        CurrentHealth = MaxHealth;
        BaseMoney = 20;
        base._Ready();
        var enemyTween = CreateTween();
        enemyTween.TweenProperty(Sprite, "position", Vector2.Up * 5, 1f).AsRelative();
        enemyTween.TweenProperty(Sprite, "position", Vector2.Down * 5, 1f).AsRelative();
        enemyTween.SetTrans(Tween.TransitionType.Quad);
        enemyTween.SetEase(Tween.EaseType.InOut);
        enemyTween.SetLoops();
        enemyTween.Play();
    }
}
