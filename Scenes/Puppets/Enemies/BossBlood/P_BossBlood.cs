using FunkEngine;
using Godot;

public partial class P_BossBlood : EnemyPuppet
{
    public static new readonly string LoadPath =
        "res://Scenes/Puppets/Enemies/BossBlood/Boss1.tscn";

    public override void _Ready()
    {
        MaxHealth = 250;
        CurrentHealth = MaxHealth;
        BaseMoney = 50;
        InitialNote = (14, 3);
        base._Ready();
        var enemTween = CreateTween();
        enemTween.TweenProperty(Sprite, "position", Vector2.Down * 5, 1f).AsRelative();
        enemTween.TweenProperty(Sprite, "position", Vector2.Up * 5, 1f).AsRelative();
        enemTween.SetTrans(Tween.TransitionType.Spring);
        enemTween.SetEase(Tween.EaseType.In);
        enemTween.SetLoops();
        enemTween.Play();

        const int effect1Val = 30;

        BattleEvents = new EnemyEffect[]
        {
            new EnemyEffect(
                this,
                BattleEffectTrigger.OnLoop,
                effect1Val,
                (e, eff, val) =>
                {
                    eff.Owner.Heal(val);
                    e.BD.RandApplyNote(eff.Owner, 14, 1);
                },
                string.Format(
                    TranslationServer.Translate("BOSSBLOOD_EFFECT1"),
                    effect1Val.ToString()
                )
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
                        SteamWhisperer.PopAchievement("actOneComp");
                    }
                },
                null
            ),
        };
    }
}
