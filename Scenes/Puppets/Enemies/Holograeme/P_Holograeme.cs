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

        _hands[0] = _redHand;
        _hands[1] = _whiteHand;

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
            new EnemyEffect(
                this,
                BattleEffectTrigger.NoteHit,
                1,
                (e, eff, val) =>
                {
                    if (e is BattleDirector.Harbinger.NoteHitArgs nArgs)
                    {
                        TweenDir(_hands[_curHandIdx], nArgs.Type);
                    }
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
        _curTween.TweenProperty(_whiteHand, "rotation", Mathf.DegToRad(-450) - 2 * Mathf.Pi, .4f);
        _curTween
            .Parallel()
            .TweenProperty(_redHand, "rotation", Mathf.DegToRad(-450) - 2 * Mathf.Pi, .4f);
        _curTween.TweenCallback(
            Callable.From(() =>
            {
                _whiteHand.RotationDegrees = 270;
                _redHand.RotationDegrees = 270;
            })
        );
    }

    private Node2D[] _hands = new Node2D[2];
    private int _curHandIdx;

    private void IncrCurHand()
    {
        _curHandIdx = (_curHandIdx + 1) % _hands.Length;
    }

    private int[] _dirToAngle = [270, 90, 180, 0]; //ArrowType to angle in deg

    private void TweenDir(Node2D hand, ArrowType dir)
    {
        _curTween = CreateTween();

        _curTween.TweenProperty(hand, "rotation", Mathf.DegToRad(_dirToAngle[(int)dir]), .1f);
        _curTween.TweenCallback(
            Callable.From(() =>
            {
                hand.RotationDegrees = _dirToAngle[(int)dir];
            })
        );
        IncrCurHand();
    }
}
