using System;
using FunkEngine;
using Godot;

public partial class P_Spider : EnemyPuppet
{
    public static new readonly string LoadPath = "res://Scenes/Puppets/Enemies/Spider/Spider.tscn";

    public override void _Ready()
    {
        MaxHealth = 60;
        CurrentHealth = MaxHealth;
        BaseMoney = 5;
        InitialNote = (15, 2);
        base._Ready();

        BattleEvents = new EnemyEffect[]
        {
            new EnemyEffect(
                this,
                BattleEffectTrigger.OnBattleStart,
                1,
                (e, eff, _) =>
                {
                    e.BD.RandApplyNote(eff.Owner, 15, 1);
                },
                null
            ),
        };
    }
}
