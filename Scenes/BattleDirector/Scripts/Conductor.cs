using System;
using System.Collections.Generic;
using FunkEngine;
using Godot;

/**<summary>Conductor: Arm of BattleDirector for handling note lanes and timing.</summary>
 */
public partial class Conductor : Node
{
    [Export]
    private ChartManager CM;
    private MidiMaestro MM;

    private readonly List<ArrowData> _noteData = new List<ArrowData>();

    private double _beatSpawnOffset;

    private bool _initialized;

    #region Initialization
    public void Initialize(SongData curSong)
    {
        if (_initialized)
            return;

        MM = new MidiMaestro(StageProducer.Config.CurSong.SongMapLocation);
        CM.ArrowFromInput += ReceiveNoteInput;

        CM.Initialize(curSong);

        //Approximately the first note offscreen
        _beatSpawnOffset = Math.Ceiling(
            CM.Size.X / TimeKeeper.ChartWidth * TimeKeeper.BeatsPerLoop
        );
        AddInitialNotes();

        _initialized = true;
    }

    private void AddInitialNotes()
    {
        foreach (ArrowType type in Enum.GetValues(typeof(ArrowType)))
        {
            foreach (NoteInfo Note in MM.GetNotes(type))
            {
                AddNoteData(Scribe.NoteDictionary[0], type, new Beat((int)Note.Beat), Note.Length);
            }
        }
        SpawnInitialNotes();
    }

    private void SpawnInitialNotes()
    {
        for (int i = 1; i <= _beatSpawnOffset; i++)
        {
            SpawnNotesAtBeat(new Beat(i));
        }
    }

    public delegate void InputHandler(ArrowData data, double beatDif);
    public event InputHandler NoteInputEvent;

    private void ReceiveNoteInput(ArrowData data)
    {
        NoteInputEvent?.Invoke(data, GetTimingDif(data.Beat));
    }
    #endregion

    //Ignores sorting, use sparingly
    private void AddNoteData(ArrowData data, int index)
    {
        if (index == -1)
        {
            GD.PushWarning(
                "Specific invalid index attempted to be passed (is -1): "
                    + data.Type
                    + " "
                    + data.Beat
            );
            return;
        }

        if (index < -1 || index > _noteData.Count)
        {
            GD.PushWarning(
                "Invalid index passed is: " + index + " data: " + data.Type + " " + data.Beat
            );
            return;
        }
        _noteData.Insert(index, data);
    }

    private int AddNoteData(Note noteRef, ArrowType type, Beat beat, double length = 0)
    {
        ArrowData result = new ArrowData(type, beat, noteRef, length);
        return AddNoteData(result);
    }

    private int AddNoteData(ArrowData data)
    {
        if (_noteData.Count == 0)
        {
            _noteData.Add(data);
            return 0;
        }

        int index = GetIndexOfData(data);
        if (index == -1)
        {
            GD.PushWarning(
                "Attempted to add duplicate note! Current note: " + data.Type + " " + data.Beat
            );
            return -1;
        }

        _noteData.Insert(index, data);
        return index;
    }

    private int GetIndexOfData(ArrowData data)
    {
        int index = _noteData.BinarySearch(data);
        if (index > 0)
        {
            GD.PushWarning(index + " Invalid index for: " + data.Type + " " + data.Beat);
            return -1;
        }

        return ~index;
    }

    //TODO: Beat spawn redundancy checking, efficiency
    private void SpawnNotesAtBeat(Beat beat)
    {
        for (int i = 0; i < _noteData.Count; i++)
        {
            if (
                _noteData[i].Beat.Loop != beat.Loop
                || (int)_noteData[i].Beat.BeatPos != (int)beat.BeatPos
            )
                continue;
            SpawnNote(i);
        }
    }

    private void SpawnNote(int index, bool newPlayerNote = false)
    {
        CM.AddNoteArrow(_noteData[index], newPlayerNote);
        _noteData[index] = _noteData[index].IncDecLoop(1);
    }

    /// <summary>
    /// Attempts to add the new specified beat to current chart. <br></br>
    /// Should only be used from On Loop effects <br></br>
    /// EXPERIMENTAL - Currently does not check for: Hold note overlap whether placing or preexisting.
    /// Notes that are far past when they should have been placed. And notes that will come around far in the future.
    /// </summary>
    /// <param name="currentTime">Current time, should be the beat of the about to happen new loop.</param>
    /// <param name="noteRef">What type of note</param>
    /// <param name="type">The lane</param>
    /// <param name="beat">Beat to spawn new note at. Should be within a loop, but outside _beatSpawnOffset buffer.</param>
    /// <param name="length">Length of note, may get removed.</param>
    /// <returns>Whether placement was successful</returns>
    public bool AddConcurrentNote(
        Beat currentTime,
        Note noteRef,
        ArrowType type,
        Beat beat,
        float length = 0
    )
    {
        if (beat < currentTime + _beatSpawnOffset)
        {
            GD.PushWarning(
                "Attempted note far in past. Current beat: "
                    + currentTime
                    + " notedata: "
                    + type
                    + " "
                    + beat
            );
            return false;
        }
        if (beat > currentTime.IncDecLoop(1) + _beatSpawnOffset)
        {
            GD.PushWarning(
                "Attempted note too far in future. Current beat: "
                    + currentTime
                    + " notedata: "
                    + type
                    + " "
                    + beat
            );
            return false;
        }
        //Beat should now be guaranteed to be coming up, at some point, from ProgressiveSpawnNote
        int index = AddNoteData(noteRef, type, beat, length);
        if (index == -1)
            return false; //Assumption: Dupe note.
        return true;
    }

    public void AddPlayerNote(Note noteRef, ArrowType type, Beat beat)
    {
        Beat compBeat = new Beat(beat.BeatPos, beat.Loop + 1);
        int index = GetIndexOfData(new ArrowData(type, compBeat, null)); //Player notes should sorted based on immediately incrementing loop
        if (index != -1)
        {
            AddNoteData(new ArrowData(type, beat, noteRef), index);
            SpawnNote(index, true);
        }
        else
            GD.PushError("Duplicate player note was attempted. (This should be stopped by CM)");
    }

    public void ProgressiveSpawnNotes(Beat beat)
    {
        Beat spawnBeat = beat + _beatSpawnOffset;
        SpawnNotesAtBeat(spawnBeat.RoundBeat());
    }

    private double GetTimingDif(Beat beat)
    {
        //Hmm, this is only ever an issue with possibly reaching beat 1 from just under beat 0, not sure if that'd happen
        if (beat.Loop != TimeKeeper.LastBeat.Loop)
            return 1;
        return Math.Abs(beat.BeatPos - TimeKeeper.LastBeat.BeatPos);
    }
}
