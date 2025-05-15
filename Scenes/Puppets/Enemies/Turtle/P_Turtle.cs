using System;
using FunkEngine;
using Godot;

public partial class P_Turtle : EnemyPuppet
{
    public static new readonly string LoadPath = "res://Scenes/Puppets/Enemies/Turtle/Turtle.tscn";

    public override void _Ready()
    {
        MaxHealth = 150;
        CurrentHealth = MaxHealth;
        BaseMoney = 10;
        base._Ready();
        var enemTween = CreateTween();
        enemTween.TweenProperty(Sprite, "position", Vector2.Right * 10, 3f).AsRelative();
        enemTween.TweenProperty(Sprite, "position", Vector2.Left * 10, 3f).AsRelative();
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
                    // take 1/4th of player's energy, and heal that amount
                    int quarterEnergy = (int)e.BD.NPB.GetCurrentBarValue() / 4;
                    e.BD.NPB.IncreaseCharge(-quarterEnergy);
                    this.Heal(quarterEnergy);

                    //gain block based on val
                    e.BD.AddStatus(Targetting.First, StatusEffect.Block, val);
                }
            ),
        };
    }
}
