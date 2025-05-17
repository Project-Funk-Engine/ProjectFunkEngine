using FunkEngine;
using Godot;

public partial class Astrorat : EnemyPuppet
{
    public static new readonly string LoadPath =
        "res://Scenes/Puppets/Enemies/Astrorat/Astrorat.tscn";

    public override void _Ready()
    {
        MaxHealth = 150;
        CurrentHealth = MaxHealth;
        BaseMoney = 25;
        InitialNote = (17, 3);
        base._Ready();
        var enemyTween = CreateTween();
        enemyTween.TweenProperty(Sprite, "position", Vector2.Up * 5, 1f).AsRelative();
        enemyTween.TweenProperty(Sprite, "position", Vector2.Down * 5, 1f).AsRelative();
        enemyTween.SetTrans(Tween.TransitionType.Quad);
        enemyTween.SetEase(Tween.EaseType.InOut);
        enemyTween.SetLoops();
        enemyTween.Play();

        BattleEvents = new EnemyEffect[]
        {
            new EnemyEffect(
                this,
                BattleEffectTrigger.OnLoop,
                1,
                (e, eff, val) =>
                {
                    e.BD.RandApplyNote(eff.Owner, InitialNote.NoteId, val);
                }
            ),
        };
    }
}
