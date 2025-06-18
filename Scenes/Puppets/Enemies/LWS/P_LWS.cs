using System;
using FunkEngine;
using Godot;

public partial class P_LWS : EnemyPuppet
{
    public static new readonly string LoadPath = "res://Scenes/Puppets/Enemies/LWS/P_LWS.tscn";

    public override void _Ready()
    {
        MaxHealth = 80;
        CurrentHealth = MaxHealth;
        BaseMoney = 10;
        InitialNote = (16, 3);
        base._Ready();
        var enemyTween = CreateTween();
        enemyTween.TweenProperty(Sprite, "position", Vector2.Up * 5, 1f).AsRelative();
        enemyTween.TweenProperty(Sprite, "position", Vector2.Down * 5, 1f).AsRelative();
        enemyTween.SetTrans(Tween.TransitionType.Quad);
        enemyTween.SetEase(Tween.EaseType.InOut);
        enemyTween.SetLoops();
        enemyTween.Play();

        BattleEvents = new EnemyEffect[]
        {
            new EnemyEffect(
                this,
                BattleEffectTrigger.OnLoop,
                1,
                (e, eff, val) =>
                {
                    e.BD.RandApplyNote(eff.Owner, InitialNote.NoteId, val);
                }
            ),
        };
    }
}
