using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

/**
 * @class BattleDirector
 * @brief Higher priority director to manage battle effects. Can directly access managers, which should signal up to Director WIP
 */
public partial class BattleDirector : Node2D
{
    #region Declarations
    private HealthBar Player;
    private HealthBar Enemy;

    [Export]
    private ChartManager CM;

    [Export]
    private InputHandler IH;

    [Export]
    private NotePlacementBar NotePlacementBar;

    [Export]
    private AudioStreamPlayer Audio;

    private double _timingInterval = .1; //secs

    [Signal]
    public delegate void PlayerDamageEventHandler(int damage);

    [Signal]
    public delegate void EnemyDamageEventHandler(int damage);

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
    private Note GetNoteAt(NoteArrow.ArrowType dir, int beat)
    {
        return GetNote(_laneData[(int)dir][beat]);
    }

    //Get note of a note arrow
    private Note GetNote(NoteArrow arrow)
    {
        return _notes[arrow.NoteIdx];
    }

    private bool AddNoteToLane(Note note, bool isActive = true)
    {
        note.Beat %= CM.BeatsPerLoop;
        //Don't add dupe notes
        if (note.Beat == 0 || _notes.Any(nt => nt.Type == note.Type && nt.Beat == note.Beat))
        {
            return false; //Beat at 0 is too messy.
        }
        _notes = _notes.Append(note).ToArray();
        //Get noteArrow from CM
        var arrow = CM.AddArrowToLane(note, _notes.Length - 1);
        arrow.IsActive = isActive;
        _laneData[(int)note.Type][note.Beat] = arrow;
        return true;
    }
    #endregion

    //Creeate dummy notes
    private void AddExampleNotes()
    {
        GD.Print(CM.BeatsPerLoop);
        for (int i = 1; i < 15; i++)
        {
            Note exampleNote = new Note(NoteArrow.ArrowType.Up, i * 4);
            AddNoteToLane(exampleNote);
        }
        for (int i = 1; i < 15; i++)
        {
            Note exampleNote = new Note(NoteArrow.ArrowType.Left, 4 * i + 1);
            AddNoteToLane(exampleNote);
        }
        for (int i = 0; i < 10; i++)
        {
            Note exampleNote = new Note(NoteArrow.ArrowType.Right, 3 * i + 32);
            AddNoteToLane(exampleNote);
        }
        for (int i = 0; i < 3; i++)
        {
            Note exampleNote = new Note(NoteArrow.ArrowType.Down, 8 * i + 16);
            AddNoteToLane(exampleNote);
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

        var timer = GetTree().CreateTimer(AudioServer.GetTimeToNextMix());
        timer.Timeout += Begin;
    }

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

        Player = GetNode<HealthBar>("PlayerHP");
        Player.GetNode<Sprite2D>("Sprite2D").Scale *= .5f; //TEMP
        Player.GetNode<Sprite2D>("Sprite2D").Position += Vector2.Down * 30; //TEMP
        Enemy = GetNode<HealthBar>("EnemyHP");

        //TEMP
        var enemTween = CreateTween();
        enemTween
            .TweenProperty(Enemy.GetNode<Sprite2D>("Sprite2D"), "position", Vector2.Down * 5, 1f)
            .AsRelative();
        enemTween
            .TweenProperty(Enemy.GetNode<Sprite2D>("Sprite2D"), "position", Vector2.Up * 5, 1f)
            .AsRelative();
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
    private void OnNotePressed(NoteArrow.ArrowType type)
    {
        CheckNoteTiming(type);
    }

    private void OnNoteReleased(NoteArrow.ArrowType arrowType) { }

    //Check all lanes for misses from missed inputs
    private void CheckMiss()
    {
        //On current beat, if prev beat is active and not inputted
        double realBeat = TimeKeeper.CurrentTime / (60 / (double)_curSong.Bpm) % CM.BeatsPerLoop;
        for (int i = 0; i < _laneData.Length; i++)
        {
            if (
                _laneLastBeat[i] < Math.Floor(realBeat)
                || (_laneLastBeat[i] == CM.BeatsPerLoop - 1 && Math.Floor(realBeat) == 0)
            )
            { //If above, a note has been missed
                //GD.Print("Last beat " + _laneLastBeat[i]);
                if (
                    _laneData[i][_laneLastBeat[i]] == null
                    || !_laneData[i][_laneLastBeat[i]].IsActive
                )
                {
                    _laneLastBeat[i] = (_laneLastBeat[i] + 1) % CM.BeatsPerLoop;
                    continue;
                }
                //Note exists and has been missed
                _laneData[i][_laneLastBeat[i]].NoteHit();
                HandleTiming((NoteArrow.ArrowType)i, 1);
                _laneLastBeat[i] = (_laneLastBeat[i] + 1) % CM.BeatsPerLoop;
            }
        }
    }

    private void CheckNoteTiming(NoteArrow.ArrowType type)
    {
        double realBeat = TimeKeeper.CurrentTime / (60 / (double)_curSong.Bpm) % CM.BeatsPerLoop;
        int curBeat = (int)Math.Round(realBeat);
        GD.Print("Cur beat " + curBeat + "Real: " + realBeat.ToString("#.###"));
        if (
            _laneData[(int)type][curBeat % CM.BeatsPerLoop] == null
            || !_laneData[(int)type][curBeat % CM.BeatsPerLoop].IsActive
        )
        {
            _laneLastBeat[(int)type] = (curBeat) % CM.BeatsPerLoop;
            PlayerAddNote(type, curBeat);
            return;
        }
        double beatDif = Math.Abs(realBeat - curBeat);
        _laneData[(int)type][curBeat % CM.BeatsPerLoop].NoteHit();
        _laneLastBeat[(int)type] = (curBeat) % CM.BeatsPerLoop;
        HandleTiming(type, beatDif);
    }

    private void HandleTiming(NoteArrow.ArrowType type, double beatDif)
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

    private void PlayerAddNote(NoteArrow.ArrowType type, int beat)
    {
        // can also add some sort of keybind here to also have pressed
        // in case the user just presses the note too early and spawns a note
        GD.Print(
            $"Player trying to place {type} typed note at beat: "
                + beat
                + " Verdict: "
                + NotePlacementBar.CanPlaceNote()
        );
        if (NotePlacementBar.CanPlaceNote())
        {
            Note exampleNote = new Note(type, beat % CM.BeatsPerLoop);
            if (AddNoteToLane(exampleNote, false))
                NotePlacementBar.PlacedNote();
        }
    }
}
