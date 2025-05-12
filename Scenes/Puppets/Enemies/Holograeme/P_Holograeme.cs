using FunkEngine;
using Godot;

public partial class P_Holograeme : EnemyPuppet
{
    public static new readonly string LoadPath =
        "res://Scenes/Puppets/Enemies/Holograeme/Holograeme.tscn";

    [Export]
    private Node2D _whiteHand;

    [Export]
    private Node2D _redHand;

    public override void _Ready()
    {
        MaxHealth = 150;
        CurrentHealth = MaxHealth;
        BaseMoney = 20;
        base._Ready();

        BattleEvents = new EnemyEffect[]
        {
            new EnemyEffect(
                this,
                BattleEffectTrigger.OnLoop,
                1,
                (e, eff, val) =>
                {
                    TweenLoop();
                }
            ),
        };
    }

    private Tween _curTween;

    private void TweenLoop()
    {
        _curTween?.Stop();
        _curTween = CreateTween();

        _curTween.TweenProperty(_whiteHand, "rotation", _redHand.Rotation, .1f);
        _curTween.SetParallel();
        _curTween.TweenProperty(_whiteHand, "rotation", Mathf.DegToRad(630), .5f);
        _curTween.TweenProperty(_redHand, "rotation", Mathf.DegToRad(630), .5f);
        _curTween.TweenCallback(
            Callable.From(() =>
            {
                _whiteHand.RotationDegrees = 270;
                _redHand.RotationDegrees = 270;
            })
        );
    }
}
