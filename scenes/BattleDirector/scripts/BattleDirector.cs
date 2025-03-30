using System;
using FunkEngine;
using Godot;

/**<summary>BattleDirector: Higher priority director to manage battle effects. Can directly access managers, which should signal up to Director WIP</summary>
 */
public partial class BattleDirector : Node2D
{
    #region Declarations

    public static readonly string LoadPath = "res://Scenes/BattleDirector/BattleScene.tscn";

    public PlayerPuppet Player;
    public EnemyPuppet Enemy;

    [Export]
    private ChartManager CM;

    [Export]
    public NotePlacementBar NPB;

    [Export]
    private Conductor CD;

    [Export]
    private AudioStreamPlayer Audio;

    [Export]
    private Button _focusedButton; //Initially start button

    private double _timingInterval = .1; //in beats, maybe make note dependent

    private SongData _curSong;

    private bool _isPlaying;

    #endregion

    #region Note Handling
    private bool PlayerAddNote(ArrowType type, Beat beat)
    {
        if (!NPB.CanPlaceNote())
            return false;
        CD.AddPlayerNote(NPB.PlacedNote(this), type, beat);
        NotePlaced?.Invoke(this);
        return true;
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
        _curSong = StageProducer.Config.CurSong.SongData;
        Audio.SetStream(GD.Load<AudioStream>(StageProducer.Config.CurSong.AudioLocation));
        if (_curSong.SongLength <= 0)
        {
            _curSong.SongLength = Audio.Stream.GetLength();
        }
        TimeKeeper.InitVals(_curSong.Bpm);

        Player = GD.Load<PackedScene>(PlayerPuppet.LoadPath).Instantiate<PlayerPuppet>();
        AddChild(Player);
        Player.Defeated += CheckBattleStatus;
        EventizeRelics();
        NPB.Setup(StageProducer.PlayerStats);

        //TODO: Refine
        Enemy = GD.Load<PackedScene>(StageProducer.Config.EnemyScenePath)
            .Instantiate<EnemyPuppet>();
        AddChild(Enemy);
        Enemy.Defeated += CheckBattleStatus;
        AddEnemyEffects();

        CM.Initialize(_curSong);
        CD.Initialize();
        CD.NoteInputEvent += OnTimedInput;

        _focusedButton.GrabFocus();
        _focusedButton.Pressed += () =>
        {
            var timer = GetTree().CreateTimer(AudioServer.GetTimeToNextMix());
            timer.Timeout += Begin;
            _focusedButton.QueueFree();
            _focusedButton = null;
        };
    }

    private void Begin()
    {
        CM.BeginTweens();
        Audio.Play();
        _isPlaying = true;
    }

    private void EndBattle()
    {
        StageProducer.ChangeCurRoom(StageProducer.Config.BattleRoom.Idx);
        StageProducer.LiveInstance.TransitionStage(Stages.Map);
    }

    public override void _Process(double delta)
    {
        TimeKeeper.CurrentTime = Audio.GetPlaybackPosition();
        Beat realBeat = TimeKeeper.GetBeatFromTime(Audio.GetPlaybackPosition());

        UpdateBeat(realBeat);
    }

    private void UpdateBeat(Beat beat)
    {
        //Still iffy, but approximately once per beat check, happens at start of new beat
        if (Math.Floor(beat.BeatPos) >= Math.Floor((TimeKeeper.LastBeat + 1).BeatPos))
        {
            CD.ProgressiveAddNotes(beat);
        }
        if (beat.Loop > TimeKeeper.LastBeat.Loop)
        {
            ChartLooped?.Invoke(this);
        }
        TimeKeeper.LastBeat = beat;
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
    }

    private void OnTimedInput(NoteArrowData data, double beatDif)
    {
        if (data.NoteRef == NoteArrowData.Placeholder.NoteRef)
            return; //Hit an inactive note, for now do nothing
        if (data.NoteRef == null)
        {
            if ((int)data.Beat.BeatPos % (int)TimeKeeper.BeatsPerLoop == 0)
                return; //We never ever try to place at 0
            if (PlayerAddNote(data.Type, data.Beat))
                return; //Miss on empty note. This does not apply to inactive existing notes as a balance decision for now.
            ForceMiss(data.Type);
            return;
        }

        Timing timed = CheckTiming(beatDif);

        data.NoteRef.OnHit(this, timed);
        NPB.HandleTiming(timed);
        CM.ComboText(timed, data.Type, NPB.GetCurrentCombo());
    }

    private void ForceMiss(ArrowType type)
    {
        NPB.HandleTiming(Timing.Miss);
        CM.ComboText(Timing.Miss, type, NPB.GetCurrentCombo());
        Player.TakeDamage(4);
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
            BattleLost();
            return;
        }
        if (puppet == Enemy)
            BattleWon(); //will have to adjust this to account for when we have multiple enemies at once
    }

    private void BattleWon()
    {
        Audio.StreamPaused = true;
        CleanUpRelics();
        ShowRewardSelection(3);
    }

    private void BattleLost()
    {
        Audio.StreamPaused = true;
        SaveSystem.ClearSave();
        AddChild(GD.Load<PackedScene>(EndScreen.LoadPath).Instantiate());
        GetTree().Paused = true;
    }

    private void ShowRewardSelection(int amount)
    {
        var rewardSelect = RewardSelect.CreateSelection(
            this,
            Player.Stats,
            amount,
            StageProducer.Config.RoomType
        );
        rewardSelect.GetNode<Label>("%TopLabel").Text = Tr("BATTLE_ROOM_WIN");
        rewardSelect.Selected += EndBattle;
    }

    #endregion

    #region BattleEffect Handling

    private delegate void NotePlacedHandler(BattleDirector BD);
    private event NotePlacedHandler NotePlaced;

    private delegate void ChartLoopHandler(BattleDirector BD);
    private event ChartLoopHandler ChartLooped;

    private void AddEvent(IBattleEvent bEvent)
    {
        switch (bEvent.GetTrigger()) //TODO: Look into a way to get eventhandler from string
        {
            case BattleEffectTrigger.NotePlaced:
                NotePlaced += bEvent.OnTrigger;
                break;
            case BattleEffectTrigger.OnLoop:
                ChartLooped += bEvent.OnTrigger;
                break;
        }
    }

    private void AddEnemyEffects()
    {
        foreach (var effect in Enemy.GetBattleEvents())
        {
            AddEvent(effect);
        }
    }

    private void EventizeRelics()
    {
        foreach (var relic in Player.Stats.CurRelics)
        {
            foreach (var effect in relic.Effects)
            {
                AddEvent(effect);
            }
        }
    }

    private void CleanUpRelics()
    {
        foreach (var relic in Player.Stats.CurRelics)
        {
            foreach (var effect in relic.Effects)
            {
                effect.OnBattleEnd();
            }
        }
    }
    #endregion

    private void DebugKillEnemy()
    {
        Enemy.TakeDamage(1000);
    }
}
