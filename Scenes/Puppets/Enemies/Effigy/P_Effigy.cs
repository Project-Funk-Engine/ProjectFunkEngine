using System;
using FunkEngine;
using Godot;

public partial class P_Effigy : EnemyPuppet
{
    public static new readonly string LoadPath = "res://Scenes/Puppets/Enemies/Effigy/P_Effigy.cs";

    public override void _Ready()
    {
        MaxHealth = 99;
        BaseMoney = 99;
        CurrentHealth = MaxHealth;
        base._Ready();

        BattleEvents = new EnemyEffect[]
        {
            new EnemyEffect(
                this,
                BattleEffectTrigger.OnDamageInstance,
                1,
                (e, eff, val) =>
                {
                    if (e is not BattleDirector.Harbinger.OnDamageInstanceArgs dArgs)
                        return;
                    if (dArgs.Dmg.Target == this)
                        dArgs.Dmg.ModifyDamage(-dArgs.Dmg.Damage + 1);
                    dArgs.Dmg.Source.TakeDamage(new DamageInstance(1, null, dArgs.Dmg.Source));
                }
            ),
            new EnemyEffect(
                this,
                BattleEffectTrigger.OnBattleEnd,
                1,
                (e, eff, val) =>
                {
                    e.BD.DealDamage(Targetting.Player, e.BD.Player.GetCurrentHealth() - 1, null);
                }
            ),
        };
    }
}
