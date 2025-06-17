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
        CurrentHealth = 40;
        MaxHealth = 40;
        BaseMoney = 5;
        base._Ready();

        BattleEvents = new EnemyEffect[]
        {
            new EnemyEffect(
                this,
                BattleEffectTrigger.OnBattleStart,
                1,
                (e, _, _) =>
                {
                    _tutorialInstance = Toriel.AttachNewToriel(e.BD);
                    _tutorialInstance.IntroDialogue();
                },
                null
            ),
            new EnemyEffect(
                this,
                BattleEffectTrigger.OnDamageInstance,
                2,
                (e, eff, _) =>
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
                },
                "STRAWMAN_EFFECT1"
            ),
            new EnemyEffect(
                this,
                BattleEffectTrigger.OnLoop,
                1,
                (e, _, _) =>
                {
                    if (e is not BattleDirector.Harbinger.LoopEventArgs lArgs)
                        return;
                    if (lArgs.Loop == 1)
                    {
                        _tutorialInstance.LoopDialogue();
                    }
                },
                null
            ),
            new EnemyEffect(
                this,
                BattleEffectTrigger.NoteHit,
                1,
                (e, eff, _) =>
                {
                    if (eff.Value == 0)
                        return;
                    if (e is not BattleDirector.Harbinger.NoteHitArgs nArgs)
                        return;
                    if (!nArgs.BD.NPB.CanPlaceNote() || TimeKeeper.LastBeat.Loop < 1)
                        return;
                    eff.Value = 0;
                    _tutorialInstance.PlaceDialogue1();
                },
                null
            ),
            new EnemyEffect(
                this,
                BattleEffectTrigger.NotePlaced,
                1,
                (_, _, _) =>
                {
                    _tutorialInstance.CallDeferred(nameof(_tutorialInstance.OnPlaceDialogue1));
                },
                null
            ),
            new EnemyEffect(
                this,
                BattleEffectTrigger.OnDamageInstance,
                1,
                (e, _, _) =>
                {
                    if (e is not BattleDirector.Harbinger.OnDamageInstanceArgs dArgs)
                        return;
                    if (
                        dArgs.Dmg.Target == this
                        && dArgs.Dmg.Target.GetCurrentHealth() <= dArgs.Dmg.Damage
                    )
                    {
                        StageProducer.UpdatePersistantValues(
                            StageProducer.PersistKeys.TutorialDone,
                            1
                        );
                        SteamWhisperer.PopAchievement("tutorial");
                    }
                },
                null
            ),
        };
    }
}
