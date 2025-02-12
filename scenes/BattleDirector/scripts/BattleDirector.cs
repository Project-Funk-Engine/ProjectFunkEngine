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

    [Export]
    private NoteQueue NQ;

    #endregion

    #region Note Handling
    private void PlayerAddNote(ArrowType type, int beat)
    {
        //TODO: note that should be added from the queue
        Note note = NQ.GetCurrentNote();
        if (note == null)
        {
            GD.Print("No notes in queue");
            return;
        }

        GD.Print($"Player trying to place {type} typed note at beat: " + beat);
        if (!NotePlacementBar.CanPlaceNote())
            return;
        if (CD.AddNoteToLane(type, beat % CM.BeatsPerLoop, false))
        {
            NotePlacementBar.PlacedNote();
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
        _curSong = new SongData
        {
            Bpm = 120,
            SongLength = Audio.Stream.GetLength(),
            NumLoops = 5,
        };
        TimeKeeper.Bpm = _curSong.Bpm;

        Player = new PlayerPuppet();
        AddChild(Player);
        EventizeRelics();
        //TODO: Refine
        foreach (var note in Player.Stats.CurNotes)
        {
            note.Owner = Player;
            CD.Notes = CD.Notes.Append(note).ToArray();
        }
        Note enemNote = Scribe.NoteDictionary[0].Clone();
        CD.Notes = CD.Notes.Append(enemNote).ToArray();

        Enemy = new PuppetTemplate();
        Enemy.SetPosition(new Vector2(400, 0));
        AddChild(Enemy);
        Enemy.Init(GD.Load<Texture2D>("res://scenes/BattleDirector/assets/Enemy1.png"), "Enemy");
        Enemy.Sprite.Scale *= 2;

        var timer = GetTree().CreateTimer(AudioServer.GetTimeToNextMix());
        timer.Timeout += Begin;
    }

    //TODO: This will all change
    private void Begin()
    {
        CM.PrepChart(_curSong);
        CD.Prep();
        CD.TimedInput += OnTimedInput;

        //TEMP TODO: Make enemies, can put this in an enemy subclass
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
    }
    #endregion

    #region Input&Timing
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
        //TODO: Evaluate Timing as a function
        Timing timed = CheckTiming(beatDif);
        GD.Print(timed);

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

    #endregion

    #region BattleEffect Handling

    private delegate void NotePlacedHandler(BattleDirector BD);
    private event NotePlacedHandler NotePlaced;

    private void EventizeRelics()
    {
        foreach (var relic in Player.Stats.CurRelics)
        {
            GetNode<Label>("TempRelicList").Text += "\n" + relic.Name;
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
}
