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
        BaseMoney = 20;
        InitialNote = (10, 7);
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
                BattleEffectTrigger.OnLoop,
                1,
                (e, eff, val) =>
                {
                    e.BD.RandApplyNote(eff.Owner, 10, 2);
                    if (val == 0)
                        return;
                    e.BD.EnemyAddNote(
                        ArrowType.Down,
                        new Beat(37),
                        Scribe.NoteDictionary[10].Clone(),
                        0,
                        eff.Owner
                    );
                    e.BD.EnemyAddNote(
                        ArrowType.Left,
                        new Beat(51),
                        Scribe.NoteDictionary[10].Clone(),
                        0,
                        eff.Owner
                    );
                    e.BD.EnemyAddNote(
                        ArrowType.Right,
                        new Beat(59),
                        Scribe.NoteDictionary[10].Clone(),
                        0,
                        eff.Owner
                    );
                    eff.Value = 0;
                },
                "GWS_EFFECT1"
            ),
        };
    }
}
