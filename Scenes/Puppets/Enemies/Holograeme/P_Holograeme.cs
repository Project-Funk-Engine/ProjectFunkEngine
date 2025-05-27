using FunkEngine;
using Godot;

public partial class P_Holograeme : EnemyPuppet
{
    public static new readonly string LoadPath =
        "res://Scenes/Puppets/Enemies/Holograeme/Holograeme.tscn";

    private readonly string NoteCoverPath = "res://Scenes/Puppets/Enemies/Holograeme/NoteCover.png";

    [Export]
    private Node2D _whiteHand;

    [Export]
    private Node2D _redHand;

    public override void _ExitTree()
    {
        Scribe.NoteDictionary[0].Texture = null;
        BattleDirector.AutoPlay = false;
        BattleDirector.PlayerDisabled = false;
        Conductor.BeatSpawnOffsetModifier = 0;
    }

    public override void _Ready()
    {
        Conductor.BeatSpawnOffsetModifier = 1;
        MaxHealth = 3;
        CurrentHealth = MaxHealth;
        BaseMoney = 40;
        base._Ready();

        _hands[0] = _redHand;
        _hands[1] = _whiteHand;

        BattleEvents = new EnemyEffect[]
        {
            new EnemyEffect(
                this,
                BattleEffectTrigger.OnBattleStart,
                -1,
                (e, _, _) =>
                {
                    BattleDirector.AutoPlay = true;
                    BattleDirector.PlayerDisabled = true;
                    e.BD.AddStatus(Targetting.Player, StatusEffect.Disable);
                }
            ),
            new EnemyEffect(
                this,
                BattleEffectTrigger.OnLoop,
                1,
                (e, _, _) =>
                {
                    TweenLoop();
                    if (e is not BattleDirector.Harbinger.LoopEventArgs lArgs)
                        return;
                    if (lArgs.Loop % 2 == 0)
                    {
                        BattleDirector.AutoPlay = true;
                        BattleDirector.PlayerDisabled = true;
                        e.BD.AddStatus(Targetting.Player, StatusEffect.Disable);
                        Scribe.NoteDictionary[0].Texture = null;
                    }
                    else if (lArgs.Loop % 2 == 1)
                    {
                        BattleDirector.AutoPlay = false;
                        Scribe.NoteDictionary[0].Texture = GD.Load<Texture2D>(NoteCoverPath);
                    }
                }
            ),
            new EnemyEffect(
                this,
                BattleEffectTrigger.NoteHit,
                1,
                (e, _, _) =>
                {
                    if (e is BattleDirector.Harbinger.NoteHitArgs nArgs)
                    {
                        TweenDir(nArgs.Type);
                    }
                }
            ),
            new EnemyEffect(
                this,
                BattleEffectTrigger.OnDamageInstance,
                3,
                (e, eff, val) =>
                {
                    if (
                        e is not BattleDirector.Harbinger.OnDamageInstanceArgs dArgs
                        || dArgs.Dmg.Target != eff.Owner
                    )
                        return;
                    if (dArgs.Dmg.Source != e.BD.Player || dArgs.Dmg.Damage < val)
                    {
                        dArgs.Dmg.ModifyDamage(0, 0);
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

    private int[] _dirToAngle = [270, 450, 180, 360]; //ArrowType to angle in deg

    private void TweenDir(ArrowType dir)
    {
        int handIdx = 0;
        if (dir == ArrowType.Down || dir == ArrowType.Up)
            handIdx = 1;

        _curTween = CreateTween();

        _curTween.TweenProperty(
            _hands[handIdx],
            "rotation",
            Mathf.DegToRad(_dirToAngle[(int)dir]),
            .1f
        );
        _curTween.TweenCallback(
            Callable.From(() =>
            {
                int offset = 0;
                if (_dirToAngle[(int)dir] >= 360)
                    offset = 360;
                _hands[handIdx].RotationDegrees = _dirToAngle[(int)dir] - offset;
            })
        );
    }
}
