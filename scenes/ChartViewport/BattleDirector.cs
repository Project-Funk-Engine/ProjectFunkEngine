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
    [Export]
    public ChartManager CM;

    [Export]
    public NoteManager NM;

    private HealthBar Player;
    private HealthBar Enemy;

    private double _timingInterval = .1; //secs

    [Signal]
    public delegate void PlayerDamageEventHandler(int damage);

    [Signal]
    public delegate void EnemyDamageEventHandler(int damage);

    public struct SongData
    {
        public int Bpm;
        public double SongLength;
        public int NumLoops;
    }

    private SongData _curSong;
    private readonly Note[][] _laneNotes = new Note[][]
    {
        Array.Empty<Note>(),
        Array.Empty<Note>(),
        Array.Empty<Note>(),
        Array.Empty<Note>(),
    };
    private Note[] _notes = Array.Empty<Note>();

    public override void _Ready()
    {
        AddExampleNote();
        CM.PrepChart(_curSong, _notes);

        Player = GetNode<HealthBar>("PlayerHP");
        Enemy = GetNode<HealthBar>("EnemyHP");

        //TODO: Hook up signals
        CM.Connect(nameof(NoteManager.NotePressed), new Callable(this, nameof(OnNotePressed)));
        CM.Connect(nameof(NoteManager.NoteReleased), new Callable(this, nameof(OnNoteReleased)));
    }

    public override void _Process(double delta)
    {
        TimeKeeper.CurrentTime += delta;
        //Check beats downwards
        double curBeat = TimeKeeper.CurrentTime / (60 / (double)_curSong.Bpm);
        for (int i = 0; i < _laneNotes.Length; i++)
        {
            if (_laneNotes[i].Length > 0)
            {
                if (curBeat > _laneNotes[i][0].Beat + 1)
                {
                    GD.Print($"Beat: {_laneNotes[i][0].Beat} Miss");
                    GD.Print(TimeKeeper.CurrentTime);
                    //Cycle note queue
                    _laneNotes[i].First().Beat += CM._beatsPerLoop;
                    _laneNotes[i] = _laneNotes[i].Skip(1).Concat(_laneNotes[i].Take(1)).ToArray(); //TODO: No stackoverflow code
                    Player.TakeDamage(10);
                    CM.HandleNote((NoteArrow.ArrowType)i);
                }
            }
        }
    }

    private void AddExampleNote()
    {
        //Create Dummy Song Data
        _curSong = new SongData
        {
            Bpm = 120,
            SongLength = 100,
            NumLoops = 5,
        };
        //Add note
        for (int i = 0; i < 4; i++)
        {
            Note exampleNote = new Note(NoteArrow.ArrowType.Up, i + 3);
            AddNoteToLane(exampleNote);
        }
        for (int i = 0; i < 1; i++)
        {
            Note exampleNote = new Note(NoteArrow.ArrowType.Left, i + 4);
            AddNoteToLane(exampleNote);
        }
    }

    private void AddNoteToLane(Note note)
    {
        _notes = _notes.Append(note).ToArray();
        _laneNotes[(int)note.Type] = _laneNotes[(int)note.Type].Append(note).ToArray();
    }

    private void OnNotePressed(NoteArrow.ArrowType type)
    {
        //GD.Print("Note pressed: " + type + " at " + _currentTime + " seconds.");
        CheckNoteTiming(type);
    }

    private void OnNoteReleased(NoteArrow.ArrowType arrowType)
    {
        //GD.Print("Note released: " + arrowType + " at " + _currentTime + " seconds.");
    }

    private void CheckNoteTiming(NoteArrow.ArrowType type)
    {
        //Assume queue structure for notes
        //Know current time, calculate beat timing
        double curBeat = TimeKeeper.CurrentTime / (60 / (double)_curSong.Bpm);
        if (_laneNotes[(int)type].Length == 0)
            return;
        double beatDif = Math.Abs(curBeat - _laneNotes[(int)type].First().Beat);
        if (beatDif > 1)
            return;
        GD.Print("Note Hit. Dif: " + beatDif);
        CM.HandleNote(type);
        //Cycle note queue
        _laneNotes[(int)type].First().Beat += CM._beatsPerLoop;
        _laneNotes[(int)type] = _laneNotes[(int)type] //Credit: Stackoverflow https://stackoverflow.com/questions/49494535/moving-the-first-array-element-to-end-in-c-sharp
            .Skip(1)
            .Concat(_laneNotes[(int)type].Take(1))
            .ToArray(); //TODO: No stackoverflow code
        //Do timing stuff
        if (beatDif < _timingInterval * 2)
        {
            GD.Print("Perfect");
            Enemy.TakeDamage(10);
        }
        else if (beatDif < _timingInterval * 4)
        {
            GD.Print("Good");
            Enemy.TakeDamage(5);
        }
        else if (beatDif < _timingInterval * 6)
        {
            GD.Print("Okay");
            Enemy.TakeDamage(1);
        }
        else
        {
            GD.Print("Miss");
            Player.TakeDamage(10);
        }
    }
}
