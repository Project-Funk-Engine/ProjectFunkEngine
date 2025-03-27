using System;
using FunkEngine;
using Godot;

/**<summary>Conductor: Arm of BattleDirector for handling note lanes and timing.</summary>
 */
public partial class Conductor : Node
{
    [Export]
    private ChartManager CM;
    public MidiMaestro MM;

    public delegate void TimedInputHandler(Note note, ArrowType type, Beat beat, double beatDif);
    public event TimedInputHandler TimedInput;

    private double _beatSpawnOffset;

    //Assume queue structure for notes in each lane.
    //Can eventually make this its own structure
    private NoteArrow[][] _laneData = Array.Empty<NoteArrow[]>();

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

    public bool AddNoteToLane(
        ArrowType type,
        int beat,
        Note note,
        bool isActive = true,
        NoteArrow pooledArrow = null
    )
    {
        beat %= CM.BeatsPerLoop;
        Note newNote = note.Clone();
        if (beat == 0 || _laneData[(int)type][beat] != null) //TODO: Double check if this is still necessary, doesn't seem to matter for player placed notes
            return false;

        NoteArrow arrow;
        if (isActive) //Currently isActive means an enemy note.
        {
            arrow = CM.AddArrowToLane(type, new Beat(beat), newNote, default, pooledArrow);
        }
        else
        {
            arrow = CM.AddArrowToLane(
                type,
                new Beat(beat),
                newNote,
                new Color(1, 0.43f, 0.26f),
                pooledArrow
            );
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
        CM.Missed += OnEventCheckMiss;
    }

    public void Prep()
    {
        //-1 is slightly arbitrary, there were strange results without it.
        _beatSpawnOffset = Math.Round(CM.Size.X / TimeKeeper.ChartLength * CM.TrueBeatsPerLoop - 1);
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

    public void ProgressiveAddNotes(Beat beat)
    {
        Beat spawnBeat = beat + _beatSpawnOffset;
        int beatIdx = (int)spawnBeat.BeatPos % CM.BeatsPerLoop;
        foreach (ArrowType type in Enum.GetValues(typeof(ArrowType)))
        {
            if (_laneData[(int)type][beatIdx] != null)
            {
                CM.AddArrowToLane(
                    type,
                    new Beat(beatIdx, spawnBeat.Loop),
                    _laneData[(int)type][beatIdx].NoteRef,
                    default,
                    _laneData[(int)type][beatIdx]
                );
            }
        }
    }

    //Check all lanes for misses from missed inputs
    private void OnEventCheckMiss(NoteArrow arrow)
    {
        TimedInput?.Invoke(arrow.NoteRef, arrow.Type, arrow.Beat, 1);
    }

    public void CheckNoteTiming(ArrowType type)
    {
        double realBeat = TimeKeeper.LastBeat.BeatPos;
        int curBeat = (int)Math.Round(realBeat);

        if (curBeat % CM.BeatsPerLoop == 0)
            return; //Ignore note 0 //TODO: Double check this works as intended.
        if (_laneData[(int)type][curBeat % CM.BeatsPerLoop] == null)
        {
            TimedInput?.Invoke(null, type, new Beat(curBeat), Math.Abs(realBeat - curBeat));
            return;
        }
        if (!_laneData[(int)type][curBeat % CM.BeatsPerLoop].IsActive)
            return;

        double beatDif = Math.Abs(realBeat - curBeat);
        _laneData[(int)type][curBeat % CM.BeatsPerLoop].NoteHit();
        TimedInput?.Invoke(GetNoteAt(type, curBeat), type, new Beat(curBeat), beatDif);
    }
}
