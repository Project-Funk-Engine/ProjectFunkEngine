using System;
using FunkEngine;
using Godot;

public partial class Conductor : Node
{
    [Export]
    private ChartManager CM;
    public MidiMaestro MM;

    public delegate void TimedInputHandler(Note note, ArrowType type, int beat, double beatDif);
    public event TimedInputHandler TimedInput;

    //Assume queue structure for notes in each lane.
    //Can eventually make this its own structure
    private NoteArrow[][] _laneData = Array.Empty<NoteArrow[]>();

    private int[] _laneLastBeat = new int[]
    {
        //Temporary (hopefully) measure to bridge from note queue structure to ordered array
        0,
        0,
        0,
        0,
    };

    //Returns first note of lane without modifying lane data
    private Note GetNoteAt(ArrowType dir, int beat)
    {
        return GetNote(_laneData[(int)dir][beat]);
    }

    //Get note of a note arrow
    private Note GetNote(NoteArrow arrow)
    {
        return arrow.NoteRef;
    }

    private bool IsNoteActive(ArrowType type, int beat)
    {
        return _laneData[(int)type][beat] != null && _laneData[(int)type][beat].IsActive;
    }

    public bool AddNoteToLane(ArrowType type, int beat, Note note, bool isActive = true)
    {
        beat %= CM.BeatsPerLoop;
        Note newNote = note.Clone();
        if (beat == 0 || _laneData[(int)type][beat] != null) //TODO: Double check if this is still necessary, doesn't seem to matter for player placed notes
            return false;

        NoteArrow arrow;
        if (isActive) //Currently isActive means an enemy note.
        {
            arrow = CM.AddArrowToLane(type, beat, newNote);
        }
        else
        {
            arrow = CM.AddArrowToLane(type, beat, newNote, new Color(1, 0.43f, 0.26f));
            NoteQueueParticlesFactory.NoteParticles(arrow, note.Texture, .5f);
        }

        if (!isActive)
            arrow.NoteHit();
        _laneData[(int)type][beat] = arrow;
        return true;
    }

    public override void _Ready()
    {
        MM = new MidiMaestro(StageProducer.Config.CurSong.MIDILocation);
    }

    public void Prep()
    {
        _laneData = new NoteArrow[][]
        {
            new NoteArrow[CM.BeatsPerLoop],
            new NoteArrow[CM.BeatsPerLoop],
            new NoteArrow[CM.BeatsPerLoop],
            new NoteArrow[CM.BeatsPerLoop],
        };
        AddInitialNotes();
    }

    private void AddInitialNotes()
    {
        foreach (ArrowType type in Enum.GetValues(typeof(ArrowType)))
        {
            foreach (midiNoteInfo mNote in MM.GetNotes(type))
            {
                AddNoteToLane(type, (int)mNote.GetStartTimeBeat(), Scribe.NoteDictionary[0]);
            }
        }
    }

    //Check all lanes for misses from missed inputs
    public void CheckMiss(double realBeat)
    {
        //On current beat, if prev beat is active and not inputted
        for (int i = 0; i < _laneData.Length; i++)
        {
            if (
                realBeat > CM.BeatsPerLoop
                || (
                    _laneLastBeat[i] >= Math.Floor(realBeat)
                    && (_laneLastBeat[i] < CM.BeatsPerLoop - 1 || Math.Floor(realBeat) != 0)
                )
            )
                continue;
            if (_laneData[i][_laneLastBeat[i]] == null || !_laneData[i][_laneLastBeat[i]].IsActive)
            {
                _laneLastBeat[i] = (_laneLastBeat[i] + 1) % CM.BeatsPerLoop;
                continue;
            }

            //Note exists and has been missed
            _laneData[i][_laneLastBeat[i]].NoteHit();
            TimedInput?.Invoke(
                GetNoteAt((ArrowType)i, _laneLastBeat[i]),
                (ArrowType)i,
                _laneLastBeat[i],
                1
            );
            _laneLastBeat[i] = (_laneLastBeat[i] + 1) % CM.BeatsPerLoop;
        }
    }

    public void CheckNoteTiming(ArrowType type)
    {
        double realBeat = TimeKeeper.CurrentTime / (60 / (double)TimeKeeper.Bpm) % CM.BeatsPerLoop;
        int curBeat = (int)Math.Round(realBeat);
        if (curBeat % CM.BeatsPerLoop == 0)
            return; //Ignore note 0 //TODO: Double check this works as intended.
        if (_laneData[(int)type][curBeat % CM.BeatsPerLoop] == null)
        {
            TimedInput?.Invoke(null, type, curBeat, Math.Abs(realBeat - curBeat));
            return;
        }

        if (!_laneData[(int)type][curBeat % CM.BeatsPerLoop].IsActive)
            return;
        double beatDif = Math.Abs(realBeat - curBeat);
        _laneData[(int)type][curBeat % CM.BeatsPerLoop].NoteHit();
        _laneLastBeat[(int)type] = (curBeat) % CM.BeatsPerLoop;
        TimedInput?.Invoke(GetNoteAt(type, curBeat), type, curBeat, beatDif);
    }
}
