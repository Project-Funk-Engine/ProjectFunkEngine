using System;
using Godot;
using ArrowType = NoteArrow.ArrowType;

/*
//Lets say this inits all the initial notes and manages the chart BG.

//What does this do?
//Input, visual looping, timing, battle stuff, combo, note creation

//Focus on the looping
 This should manage creating sprites for notes???
 This should manage subview camera pos and zoom.

Movement should primarily be done from a parent node
!!Backgrounds need to be big enough/notes and beats spaced enough that when a note goes off screen, it can be teleported in position offscreen
BackGround probably needs 2 sprites or parallax:
    Get a set length, based on viewport and loop/song length (Const PLAYWIDTH)
    Once one BG hits a certain left pos return it to the right pos

Notes are similar, but only need 1 representation.
    Once hits left bounds return to right bounds
    (Something else should probably manage refreshing, input, etc)
    Can probably use an object pool

If timing based input checking:
    This is enough, notes are visually just sprites
    Collision based - This might need to manage that, or have a sister manager that does, notes need more stuff on their own
 */

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

    public BattleDirector.SongData SongData; //TODO: Maybe. Make settable from outside, but readonly

    //Arbitrary vars, play with these
    private const double ChartLength = 1400; //Might move this to be song specific?

    //Speed that chart objs should move at, to be synced to song, in theory
    private double _rateOfChart;
    private double _loopLen; //secs
    private int _beatsPerLoop;

    private void InitBackgrounds()
    {
        //TODO: Get better visual for BG's, and/or create BG's on demand. Though we should only ever need 2.
        int i = 0;
        foreach (Node child in ChartLoopables.GetChildren())
        {
            if (child is Loopable)
            {
                Loopable loopable = (Loopable)child;
                //TODO: Consolidate
                loopable.SetSize(new Vector2((float)ChartLength / 2 + 1, Size.Y));
                loopable.SetPosition(new Vector2((float)ChartLength / 2 * i, 0));
                loopable.Bounds = (float)ChartLength / 2;
                loopable.Speed = (float)_rateOfChart;

                i++;
            }
        }
    }

    private void InitNotes(Note[] notes)
    {
        foreach (Note noteData in notes)
        {
            CreateNote(noteData.Type, noteData.Beat);
        }
    }

    public void PrepChart(BattleDirector.SongData songData, Note[] notes)
    {
        SongData = songData;

        _loopLen = SongData.SongLength / SongData.NumLoops;
        _beatsPerLoop = (int)(_loopLen / (60f / SongData.Bpm));

        _rateOfChart = 700 / _loopLen; //px/s

        InitBackgrounds();
        InitNotes(notes);
    }

    //TODO: Rework these
    public NoteArrow CreateNote(ArrowType arrow, int beat = 0)
    {
        var newNote = CreateNote(NM.Arrows[arrow]);
        newNote.Bounds =
            (float)((double)beat / _beatsPerLoop * (ChartLength / 2)) - newNote.Size.X / 2; //eww
        newNote.Position += Vector2.Right * newNote.Bounds;
        return newNote;
    }

    private NoteArrow CreateNote(NoteManager.ArrowData arrowData)
    {
        var noteScene = ResourceLoader.Load<PackedScene>("res://scenes/NoteManager/note.tscn");
        var note = noteScene.Instantiate<NoteArrow>();

        note.Init(arrowData, (float)_rateOfChart, -1);

        ChartLoopables.AddChild(note);
        return note;
    }

    //TODO: Queue next notes. Needs Timing System
    /*The logic:
     *Spawn in pos is a proportion (intended beat/beats per loop) = (intended pos/track length in px) ->
     *		intended pos = intended beat / bpl * track length
     *
     *		Respawn (probably obj pool, or for now new instantiation)
     *			When a note's pos is at its intended initial pos, queue up and spawn the next note of its beat at intended pos + track length
     */
}
