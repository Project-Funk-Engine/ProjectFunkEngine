using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FunkEngine;
using Godot;

/**
 * @class BattleDirector
 * @brief Higher priority director to manage battle effects. Can directly access managers, which should signal up to Director WIP
 */
public partial class BattleDirector : Node2D
{ //TODO: Maybe move some Director functionality to a sub node.
    #region Declarations

    public PlayerPuppet Player;
    public PuppetTemplate Enemy;

    [Export]
    private ChartManager CM;

    [Export]
    private NotePlacementBar NotePlacementBar;

    [Export]
    private Conductor CD;

    [Export]
    private AudioStreamPlayer Audio;

    private double _timingInterval = .1; //secs, maybe make somewhat note dependent

    private SongData _curSong;

    private bool _battleEnd;

    #endregion

    #region Note Handling
    private void PlayerAddNote(ArrowType type, int beat)
    {
        if (!NotePlacementBar.CanPlaceNote())
            return;
        if (CD.AddNoteToLane(type, beat % CM.BeatsPerLoop, NotePlacementBar.PlacedNote(), false))
        {
            NotePlaced?.Invoke(this);
            GD.Print("Note Placed.");
        }
    }

    public PuppetTemplate GetTarget(Note note)
    {
        if (note.Owner == Player)
        {
            return Enemy;
        }

        return Player;
    }
    #endregion

    #region Initialization
    public override void _Ready()
    {
        //TODO: Should come from transition into battle
        _curSong = new SongData
        {
            Bpm = 120,
            SongLength = Audio.Stream.GetLength(),
            NumLoops = 5,
        };
        TimeKeeper.Bpm = _curSong.Bpm;

        Player = new PlayerPuppet();
        AddChild(Player);
        Player.Defeated += CheckBattleStatus;
        EventizeRelics();
        NotePlacementBar.Setup(StageProducer.PlayerStats);

        //TODO: Refine
        Enemy = new PuppetTemplate();
        Enemy.SetPosition(new Vector2(400, 0));
        AddChild(Enemy);
        Enemy.Defeated += CheckBattleStatus;
        Enemy.Init(GD.Load<Texture2D>("res://scenes/BattleDirector/assets/Enemy1.png"), "Enemy");

        //TODO: This is a temporary measure
        Button startButton = new Button();
        startButton.Text = "Start";
        startButton.Position = GetViewportRect().Size / 2;
        AddChild(startButton);
        startButton.Pressed += () =>
        {
            var timer = GetTree().CreateTimer(AudioServer.GetTimeToNextMix());
            timer.Timeout += Begin;
            startButton.QueueFree();
        };
    }

    //TODO: This will all change
    private void Begin()
    {
        CM.PrepChart(_curSong);
        CD.Prep();
        CD.TimedInput += OnTimedInput;

        //TODO: Make enemies, can put this in an enemy subclass
        Enemy.Sprite.Scale *= 2;
        var enemTween = CreateTween();
        enemTween.TweenProperty(Enemy.Sprite, "position", Vector2.Down * 5, 1f).AsRelative();
        enemTween.TweenProperty(Enemy.Sprite, "position", Vector2.Up * 5, 1f).AsRelative();
        enemTween.SetTrans(Tween.TransitionType.Spring);
        enemTween.SetEase(Tween.EaseType.In);
        enemTween.SetLoops();
        enemTween.Play();

        CM.Connect(nameof(InputHandler.NotePressed), new Callable(this, nameof(OnNotePressed)));
        CM.Connect(nameof(InputHandler.NoteReleased), new Callable(this, nameof(OnNoteReleased)));

        Audio.Play();
    }

    public override void _Process(double delta)
    {
        TimeKeeper.CurrentTime = Audio.GetPlaybackPosition();
        CD.CheckMiss();
        if (_battleEnd)
            GetNode<StageProducer>("/root/StageProducer").TransitionStage(Stages.Map);
    }
    #endregion

    #region Input&Timing

    public override void _UnhandledInput(InputEvent @event)
    {
        //this one is for calling a debug key to insta-kill the enemy
        if (@event is InputEventKey eventKey && eventKey.Pressed && !eventKey.Echo)
        {
            if (eventKey.Keycode == Key.Key0)
            {
                DebugKillEnemy();
            }
        }

        if (@event.IsActionPressed("Pause"))
        {
            var pauseMenu = GD.Load<PackedScene>("res://scenes/UI/Pause.tscn");
            GetNode<CanvasLayer>("UILayer").AddChild(pauseMenu.Instantiate());
            GetTree().Paused = true;
        }
        if (@event.IsActionPressed("Inventory"))
        {
            var invenMenu = GD.Load<PackedScene>("res://scenes/UI/inventory.tscn")
                .Instantiate<Inventory>();
            GetNode<CanvasLayer>("UILayer").AddChild(invenMenu);
            invenMenu.Display(Player.Stats);
            GetTree().Paused = true;
        }
    }

    private void OnNotePressed(ArrowType type)
    {
        CD.CheckNoteTiming(type);
    }

    private void OnNoteReleased(ArrowType arrowType) { }

    private void OnTimedInput(Note note, ArrowType arrowType, int beat, double beatDif)
    {
        GD.Print(arrowType + " " + beat + " difference: " + beatDif);
        if (note == null)
        {
            PlayerAddNote(arrowType, beat);
            return;
        }

        Timing timed = CheckTiming(beatDif);

        if (timed == Timing.Miss)
        {
            note.OnHit(this, timed);
            NotePlacementBar.MissNote();
        }
        else
        {
            note.OnHit(this, timed);
            NotePlacementBar.HitNote();
        }
        NotePlacementBar.ComboText(timed.ToString());
    }

    private Timing CheckTiming(double beatDif)
    {
        if (beatDif < _timingInterval * 1)
        {
            return Timing.Perfect;
        }

        if (beatDif < _timingInterval * 2)
        {
            return Timing.Good;
        }

        if (beatDif < _timingInterval * 3)
        {
            return Timing.Okay;
        }

        return Timing.Miss;
    }

    private void CheckBattleStatus(PuppetTemplate puppet)
    {
        if (puppet == Player)
        {
            GD.Print("Player is Dead");
            GetNode<StageProducer>("/root/StageProducer").TransitionStage(Stages.Title);
            return;
        }

        //will have to adjust this to account for when we have multiple enemies at once
        if (puppet == Enemy)
        {
            GD.Print("Enemy is dead");
            ShowRewardSelection(3);
            _battleEnd = true;
        }
    }

    private void ShowRewardSelection(int amount)
    {
        var rewardUI = GD.Load<PackedScene>("res://scenes/UI/RewardSelectionUI.tscn")
            .Instantiate<RewardSelect>();
        AddChild(rewardUI);
        rewardUI.Initialize(Player.Stats, amount);
        GetTree().Paused = true;
    }

    #endregion

    #region BattleEffect Handling

    private delegate void NotePlacedHandler(BattleDirector BD);
    private event NotePlacedHandler NotePlaced;

    private void EventizeRelics()
    {
        foreach (var relic in Player.Stats.CurRelics)
        {
            foreach (var effect in relic.Effects)
            {
                switch (effect.GetTrigger()) //TODO: Look into a way to get eventhandler from string
                {
                    case BattleEffectTrigger.NotePlaced:
                        NotePlaced += effect.OnTrigger;
                        break;
                }
            }
        }
    }
    #endregion

    private void DebugKillEnemy()
    {
        Enemy.TakeDamage(1000);
    }
}
