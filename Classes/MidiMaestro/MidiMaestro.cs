using System;
using System.Linq;
using FunkEngine;
using Godot;

/**
 <summary> MidiMaestro: Manages reading a song map file into lane note information.</summary>

 */
public partial class MidiMaestro : Resource
{
    //The four note rows that we care about
    private readonly NoteInfo[] _upNotes;
    private readonly NoteInfo[] _downNotes;
    private readonly NoteInfo[] _leftNotes;
    private readonly NoteInfo[] _rightNotes;

    /**
     * <summary>Constructor loads resource file and populates lane note arrays with NoteInfo</summary>
     * <param name="filePath">A string file path to a valid songMap .tres file</param>
     */
    public MidiMaestro(NoteChart savedChart)
    {
        if (savedChart != null)
        {
            _upNotes = savedChart.GetLane(ArrowType.Up).ToArray();
            _downNotes = savedChart.GetLane(ArrowType.Down).ToArray();
            _leftNotes = savedChart.GetLane(ArrowType.Left).ToArray();
            _rightNotes = savedChart.GetLane(ArrowType.Right).ToArray();
        }
        else
        {
            GD.PushError("ERROR: Unable to load songMap resource file!");
            _upNotes = [];
            _downNotes = [];
            _leftNotes = [];
            _rightNotes = [];
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
