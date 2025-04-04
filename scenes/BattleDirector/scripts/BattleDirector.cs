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
    private Conductor CD;

    [Export]
    private ChartManager CM;

    [Export]
    public NotePlacementBar NPB;

    [Export]
    private AudioStreamPlayer Audio;

    [Export]
    private Button _focusedButton; //Initial start button

    private double _timingInterval = .1; //in beats, maybe make note/bpm dependent

    private bool _initializedPlaying;

    #endregion

    #region Note Handling
    private bool PlayerAddNote(ArrowType type, Beat beat)
    {
        if (!NPB.CanPlaceNote())
            return false;

        Note noteToPlace = NPB.NotePlaced();
        noteToPlace.OnHit(this, Timing.Okay);
        CD.AddPlayerNote(noteToPlace, type, beat);
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
    private void SyncStartWithMix()
    {
        var timer = GetTree().CreateTimer(AudioServer.GetTimeToNextMix());
        timer.Timeout += BeginPlayback;
        _focusedButton.QueueFree();
        _focusedButton = null;
    }

    private void BeginPlayback()
    {
        CM.BeginTweens();
        Audio.Play();
        _initializedPlaying = true;
    }

    public override void _Ready()
    {
        SongData curSong = StageProducer.Config.CurSong.SongData;
        Audio.SetStream(GD.Load<AudioStream>(StageProducer.Config.CurSong.AudioLocation));
        if (curSong.SongLength <= 0)
        {
            curSong.SongLength = Audio.Stream.GetLength();
        }

        TimeKeeper.InitVals(curSong.Bpm);
        InitPlayer();
        InitEnemies();
        CD.Initialize(curSong);
        CD.NoteInputEvent += OnTimedInput;

        _focusedButton.GrabFocus();
        _focusedButton.Pressed += SyncStartWithMix;
    }

    private void InitPlayer()
    {
        Player = GD.Load<PackedScene>(PlayerPuppet.LoadPath).Instantiate<PlayerPuppet>();
        AddChild(Player);
        Player.Defeated += CheckBattleStatus;
        EventizeRelics();
        NPB.Setup(StageProducer.PlayerStats);
    }

    private void InitEnemies()
    {
        //TODO: Refine
        Enemy = GD.Load<PackedScene>(StageProducer.Config.EnemyScenePath)
            .Instantiate<EnemyPuppet>();
        AddChild(Enemy);
        Enemy.Defeated += CheckBattleStatus;
        AddEnemyEffects();
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
            CD.ProgressiveSpawnNotes(beat);
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
        if (@event is InputEventKey eventKey && eventKey.Pressed && !eventKey.Echo)
        {
            if (eventKey.Keycode == Key.Key0)
            {
                DebugKillEnemy();
            }
        }
    }

    //Only called from CD signal when a note is processed
    private void OnTimedInput(ArrowData data, double beatDif)
    {
        if (data.NoteRef == ArrowData.Placeholder.NoteRef)
            return; //An inactive note was passed, for now do nothing, could force miss.
        if (data.NoteRef == null) //An empty beat
        {
            if ((int)data.Beat.BeatPos % (int)TimeKeeper.BeatsPerLoop == 0)
                return; //We never ever try to place at 0
            if (PlayerAddNote(data.Type, data.Beat))
                return; //Exit handling for a placed note
            ForceMiss(data.Type); //Else force miss when a note can't be placed.
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
    #endregion

    #region Battle End
    private void CheckBattleStatus(PuppetTemplate puppet) //Called when a puppet dies
    {
        if (puppet == Player)
        {
            OnBattleLost();
            return;
        }
        if (puppet == Enemy)
            OnBattleWon(); //will have to adjust this to account for when we have multiple enemies at once
    }

    private void OnBattleWon()
    {
        Audio.StreamPaused = true;
        CleanUpRelics();
        ShowRewardSelection(3);
    }

    private void OnBattleLost()
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
        rewardSelect.Selected += TransitionOutOfBattle;
    }

    private void TransitionOutOfBattle()
    {
        StageProducer.LiveInstance.TransitionStage(Stages.Map);
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
        //Enemy.TakeDamage(1000);
    }
}
