using System;
using System.Linq;
using Godot;
using ArrowType = NoteArrow.ArrowType;

/**
 * @class ChartManager
 * @brief Chart Manager is meant to handle the visual aspects of a battle. Setting up the chart background, initial notes, and handle looping. WIP
 */
public partial class ChartManager : SubViewportContainer
{
    //Nodes from scene
    [Export]
    public InputHandler IH;

    [Export]
    public CanvasGroup ChartLoopables;

    [Signal]
    public delegate void NotePressedEventHandler(ArrowType arrowType);

    [Signal]
    public delegate void NoteReleasedEventHandler(ArrowType arrowType);

    //Arbitrary vars, play with these
    private double ChartLength = 2400; //Might move this to be song specific?
    private double _loopLen; //secs
    public int BeatsPerLoop;

    public void OnNotePressed(ArrowType type)
    {
        EmitSignal(nameof(NotePressed), (int)type);
    }

    public void OnNoteReleased(ArrowType type)
    {
        EmitSignal(nameof(NoteReleased), (int)type);
    }

    public void PrepChart(BattleDirector.SongData songData)
    {
        _loopLen = songData.SongLength / songData.NumLoops;
        TimeKeeper.LoopLength = (float)_loopLen;
        BeatsPerLoop = (int)(_loopLen / (60f / songData.Bpm));
        ChartLength = (float)_loopLen * (float)Math.Floor(ChartLength / _loopLen);
        TimeKeeper.ChartLength = (float)ChartLength;

        InitBackgrounds();

        IH.Connect(nameof(InputHandler.NotePressed), new Callable(this, nameof(OnNotePressed)));
        IH.Connect(nameof(InputHandler.NoteReleased), new Callable(this, nameof(OnNoteReleased)));
    }

    private void InitBackgrounds()
    {
        int i = 0;
        foreach (Node child in ChartLoopables.GetChildren())
        {
            if (child is not Loopable)
                continue;
            Loopable loopable = (Loopable)child;
            loopable.SetSize(new Vector2((float)ChartLength / 2 + 1, Size.Y));
            loopable.Bounds = (float)ChartLength / 2 * i;
            i++;
        }
    }

    //TODO: Rework these?
    public NoteArrow AddArrowToLane(Note note, int noteIdx)
    {
        var newNote = CreateNote(note.Type, note.Beat);
        CreateNote(note.Type, note.Beat + BeatsPerLoop); //Create a dummy arrow for looping visuals
        newNote.NoteIdx = noteIdx;
        return newNote;
    }

    private NoteArrow CreateNote(ArrowType arrow, int beat = 0)
    {
        var newNote = CreateNote(IH.Arrows[(int)arrow]);
        newNote.Bounds = (float)((double)beat / BeatsPerLoop * (ChartLength / 2));
        return newNote;
    }

    private NoteArrow CreateNote(InputHandler.ArrowData arrowData)
    {
        var noteScene = ResourceLoader.Load<PackedScene>("res://scenes/NoteManager/note.tscn");
        NoteArrow note = noteScene.Instantiate<NoteArrow>();
        note.Init(arrowData);
        ChartLoopables.AddChild(note);
        return note;
    }
}
