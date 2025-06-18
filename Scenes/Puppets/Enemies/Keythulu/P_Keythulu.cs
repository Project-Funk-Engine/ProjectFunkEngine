using System;
using FunkEngine;
using Godot;

public partial class P_Keythulu : EnemyPuppet
{
    [Export]
    private Node2D _effectSprite;

    public static new readonly string LoadPath =
        "res://Scenes/Puppets/Enemies/Keythulu/Keythulu.tscn";

    public override void _Ready()
    {
        MaxHealth = 500;
        CurrentHealth = MaxHealth;
        BaseMoney = 80;
        base._Ready();

        _effectSprite.Visible = false;

        var enemTween = CreateTween();
        enemTween.TweenProperty(Sprite, "position", Vector2.Left * 10, 1f).AsRelative();
        enemTween.TweenProperty(Sprite, "position", Vector2.Right * 10, 1f).AsRelative();
        enemTween.SetTrans(Tween.TransitionType.Bounce);
        enemTween.SetEase(Tween.EaseType.InOut);
        enemTween.SetLoops();
        enemTween.Play();

        BattleEvents = new EnemyEffect[]
        {
            new EnemyEffect(
                this,
                BattleEffectTrigger.OnBattleStart,
                6,
                (e, eff, val) =>
                {
                    e.BD.AddStatus(Targetting.Player, StatusEffect.MindCrush, val);
                }
            ),
            new EnemyEffect(
                this,
                BattleEffectTrigger.OnLoop,
                3,
                (e, eff, val) =>
                {
                    _effectSprite.Position = Vector2.Zero;
                    _effectSprite.Visible = true;
                    var effectTween = CreateTween();
                    effectTween
                        .TweenProperty(_effectSprite, "position", Vector2.Left * 2000, 6f)
                        .AsRelative();
                    effectTween.TweenCallback(
                        Callable.From(() =>
                        {
                            _effectSprite.Position = Vector2.Zero;
                            _effectSprite.Visible = false;
                        })
                    );
                }
            ),
            new EnemyEffect(
                this,
                BattleEffectTrigger.OnDamageInstance,
                1,
                (e, eff, val) =>
                {
                    if (
                        StageProducer.Config.RoomType == Stages.Custom
                        || e is not BattleDirector.Harbinger.OnDamageInstanceArgs dArgs
                    )
                        return;
                    if (
                        dArgs.Dmg.Target == this
                        && dArgs.Dmg.Target.GetCurrentHealth() <= dArgs.Dmg.Damage
                    )
                    {
                        SteamWhisperer.PopAchievement("actTwoComp");
                        SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.HasWon, true);
                    }
                }
            ),
        };
    }
}
