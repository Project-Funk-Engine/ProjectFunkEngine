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
{
    #region Declarations
    private Puppet_Template Player;
    private Puppet_Template Enemy;

    [Export]
    private ChartManager CM;

    [Export]
    private InputHandler IH;

    [Export]
    private NotePlacementBar NotePlacementBar;

    [Export]
    private AudioStreamPlayer Audio;

    private double _timingInterval = .1; //secs, maybe make somewhat note dependent

    private SongData _curSong;

    public struct SongData
    {
        public int Bpm;
        public double SongLength;
        public int NumLoops;
    }
    #endregion

    #region Note Handling
    //Assume queue structure for notes in each lane.
    //Can eventually make this its own structure
    private NoteArrow[][] _laneData = Array.Empty<NoteArrow[]>();
    private int[] _laneLastBeat = new int[]
    { //Temporary (hopefully) measure to bridge from note queue structure to ordered array
        0,
        0,
        0,
        0,
    };
    private Note[] _notes = Array.Empty<Note>();

    //Returns first note of lane without modifying lane data
    private Note GetNoteAt(ArrowType dir, int beat)
    {
        return GetNote(_laneData[(int)dir][beat]);
    }

    //Get note of a note arrow
    private Note GetNote(NoteArrow arrow)
    {
        return _notes[arrow.NoteIdx];
    }

    private bool IsNoteActive(ArrowType type, int beat)
    {
        return _laneData[(int)type][beat] != null && _laneData[(int)type][beat].IsActive;
    }

    private bool AddNoteToLane(ArrowType type, int beat, bool isActive = true)
    {
        beat %= CM.BeatsPerLoop;
        //Don't add dupe notes //Beat at 0 is too messy.
        if (beat == 0 || _laneData[(int)type][beat] != null)
        {
            return false;
        }
        //Get noteArrow from CM
        var arrow = CM.AddArrowToLane(type, beat, _notes.Length - 1);
        arrow.IsActive = isActive;
        _laneData[(int)type][beat] = arrow;
        return true;
    }
    #endregion

    //Creeate dummy notes
    private void AddExampleNotes()
    {
        GD.Print(CM.BeatsPerLoop);
        for (int i = 1; i < 15; i++)
        {
            AddNoteToLane(ArrowType.Up, i * 4);
        }
        for (int i = 1; i < 15; i++)
        {
            AddNoteToLane(ArrowType.Left, 4 * i + 1);
        }
        for (int i = 0; i < 10; i++)
        {
            AddNoteToLane(ArrowType.Right, 3 * i + 32);
        }
        for (int i = 0; i < 3; i++)
        {
            AddNoteToLane(ArrowType.Down, 8 * i + 16);
        }
    }

    public override void _Ready()
    {
        _curSong = new SongData
        {
            Bpm = 120,
            SongLength = Audio.Stream.GetLength(),
            NumLoops = 5,
        };

        Player = new Puppet_Template();
        AddChild(Player);
        Player.Init(
            GD.Load<Texture2D>("res://scenes/BattleDirector/assets/Character1.png"),
            "Player"
        );
        Player.SetPosition(new Vector2(80, 0));
        Player.Sprite.Position += Vector2.Down * 30; //TEMP

        Enemy = new Puppet_Template();
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
        _laneData = new NoteArrow[][]
        {
            new NoteArrow[CM.BeatsPerLoop],
            new NoteArrow[CM.BeatsPerLoop],
            new NoteArrow[CM.BeatsPerLoop],
            new NoteArrow[CM.BeatsPerLoop],
        };
        AddExampleNotes();

        //TEMP
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
        CheckMiss();
    }

    #region Input&Timing
    private void OnNotePressed(ArrowType type)
    {
        CheckNoteTiming(type);
    }

    private void OnNoteReleased(ArrowType arrowType) { }

    //Check all lanes for misses from missed inputs
    private void CheckMiss()
    {
        //On current beat, if prev beat is active and not inputted
        double realBeat = TimeKeeper.CurrentTime / (60 / (double)_curSong.Bpm) % CM.BeatsPerLoop;
        for (int i = 0; i < _laneData.Length; i++)
        {
            if (!(_laneLastBeat[i] < Math.Floor(realBeat)))
                continue;
            if (!IsNoteActive((ArrowType)i, _laneLastBeat[i]))
            {
                _laneLastBeat[i] = (_laneLastBeat[i] + 1) % CM.BeatsPerLoop;
                continue;
            }
            //Note exists and has been missed
            _laneData[i][_laneLastBeat[i]].NoteHit();
            HandleTiming(1);
            _laneLastBeat[i] = (_laneLastBeat[i] + 1) % CM.BeatsPerLoop;
        }
    }

    private void CheckNoteTiming(ArrowType type)
    {
        double realBeat = TimeKeeper.CurrentTime / (60 / (double)_curSong.Bpm) % CM.BeatsPerLoop;
        int curBeat = (int)Math.Round(realBeat);
        GD.Print("Cur beat " + curBeat + "Real: " + realBeat.ToString("#.###"));
        if (_laneData[(int)type][curBeat % CM.BeatsPerLoop] == null)
        {
            PlayerAddNote(type, curBeat);
            return;
        }
        if (!_laneData[(int)type][curBeat % CM.BeatsPerLoop].IsActive)
            return;
        double beatDif = Math.Abs(realBeat - curBeat);
        _laneData[(int)type][curBeat % CM.BeatsPerLoop].NoteHit();
        _laneLastBeat[(int)type] = (curBeat) % CM.BeatsPerLoop;
        HandleTiming(beatDif);
    }

    private void HandleTiming(double beatDif)
    {
        if (beatDif < _timingInterval * 1)
        {
            GD.Print("Perfect");
            Enemy.TakeDamage(3);
            NotePlacementBar.HitNote();
            NotePlacementBar.ComboText("Perfect!");
        }
        else if (beatDif < _timingInterval * 2)
        {
            GD.Print("Good");
            Enemy.TakeDamage(1);
            NotePlacementBar.HitNote();
            NotePlacementBar.ComboText("Good");
        }
        else if (beatDif < _timingInterval * 3)
        {
            GD.Print("Ok");
            Player.TakeDamage(1);
            NotePlacementBar.HitNote();
            NotePlacementBar.ComboText("Okay");
        }
        else
        {
            GD.Print("Miss");
            Player.TakeDamage(2);
            NotePlacementBar.MissNote();
            NotePlacementBar.ComboText("Miss");
        }
    }
    #endregion

    private void PlayerAddNote(ArrowType type, int beat)
    {
        // can also add some sort of keybind here to also have pressed
        // in case the user just presses the note too early and spawns a note
        GD.Print($"Player trying to place {type} typed note at beat: " + beat);
        if (NotePlacementBar.CanPlaceNote())
        {
            if (AddNoteToLane(type, beat % CM.BeatsPerLoop, false))
                NotePlacementBar.PlacedNote();
            GD.Print("Note Placed.");
        }
    }
}
