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
        };
    }
}
