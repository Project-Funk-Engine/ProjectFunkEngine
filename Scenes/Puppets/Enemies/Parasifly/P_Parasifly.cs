using FunkEngine;
using Godot;

public partial class P_Parasifly : EnemyPuppet
{
    public static new readonly string LoadPath =
        "res://Scenes/Puppets/Enemies/Parasifly/Parasifly.tscn";

    public override void _Ready()
    {
        MaxHealth = 100;
        CurrentHealth = MaxHealth;
        BaseMoney = 7;
        InitialNote = (13, 2);
        base._Ready();
        var enemTween = CreateTween();
        enemTween.TweenProperty(Sprite, "position", Vector2.Down * 2, 2f).AsRelative();
        enemTween.TweenProperty(Sprite, "position", Vector2.Up * 2, 2f).AsRelative();
        enemTween.SetTrans(Tween.TransitionType.Linear);
        enemTween.SetEase(Tween.EaseType.In);
        enemTween.SetLoops();
        enemTween.Play();

        BattleEvents = new EnemyEffect[]
        {
            new EnemyEffect(
                this,
                BattleEffectTrigger.OnBattleStart,
                1,
                (e, eff, _) =>
                {
                    e.BD.RandApplyNote(eff.Owner, 13, 1);
                }
            ),
            new EnemyEffect(
                this,
                BattleEffectTrigger.OnDamageInstance,
                3,
                (e, eff, val) =>
                {
                    if (
                        val <= 0
                        || e is not BattleDirector.Harbinger.OnDamageInstanceArgs dArgs
                        || dArgs.Dmg.Target != eff.Owner
                        || dArgs.Dmg.Damage < dArgs.Dmg.Target.GetCurrentHealth()
                    )
                        return;
                    e.BD.AddStatus(Targetting.All, StatusEffect.Block, val);
                    eff.Value = 0;
                }
            ),
        };
    }
}
