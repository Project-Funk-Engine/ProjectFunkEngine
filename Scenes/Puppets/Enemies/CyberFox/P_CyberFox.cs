using System;
using FunkEngine;
using Godot;

public partial class P_CyberFox : EnemyPuppet
{
    public static new readonly string LoadPath =
        "res://Scenes/Puppets/Enemies/CyberFox/CyberFox.tscn";

    [Export]
    private GlitchScript _effectNode;

    public override void _Ready()
    {
        MaxHealth = 130;
        CurrentHealth = MaxHealth;
        BaseMoney = 20;
        base._Ready();
        var enemTween = CreateTween();
        enemTween.TweenProperty(Sprite, "position", Vector2.Right * 10, 0.5f).AsRelative();
        enemTween.TweenProperty(Sprite, "position", Vector2.Up * 5, 0.25f).AsRelative();
        enemTween.TweenProperty(Sprite, "position", Vector2.Down * 5, 0.25f).AsRelative();
        enemTween.TweenProperty(Sprite, "position", Vector2.Left * 10, 0.5f).AsRelative();
        enemTween.TweenProperty(Sprite, "position", Vector2.Up * 5, 0.25f).AsRelative();
        enemTween.TweenProperty(Sprite, "position", Vector2.Down * 5, 0.25f).AsRelative();
        enemTween.TweenProperty(Sprite, "position", Vector2.Up * 5, 0.25f).AsRelative();
        enemTween.TweenProperty(Sprite, "position", Vector2.Down * 5, 0.25f).AsRelative();
        enemTween.SetTrans(Tween.TransitionType.Bounce);
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
                    e.BD.AddStatus(Targetting.First, StatusEffect.Dodge, 1);
                    _effectNode.TriggerGlitch(1f);
                }
            ),
        };
    }
}
