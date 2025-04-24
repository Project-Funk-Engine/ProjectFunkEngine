using System;
using System.Linq;
using FunkEngine;
using Godot;

/**
 <summary> MidiMaestro: Manages reading midi file into lane note information.</summary>

 */
public partial class MidiMaestro : Resource
{
    //The four note rows that we care about
    private readonly NoteInfo[] _upNotes;
    private readonly NoteInfo[] _downNotes;
    private readonly NoteInfo[] _leftNotes;
    private readonly NoteInfo[] _rightNotes;

    //private MidiFile strippedSong;
    /**
     * <summary>Constructor loads midi file and populates lane note arrays with midiNoteInfo</summary>
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

        NoteChart savedChart = ResourceLoader.Load<NoteChart>(filePath);

        if (savedChart != null)
        {
            _upNotes = savedChart.GetLane(ArrowType.Up).ToArray();
            _downNotes = savedChart.GetLane(ArrowType.Down).ToArray();
            _leftNotes = savedChart.GetLane(ArrowType.Left).ToArray();
            _rightNotes = savedChart.GetLane(ArrowType.Right).ToArray();
        }
        else
        {
            GD.PushError("ERROR: Unable to load level Midi file: " + filePath);
            _upNotes = Array.Empty<NoteInfo>();
            _downNotes = Array.Empty<NoteInfo>();
            _leftNotes = Array.Empty<NoteInfo>();
            _rightNotes = Array.Empty<NoteInfo>();
        }
    }

    /**
     * <summary>Gets NoteInfo by lane. </summary>
     */
    public NoteInfo[] GetNotes(ArrowType arrowType)
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
