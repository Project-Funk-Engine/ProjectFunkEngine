using System;
using System.Linq;
using FunkEngine;
using Godot;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

public partial class MidiMaestro : Resource
{
    private MidiFile _midiFile;

    //The four note rows that we care about
    private midiNoteInfo[] _upNotes;
    private midiNoteInfo[] _downNotes;
    private midiNoteInfo[] _leftNotes;
    private midiNoteInfo[] _rightNotes;

    private SongData songData;

    //The path relative to the Audio folder. Will change later
    public MidiMaestro(string filePath)
    {
        if (!FileAccess.FileExists(filePath))
        {
            GD.PrintErr("ERROR: Unable to load level Midi file: " + filePath);
        }

        _midiFile = MidiFile.Read(filePath);

        //Strip out the notes from the midi file
        foreach (var track in _midiFile.GetTrackChunks())
        {
            string trackName = track.Events.OfType<SequenceTrackNameEvent>().FirstOrDefault()?.Text;
            midiNoteInfo[] noteEvents = track
                .GetNotes()
                .Select(note => new midiNoteInfo(note))
                .ToArray();

            switch (trackName)
            {
                case "Up":
                    _upNotes = noteEvents;
                    break;
                case "Down":
                    _downNotes = noteEvents;
                    break;
                case "Left":
                    _leftNotes = noteEvents;
                    break;
                case "Right":
                    _rightNotes = noteEvents;
                    break;
            }
        }

        //Populate the song data
        songData = new SongData
        {
            //TODO: Allow for changes in this data
            Bpm = 120,
            //Fudge the numbers a bit if we have a really short song
            SongLength =
                _midiFile.GetDuration<MetricTimeSpan>().Seconds < 20
                    ? 20
                    : _midiFile.GetDuration<MetricTimeSpan>().Seconds,
            NumLoops = 1,
        };
    }

    public midiNoteInfo[] GetNotes(ArrowType arrowType)
    {
        return arrowType switch
        {
            ArrowType.Up => _upNotes,
            ArrowType.Down => _downNotes,
            ArrowType.Left => _leftNotes,
            ArrowType.Right => _rightNotes,
            _ => throw new ArgumentOutOfRangeException(nameof(arrowType), arrowType, null),
        };
    }

    public SongData GetSongData()
    {
        return songData;
    }
}

//A facade to wrap the midi notes. This is a simple class that wraps a Note object from the DryWetMidi library.
public class midiNoteInfo
{
    private readonly Melanchall.DryWetMidi.Interaction.Note _note;

    public midiNoteInfo(Melanchall.DryWetMidi.Interaction.Note note)
    {
        _note = note;
    }

    public long GetStartTime() => _note.Time;

    public long GetEndTime() => _note.EndTime;

    public long GetDuration() => _note.Length;
}
