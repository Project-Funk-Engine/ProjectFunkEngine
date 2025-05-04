using System;
using FunkEngine;
using Godot;

public partial class P_Strawman : EnemyPuppet
{
    public static new readonly string LoadPath =
        "res://Scenes/Puppets/Enemies/Strawman/Strawman.tscn";

    private static Toriel _tutorialInstance;

    public override void _Ready()
    {
        CurrentHealth = 15;
        MaxHealth = 15;
        BaseMoney = 1;
        base._Ready();

        BattleEvents = new EnemyEffect[]
        {
            new EnemyEffect(
                this,
                BattleEffectTrigger.OnBattleStart,
                1,
                (e, eff, val) =>
                {
                    _tutorialInstance = Toriel.AttachNewToriel(e.BD);
                    _tutorialInstance.IntroDialogue();
                }
            ),
            new EnemyEffect(
                this,
                BattleEffectTrigger.OnDamageInstance,
                2,
                (e, eff, val) =>
                {
                    if (e is not BattleDirector.Harbinger.OnDamageInstanceArgs dArgs)
                        return;
                    if (
                        dArgs.Dmg.Target == dArgs.BD.Player
                        && dArgs.Dmg.Target.GetCurrentHealth() <= dArgs.Dmg.Damage + 1
                    )
                    {
                        dArgs.Dmg.Target.Heal(999);
                        if (eff.Value == 0)
                            return;
                        _tutorialInstance.NoDying();
                        eff.Value--;
                    }
                }
            ),
            new EnemyEffect(
                this,
                BattleEffectTrigger.OnLoop,
                1,
                (e, eff, val) =>
                {
                    if (e is not BattleDirector.Harbinger.LoopEventArgs lArgs)
                        return;
                    if (lArgs.Loop == 1)
                    {
                        _tutorialInstance.LoopDialogue();
                    }
                }
            ),
            new EnemyEffect(
                this,
                BattleEffectTrigger.NoteHit,
                1,
                (e, eff, val) =>
                {
                    if (eff.Value == 0)
                        return;
                    if (e is not BattleDirector.Harbinger.NoteHitArgs nArgs)
                        return;
                    if (!nArgs.BD.NPB.CanPlaceNote() || TimeKeeper.LastBeat.Loop < 1)
                        return;
                    eff.Value = 0;
                    _tutorialInstance.PlaceDialogue1();
                }
            ),
            new EnemyEffect(
                this,
                BattleEffectTrigger.NotePlaced,
                1,
                (e, eff, val) =>
                {
                    _tutorialInstance.CallDeferred(nameof(_tutorialInstance.OnPlaceDialogue1));
                }
            ),
            new EnemyEffect(
                this,
                BattleEffectTrigger.OnDamageInstance,
                1,
                (e, eff, val) =>
                {
                    if (e is not BattleDirector.Harbinger.OnDamageInstanceArgs dArgs)
                        return;
                    if (
                        dArgs.Dmg.Target == this
                        && dArgs.Dmg.Target.GetCurrentHealth() <= dArgs.Dmg.Damage
                    )
                    {
                        SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.FirstTime, false);
                    }
                }
            ),
        };
    }
}
