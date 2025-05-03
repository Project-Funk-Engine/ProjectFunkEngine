using FunkEngine;
using Godot;

public partial class P_BossBlood : EnemyPuppet
{
    public static new readonly string LoadPath =
        "res://Scenes/Puppets/Enemies/BossBlood/Boss1.tscn";

    public override void _Ready()
    {
        MaxHealth = 225;
        CurrentHealth = MaxHealth;
        BaseMoney = 15;
        base._Ready();
        var enemTween = CreateTween();
        enemTween.TweenProperty(Sprite, "position", Vector2.Down * 5, 1f).AsRelative();
        enemTween.TweenProperty(Sprite, "position", Vector2.Up * 5, 1f).AsRelative();
        enemTween.SetTrans(Tween.TransitionType.Spring);
        enemTween.SetEase(Tween.EaseType.In);
        enemTween.SetLoops();
        enemTween.Play();

        BattleEvents = new EnemyEffect[]
        {
            new EnemyEffect(
                this,
                BattleEffectTrigger.OnLoop,
                20,
                (e, eff, val) =>
                {
                    eff.Owner.Heal(val);
                }
            ),
        };
    }
}
