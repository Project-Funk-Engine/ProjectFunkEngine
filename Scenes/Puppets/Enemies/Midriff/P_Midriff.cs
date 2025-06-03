using FunkEngine;
using Godot;

public partial class P_Midriff : EnemyPuppet
{
    public static new readonly string LoadPath =
        "res://Scenes/Puppets/Enemies/Midriff/Midriff.tscn";

    public override void _Ready()
    {
        MaxHealth = 100;
        CurrentHealth = MaxHealth;
        BaseMoney = 12;
        base._Ready();
        var enemyTween = CreateTween();
        enemyTween.TweenProperty(Sprite, "position", Vector2.Left * 5, 1f).AsRelative();
        enemyTween.TweenProperty(Sprite, "position", Vector2.Right * 5, 1f).AsRelative();
        enemyTween.SetTrans(Tween.TransitionType.Quad);
        enemyTween.SetEase(Tween.EaseType.InOut);
        enemyTween.SetLoops();
        enemyTween.Play();

        BattleEvents = new EnemyEffect[]
        {
            new EnemyEffect(
                this,
                BattleEffectTrigger.OnBattleStart,
                50,
                (_, _, val) =>
                {
                    if (StageProducer.CurLevel.Id <= 1)
                        return;
                    MaxHealth += val;
                    CurrentHealth = MaxHealth;
                    BaseMoney += val / 10;
                }
            ),
        };
    }
}
