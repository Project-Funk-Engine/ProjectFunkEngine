using System;
using FunkEngine;
using Godot;

public partial class P_TheGWS : EnemyPuppet
{
    public static new readonly string LoadPath = "res://Scenes/Puppets/Enemies/TheGWS/GWS.tscn";

    public override void _Ready()
    {
        MaxHealth = 150;
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

        BattleEvents = new EnemyEffect[]
        {
            new EnemyEffect(
                this,
                BattleEffectTrigger.OnBattleStart,
                1,
                (e, eff, val) =>
                {
                    if (val == 0)
                        return;
                    e.BD.EnemyAddNote(
                        ArrowType.Up,
                        new Beat(3, 1),
                        Scribe.NoteDictionary[10].Clone(),
                        0,
                        eff.Owner
                    );
                    e.BD.EnemyAddNote(
                        ArrowType.Up,
                        new Beat(26),
                        Scribe.NoteDictionary[10].Clone(),
                        0,
                        eff.Owner
                    );
                    eff.Value = 0;
                }
            ),
        };
    }
}
