using System;
using FunkEngine;
using Godot;

public partial class P_Mushroom : EnemyPuppet
{
    public static new readonly string LoadPath =
        "res://Scenes/Puppets/Enemies/Mushroom/Mushroom.tscn";

    public override void _Ready()
    {
        MaxHealth = 200;
        CurrentHealth = MaxHealth;
        BaseMoney = 10;
        InitialNote = (17, 1);
        base._Ready();
        BattleEvents = new EnemyEffect[]
        {
            new EnemyEffect(
                this,
                BattleEffectTrigger.OnBattleStart,
                1,
                (e, eff, _) =>
                {
                    e.BD.RandApplyNote(eff.Owner, 17, 1);
                }
            ),
        };
    }
}
