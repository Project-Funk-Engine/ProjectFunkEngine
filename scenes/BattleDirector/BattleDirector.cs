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
    public ChartManager CM;

    [Export]
    public InputHandler IH;

    [Export]
    private NotePlacementBar NotePlacementBar;

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
    private readonly NoteArrow[][] _laneData = new NoteArrow[][]
    {
        Array.Empty<NoteArrow>(),
        Array.Empty<NoteArrow>(),
        Array.Empty<NoteArrow>(),
        Array.Empty<NoteArrow>(),
    };
    private Note[] _notes = Array.Empty<Note>();

    //Cycles lane of dir and returns the, initially, first note
    private Note CycleNote(NoteArrow.ArrowType dir)
    {
        var note = GetFirstNote(dir);
        _laneData[(int)dir] = _laneData[(int)dir] //Credit: Stackoverflow https://stackoverflow.com/questions/49494535/moving-the-first-array-element-to-end-in-c-sharp
            .Skip(1)
            .Concat(_laneData[(int)dir].Take(1))
            .ToArray();
        return note;
    }

    //Returns first note of lane without modifying lane data
    private Note GetFirstNote(NoteArrow.ArrowType dir)
    {
        return GetNote(_laneData[(int)dir].First());
    }

    //Get note of a note arrow
    private Note GetNote(NoteArrow arrow)
    {
        return _notes[arrow.NoteIdx];
    }

    private bool AddNoteToLane(Note note)
    {
        //Don't add dupe notes
        if (_notes.Any(nt => nt.Type == note.Type && nt.Beat == note.Beat))
        {
            return false;
        }
        _notes = _notes.Append(note).ToArray();
        //Get noteArrow from CM
        var arrow = CM.AddArrowToLane(note, _notes.Length - 1);
        _laneData[(int)note.Type] = _laneData[(int)note.Type].Append(arrow).ToArray();
        return true;
    }
    #endregion

    //Creeate dummy notes
    private void AddExampleNotes()
    {
        for (int i = 0; i < 4; i++)
        {
            Note exampleNote = new Note(NoteArrow.ArrowType.Up, i + 20);
            AddNoteToLane(exampleNote);
        }
        for (int i = 0; i < 1; i++)
        {
            Note exampleNote = new Note(NoteArrow.ArrowType.Left, i + 21);
            AddNoteToLane(exampleNote);
        }
    }

    public override void _Ready()
    {
        _curSong = new SongData
        {
            Bpm = 120,
            SongLength = 100,
            NumLoops = 5,
        };
        CM.PrepChart(_curSong);
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
    }

    public override void _Process(double delta)
    {
        TimeKeeper.CurrentTime += delta;
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
        double curBeat = TimeKeeper.CurrentTime / (60 / (double)_curSong.Bpm) % CM.BeatsPerLoop;
        for (int i = 0; i < _laneData.Length; i++)
        {
            if (_laneData[i].Length <= 0)
                continue;
            double beatDif = (curBeat - GetFirstNote((NoteArrow.ArrowType)i).Beat);
            if (beatDif > 1 && _laneData[i].First().Visible) //Can change, currently using visible as a stand in for already activated.
            {
                _laneData[i].First().NoteHit();
                HandleTiming((NoteArrow.ArrowType)i, Math.Abs(beatDif));
            }
        }
    }

    private void CheckNoteTiming(NoteArrow.ArrowType type)
    {
        double curBeat = TimeKeeper.CurrentTime / (60 / (double)_curSong.Bpm) % CM.BeatsPerLoop;
        if (_laneData[(int)type].Length == 0)
            return;
        double beatDif = Math.Abs(curBeat - GetFirstNote(type).Beat);
        if (beatDif > 1)
            return;
        GD.Print("Note Hit. Dif: " + beatDif);
        _laneData[(int)type].First().NoteHit();
        HandleTiming(type, beatDif);
    }

    private void HandleTiming(NoteArrow.ArrowType type, double beatDif)
    {
        CycleNote(type);
        if (beatDif < _timingInterval * 2)
        {
            GD.Print("Perfect");
            Enemy.TakeDamage(1);
            NotePlacementBar.HitNote();
        }
        else if (beatDif < _timingInterval * 4)
        {
            GD.Print("Good");
            Enemy.TakeDamage(0);
            NotePlacementBar.HitNote();
        }
        else if (beatDif < _timingInterval * 6)
        {
            GD.Print("Okay");
            Player.TakeDamage(1);
            NotePlacementBar.HitNote();
        }
        else
        {
            GD.Print("Miss");
            Player.TakeDamage(2);
            NotePlacementBar.MissNote();
        }
    }
    #endregion

    private void PlayerAddNote(NoteArrow.ArrowType type, int beat)
    {
        //TODO: notes currently can only be placed in first loop.
        // placed notes are also non-interactable

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
            if (AddNoteToLane(exampleNote))
                NotePlacementBar.PlacedNote();
        }
    }
}
