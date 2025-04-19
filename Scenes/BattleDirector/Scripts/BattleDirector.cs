using System;
using System.Linq;
using FunkEngine;
using Godot;
using Melanchall.DryWetMidi.Interaction;

/**<summary>BattleDirector: Higher priority director to manage battle effects. Can directly access managers, which should signal up to Director WIP</summary>
 */
public partial class BattleDirector : Node2D
{
    #region Declarations

    public static readonly string LoadPath = "res://Scenes/BattleDirector/BattleScene.tscn";

    public PlayerPuppet Player;
    private EnemyPuppet[] _enemies;

    [Export]
    public Marker2D[] PuppetMarkers = new Marker2D[4]; //[0] is always player

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
        Harbinger.Init(this);
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
        PuppetMarkers[0].AddChild(Player);
        Player.Defeated += CheckBattleStatus;
        EventizeRelics();
        NPB.Setup(StageProducer.PlayerStats);
    }

    private void InitEnemies()
    {
        //TODO: Refine
        _enemies = new EnemyPuppet[StageProducer.Config.EnemyScenePath.Length];
        for (int i = 0; i < StageProducer.Config.EnemyScenePath.Length; i++)
        {
            EnemyPuppet enemy = GD.Load<PackedScene>(StageProducer.Config.EnemyScenePath[0])
                .Instantiate<EnemyPuppet>();
            if (_enemies.Length == 1)
                PuppetMarkers[2].AddChild(enemy);
            else
                PuppetMarkers[i + 1].AddChild(enemy);
            enemy.Defeated += CheckBattleStatus;
            _enemies[i] = enemy;
            AddEnemyEffects(enemy);
        }
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
            Harbinger.Instance.InvokeChartLoop(beat.Loop, false);
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

    private bool PlayerAddNote(ArrowType type, Beat beat)
    {
        if (!NPB.CanPlaceNote())
            return false;

        Note noteToPlace = NPB.NotePlaced();
        noteToPlace.OnHit(this, Timing.Okay);

        CD.AddPlayerNote(noteToPlace, type, beat);
        Harbinger.Instance.InvokeNotePlaced(new ArrowData(type, beat, noteToPlace));
        Harbinger.Instance.InvokeNoteHit(noteToPlace, Timing.Okay); //TODO: test how this feels? maybe take it out later
        return true;
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
        Harbinger.Instance.InvokeNoteHit(data.NoteRef, timed);
        NPB.HandleTiming(timed, data.Type);
        CM.ComboText(timed, data.Type, NPB.GetCurrentCombo());
    }

    private void ForceMiss(ArrowType type)
    {
        NPB.HandleTiming(Timing.Miss, type);
        CM.ComboText(Timing.Miss, type, NPB.GetCurrentCombo());
        Player.TakeDamage(new DamageInstance(4, null, Player));
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
        if (puppet is EnemyPuppet && IsBattleWon())
            OnBattleWon(); //will have to adjust this to account for when we have multiple enemies at once
    }

    private bool IsBattleWon()
    {
        return GetFirstEnemy() == null;
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
        ProcessMode = ProcessModeEnum.Disabled;
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

    #region Battles
    public void DealDamage(Note note, int damage, PuppetTemplate source)
    {
        PuppetTemplate[] targets = GetTargets(note.TargetType, !note.IsPlayerNote());
        foreach (PuppetTemplate target in targets)
        {
            target.TakeDamage(new DamageInstance(damage, source, target));
        }
    }

    public void DealDamage(
        Targetting targetting,
        int damage,
        PuppetTemplate source,
        bool targetPlayer = false
    )
    {
        PuppetTemplate[] targets = GetTargets(targetting, targetPlayer);
        foreach (PuppetTemplate target in targets)
        {
            target.TakeDamage(new DamageInstance(damage, source, target));
        }
    }

    public void AddStatus(Targetting targetting, StatusEffect status, bool targetPlayer = false)
    {
        PuppetTemplate[] targets = GetTargets(targetting, targetPlayer);
        foreach (PuppetTemplate target in targets)
        {
            target.AddStatusEffect(status);
        }

        status.StatusEnd += RemoveStatus;
        AddEvent(status);
    }

    public void RemoveStatus(StatusEffect status)
    {
        status.Sufferer.RemoveStatusEffect(status);
        status.StatusEnd -= RemoveStatus;
        RemoveEvent(status);
    }

    private PuppetTemplate[] GetTargets(Targetting targetting, bool targetPlayer = false)
    {
        if (targetPlayer)
            return [Player];
        switch (targetting)
        {
            case Targetting.First:
                if (GetFirstEnemy() != null)
                    return [GetFirstEnemy()];
                return [];
            case Targetting.All:
                return _enemies.Where(x => x.GetCurrentHealth() > 0).ToArray<PuppetTemplate>();
        }
        return null;
    }

    private PuppetTemplate GetFirstEnemy()
    {
        foreach (var enemy in _enemies)
        {
            if (enemy.GetCurrentHealth() > 0)
                return enemy;
        }

        return null;
    }
    #endregion

    #region BattleEffect Handling
    private void AddEvent(IBattleEvent bEvent)
    {
        switch (bEvent.GetTrigger())
        {
            case BattleEffectTrigger.NotePlaced:
                Harbinger.Instance.NotePlaced += bEvent.OnTrigger;
                break;
            case BattleEffectTrigger.OnLoop:
                Harbinger.Instance.ChartLooped += bEvent.OnTrigger;
                break;
            case BattleEffectTrigger.NoteHit:
                Harbinger.Instance.NoteHit += bEvent.OnTrigger;
                break;
            case BattleEffectTrigger.OnBattleEnd:
                Harbinger.Instance.BattleEnded += bEvent.OnTrigger;
                break;
            case BattleEffectTrigger.OnDamageInstance:
                Harbinger.Instance.OnDamageInstance += bEvent.OnTrigger;
                break;
        }
    }

    private void RemoveEvent(IBattleEvent bEvent)
    {
        switch (bEvent.GetTrigger())
        {
            case BattleEffectTrigger.NotePlaced:
                Harbinger.Instance.NotePlaced -= bEvent.OnTrigger;
                break;
            case BattleEffectTrigger.OnLoop:
                Harbinger.Instance.ChartLooped -= bEvent.OnTrigger;
                break;
            case BattleEffectTrigger.NoteHit:
                Harbinger.Instance.NoteHit -= bEvent.OnTrigger;
                break;
            case BattleEffectTrigger.OnBattleEnd:
                Harbinger.Instance.BattleEnded -= bEvent.OnTrigger;
                break;
            case BattleEffectTrigger.OnDamageInstance:
                Harbinger.Instance.OnDamageInstance -= bEvent.OnTrigger;
                break;
        }
    }

    private void AddEnemyEffects(EnemyPuppet enemy)
    {
        foreach (var effect in enemy.GetBattleEvents())
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

    public partial class Harbinger : Resource
    {
        private static Harbinger _instance;
        public static Harbinger Instance => _instance;

        private BattleDirector _curDirector;

        static Harbinger() { }

        private Harbinger(BattleDirector BD)
        {
            _curDirector = BD;
        }

        internal static void Init(BattleDirector BD)
        {
            _instance = new Harbinger(BD);
        }

        /// <summary>
        /// Event Args to handle event types triggering from the action of a note, without timing.
        /// </summary>
        /// <param name="bd">The BattleDirector calling the event.</param>
        /// <param name="data">The note data of the passing note.</param>
        public class NoteEventArgs(BattleDirector bd, ArrowData data) : BattleEventArgs(bd)
        {
            public ArrowData Data = data;
        }

        /// <summary>
        /// Event Args to handle event types triggering from the start of a new loop.
        /// </summary>
        /// <param name="bd">The BattleDirector calling the event.</param>
        /// <param name="incomingLoop">The loop starting.</param>
        public class LoopEventArgs(BattleDirector bd, int incomingLoop, bool artificialLoop = true)
            : BattleEventArgs(bd)
        {
            public int Loop = incomingLoop;
            public bool ArtificialLoop = artificialLoop;
        }

        /// <summary>
        /// Event Args to handle notes being hit
        /// </summary>
        /// <param name="bd">The BattleDirector calling the event.</param>
        /// <param name="note">The Note being hit.</param>
        public class NoteHitArgs(BattleDirector bd, Note note, Timing timing) : BattleEventArgs(bd)
        {
            public Note Note = note;
            public Timing Timing = timing;
        }

        internal delegate void NotePlacedHandler(BattleEventArgs e);
        internal event NotePlacedHandler NotePlaced;

        public void InvokeNotePlaced(ArrowData data)
        {
            NotePlaced?.Invoke(new NoteEventArgs(_curDirector, data));
        }

        internal delegate void ChartLoopHandler(BattleEventArgs e);
        internal event ChartLoopHandler ChartLooped;

        public void InvokeChartLoop(int incLoop, bool artificialLoop = true)
        {
            ChartLooped?.Invoke(new LoopEventArgs(_curDirector, incLoop, artificialLoop));
        }

        internal delegate void NoteHitHandler(BattleEventArgs e);
        internal event NoteHitHandler NoteHit;

        public void InvokeNoteHit(Note note, Timing timing)
        {
            NoteHit?.Invoke(new NoteHitArgs(_curDirector, note, timing));
        }

        internal delegate void BattleEndedHandler(BattleEventArgs e);
        internal event BattleEndedHandler BattleEnded;

        public void InvokeBattleEnded()
        {
            BattleEnded?.Invoke(new BattleEventArgs(_curDirector));
        }

        /// <summary>
        /// Event Args to handle a damage instance being dealt. Happens before taking damage.
        /// This allows damage to be intercepted, to be reduced/increased, to counter, or heal based on incoming damage.
        /// </summary>
        /// <param name="bd">The BattleDirector calling the event.</param>
        /// <param name="dmg">The damage instance being thrown.</param>
        public class OnDamageInstanceArgs(BattleDirector bd, DamageInstance dmg)
            : BattleEventArgs(bd)
        {
            public DamageInstance Dmg = dmg;
        }

        internal delegate void OnDamageInstanceHandler(OnDamageInstanceArgs e);
        internal event OnDamageInstanceHandler OnDamageInstance;

        public void InvokeOnDamageInstance(DamageInstance dmg)
        {
            OnDamageInstance?.Invoke(new OnDamageInstanceArgs(_curDirector, dmg));
        }
    }

    private void DebugKillEnemy()
    {
        foreach (EnemyPuppet enemy in _enemies)
        {
            enemy.TakeDamage(new DamageInstance(1000, null, enemy));
        }
    }
}
