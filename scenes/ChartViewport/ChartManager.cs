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
    public NoteManager NM;

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

    private NoteArrow[][] _currentArrows = new NoteArrow[][]
    {
        Array.Empty<NoteArrow>(),
        Array.Empty<NoteArrow>(),
        Array.Empty<NoteArrow>(),
        Array.Empty<NoteArrow>(),
    };

    private void InitBackgrounds()
    {
        //TODO: Get better visual for BG's
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

    private void InitNotes(Note[] notes)
    {
        foreach (Note noteData in notes)
        {
            if (noteData != null)
                CreateNote(noteData.Type, noteData.Beat);
        }
        foreach (Note noteData in notes) //Temporary solution
        {
            if (noteData != null)
                CreateNote(noteData.Type, noteData.Beat + BeatsPerLoop);
        }
    }

    public void PrepChart(BattleDirector.SongData songData, Note[] notes)
    {
        _loopLen = songData.SongLength / songData.NumLoops;
        TimeKeeper.LoopLength = (float)_loopLen;
        BeatsPerLoop = (int)(_loopLen / (60f / songData.Bpm));
        ChartLength = (float)_loopLen * (float)Math.Floor(ChartLength / _loopLen);
        TimeKeeper.ChartLength = (float)ChartLength;

        InitBackgrounds();
        InitNotes(notes);

        NM.Connect(nameof(NoteManager.NotePressed), new Callable(this, nameof(OnNotePressed)));
        NM.Connect(nameof(NoteManager.NoteReleased), new Callable(this, nameof(OnNoteReleased)));
    }

    //TODO: Rework these?
    public NoteArrow CreateNote(ArrowType arrow, int beat = 0)
    {
        var newNote = CreateNote(NM.Arrows[(int)arrow], beat);
        newNote.Bounds = (float)((double)beat / BeatsPerLoop * (ChartLength / 2));
        return newNote;
    }

    private NoteArrow CreateNote(NoteManager.ArrowData arrowData, int beat)
    {
        var noteScene = ResourceLoader.Load<PackedScene>("res://scenes/NoteManager/note.tscn");
        NoteArrow note = noteScene.Instantiate<NoteArrow>();
        note.Init(arrowData);

        if (!(beat > BeatsPerLoop)) //All visual notes need a second to loop effectively. The second set should not be put in the queue.
        {
            _currentArrows[(int)arrowData.Type] = _currentArrows[(int)arrowData.Type]
                .Append(note)
                .ToArray();
        }
        ChartLoopables.AddChild(note);
        return note;
    }

    public void HandleNote(ArrowType type)
    {
        _currentArrows[(int)type].First().NoteHit();
        _currentArrows[(int)type] = _currentArrows[(int)type]
            .Skip(1)
            .Concat(_currentArrows[(int)type].Take(1))
            .ToArray();
    }

    public void OnNotePressed(ArrowType type)
    {
        // removed this bit of code, since placement only worked on lines that already
        // had arrows before the player added any. if needed we can add this back
        // once we use charts that have arrows in all 4 rows from the beginning
        //if (_currentArrows[(int)type].Length == 0)
        //return;

        EmitSignal(nameof(NotePressed), (int)type);
    }

    public void OnNoteReleased(ArrowType type)
    {
        EmitSignal(nameof(NoteReleased), (int)type);
    }
}
