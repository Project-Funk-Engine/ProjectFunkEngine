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

    public delegate void InputHandler(ArrowData data, double beatDif);
    public event InputHandler NoteInputEvent;

    private readonly List<ArrowData> _noteData = new List<ArrowData>();

    private double _beatSpawnOffset;

    public override void _Ready()
    {
        MM = new MidiMaestro(StageProducer.Config.CurSong.MIDILocation);
        CM.ArrowFromInput += HandleNote;
    }

    private bool _initialized;

    public void Initialize(SongData curSong)
    {
        if (_initialized)
            return;

        CM.Initialize(curSong);

        //Approximately the first note offscreen
        _beatSpawnOffset = Math.Floor(CM.Size.X / TimeKeeper.ChartLength * CM.TrueBeatsPerLoop);
        AddInitialNotes();
        SpawnInitialNotes();

        _initialized = true;
    }

    private int AddNoteData(Note noteRef, ArrowType type, Beat beat, double length = 0)
    {
        ArrowData result = new ArrowData(type, beat, noteRef, length);
        if (_noteData.Count == 0)
        {
            _noteData.Add(result);
            return 0;
        }

        int index = _noteData.BinarySearch(result); //TODO: This sorts correctly, but we don't take advantage yet.
        if (index > 0)
        {
            GD.PushWarning("Duplicate note attempted add " + type + " " + beat);
            return -1;
        }
        _noteData.Insert(~index, result);
        return ~index;
    }

    private void AddInitialNotes()
    {
        foreach (ArrowType type in Enum.GetValues(typeof(ArrowType)))
        {
            foreach (MidiNoteInfo mNote in MM.GetNotes(type))
            {
                AddNoteData(
                    Scribe.NoteDictionary[0],
                    type,
                    new Beat((int)mNote.GetStartTimeBeat()),
                    mNote.GetDurationBeats()
                );
            }
        }
    }

    private void SpawnInitialNotes()
    {
        for (int i = 1; i <= _beatSpawnOffset; i++)
        {
            SpawnNotesAtBeat(new Beat(i));
        }
    }

    public void AddPlayerNote(Note noteRef, ArrowType type, Beat beat)
    {
        int index = AddNoteData(noteRef, type, beat); //Currently player notes aren't sorted correctly
        if (index != -1)
            SpawnNote(index, true);
        else
            GD.PushError("Duplicate player note was attempted. (This should be stopped by CM)");
    }

    public void ProgressiveAddNotes(Beat beat)
    {
        Beat spawnBeat = beat + _beatSpawnOffset;
        SpawnNotesAtBeat(spawnBeat);
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
        _noteData[index] = new ArrowData(
            _noteData[index].Type,
            _noteData[index].Beat.IncDecLoop(1),
            _noteData[index].NoteRef,
            _noteData[index].Length
        ); //Structs make me sad sometimes
    }

    private void HandleNote(ArrowData data)
    {
        NoteInputEvent?.Invoke(data, GetTimingDif(data.Beat));
    }

    private double GetTimingDif(Beat beat)
    {
        //Hmm, this is only ever an issue with possibly reaching beat 1 from just under beat 0, not sure if that'd happen
        if (beat.Loop != TimeKeeper.LastBeat.Loop)
            return 1;
        return Math.Abs(beat.BeatPos - TimeKeeper.LastBeat.BeatPos);
    }
}
