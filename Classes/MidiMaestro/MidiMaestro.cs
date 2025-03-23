using System;
using System.Linq;
using FunkEngine;
using Godot;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

/**
 <summary> MidiMaestro: Manages reading midi file into lane note information.</summary>

 */
public partial class MidiMaestro : Resource
{
    private MidiFile _midiFile;

    public static TempoMap TempoMap;
    public static TimeSignature TimeSignature;

    //The four note rows that we care about
    private midiNoteInfo[] _upNotes;
    private midiNoteInfo[] _downNotes;
    private midiNoteInfo[] _leftNotes;
    private midiNoteInfo[] _rightNotes;

    //private MidiFile strippedSong;

    //The path relative to the Audio folder. Will change later
    /**
     * <summary>Constrctor loads midi file and populates lane note arrays with midiNoteInfo</summary>
     * <param name="filePath">A string file path to a valid midi file</param>
     */
    public MidiMaestro(string filePath)
    {
        if (!OS.HasFeature("editor"))
        {
            filePath = OS.GetExecutablePath().GetBaseDir() + "/" + filePath;
        }

        if (!FileAccess.FileExists(filePath))
        {
            GD.PushError("ERROR: Unable to load level Midi file: " + filePath);
        }

        _midiFile = MidiFile.Read(filePath);
        TempoMap = _midiFile.GetTempoMap();
        TimeSignature = TempoMap.GetTimeSignatureAtTime(new MidiTimeSpan());

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
    }

    /**
     * <summary>Gets midiNoteInfo by lane. </summary>
     */
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
}

//A facade to wrap the midi notes. This is a simple class that wraps a Note object from the DryWetMidi library.
public class midiNoteInfo
{
    private readonly Melanchall.DryWetMidi.Interaction.Note _note;

    public midiNoteInfo(Melanchall.DryWetMidi.Interaction.Note note)
    {
        _note = note;
    }

    public long GetStartTimeBeat()
    {
        var beatsBar = _note.TimeAs<BarBeatTicksTimeSpan>(MidiMaestro.TempoMap);
        return beatsBar.Bars * MidiMaestro.TimeSignature.Numerator + beatsBar.Beats;
    }

    public long GetStartTimeTicks() => _note.Time;

    public float GetStartTimeSeconds() =>
        _note.TimeAs<MetricTimeSpan>(MidiMaestro.TempoMap).Milliseconds / 1000f
        + _note.TimeAs<MetricTimeSpan>(MidiMaestro.TempoMap).Seconds;

    public long GetEndTime() => _note.EndTime;

    public long GetDuration() => _note.Length;
}
