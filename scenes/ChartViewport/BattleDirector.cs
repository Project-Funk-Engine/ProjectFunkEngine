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

    //TODO: Slowly add data based on what it needs.
    private double _startTime;
    private double _currentTime;
    private double _timingInterval = .1; //secs

    public struct SongData
    {
        public int Bpm;
        public double SongLength;
        public int NumLoops;
    }

    private SongData _curSong;
    private Dictionary<string, Note[]> LaneNotes;
    private Note[] _notes = Array.Empty<Note>();

    public override void _Ready()
    {
        LaneNotes = new()
        {
            { "arrowUp", Array.Empty<Note>() },
            { "arrowLeft", Array.Empty<Note>() },
            { "arrowDown", Array.Empty<Note>() },
            { "arrowRight", Array.Empty<Note>() },
        };
        AddExampleNote();

        CM.PrepChart(_curSong, _notes);
        //TODO: Hook up signals
        NM.Connect(nameof(NoteManager.NotePressed), new Callable(this, nameof(OnNotePressed)));
        NM.Connect(nameof(NoteManager.NoteReleased), new Callable(this, nameof(OnNoteReleased)));

        _startTime = (double)Time.GetTicksMsec() / 1000;
    }

    public override void _Process(double delta)
    {
        _currentTime += delta;
        //GD.Print($"Current Time: {_currentTime}");
    }

    private void AddExampleNote()
    {
        //Create Dummy Song Data
        _curSong = new SongData
        {
            Bpm = 120,
            SongLength = 160,
            NumLoops = 5,
        };
        //Add note
        for (int i = 0; i < 5; i++)
        {
            Note exampleNote = new Note(NoteArrow.ArrowType.Left, i + 9);
            AddNoteToLane(exampleNote);
        }
    }

    private void AddNoteToLane(Note note)
    {
        _notes = _notes.Append(note).ToArray();
        LaneNotes[NM.Arrows[note.Type].Key] = LaneNotes[NM.Arrows[note.Type].Key]
            .Append(note)
            .ToArray();
    }

    private void OnNotePressed(string key)
    {
        GD.Print("Note pressed: " + key + " at " + _currentTime + " seconds.");
        CheckNoteTiming(key);
    }

    private void OnNoteReleased(NoteArrow.ArrowType arrowType)
    {
        GD.Print("Note released: " + arrowType + " at " + _currentTime + " seconds.");
    }

    private void CheckNoteTiming(string arrowString)
    {
        //Assume queue structure for notes
        //Know current time, calculate beat timing
        var curBeat = _currentTime / (60 / (double)_curSong.Bpm);
        if (LaneNotes[arrowString].Length == 0)
            return;
        double beatDif = Math.Abs(curBeat - LaneNotes[arrowString].First().Beat);
        GD.Print(beatDif);
        if (beatDif > 1)
            return;
        GD.Print("Note Hit.");
        //Cycle note queue
        LaneNotes[arrowString] = LaneNotes[arrowString]
            .Skip(1)
            .Concat(LaneNotes[arrowString].Take(1))
            .ToArray(); //TODO: No stackoverflow code
        //Change note visual
        CM.TriggerArrow();
        //Do timing stuff
        if (beatDif < _timingInterval * 2)
        {
            GD.Print("Perfect");
        }
        else if (beatDif < _timingInterval * 4)
        {
            GD.Print("Good");
        }
        else if (beatDif < _timingInterval * 6)
        {
            GD.Print("Okay");
        }
        else
        {
            GD.Print("Miss");
        }
    }
}
