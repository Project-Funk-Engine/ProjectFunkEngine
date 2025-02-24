using System;
using System.Linq;
using FunkEngine;
using Godot;

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

    private Node _arrowGroup;

    [Signal]
    public delegate void NotePressedEventHandler(ArrowType arrowType);

    [Signal]
    public delegate void NoteReleasedEventHandler(ArrowType arrowType);

    //Arbitrary vars, play with these
    private double ChartLength = 5000; //Might move this to be song specific?
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

    public void PrepChart(SongData songData)
    {
        _loopLen = songData.SongLength / songData.NumLoops;
        TimeKeeper.LoopLength = (float)_loopLen;
        BeatsPerLoop = (int)(_loopLen / (60f / songData.Bpm));
        ChartLength = (float)_loopLen * (float)Math.Floor(ChartLength / _loopLen);
        TimeKeeper.ChartLength = (float)ChartLength;
        TimeKeeper.Bpm = songData.Bpm;

        _arrowGroup = ChartLoopables.GetNode<Node>("ArrowGroup");

        IH.Connect(nameof(InputHandler.NotePressed), new Callable(this, nameof(OnNotePressed)));
        IH.Connect(nameof(InputHandler.NoteReleased), new Callable(this, nameof(OnNoteReleased)));

        //This could be good as a function to call on something, to have many things animated to the beat.
        var tween = GetTree().CreateTween();
        tween
            .TweenMethod(
                Callable.From((Vector2 scale) => TweenArrows(scale)),
                Vector2.One * .8f,
                Vector2.One,
                60f / TimeKeeper.Bpm / 2
            )
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Elastic);
        tween.TweenMethod(
            Callable.From((Vector2 scale) => TweenArrows(scale)),
            Vector2.One,
            Vector2.One * .8f,
            60f / TimeKeeper.Bpm / 2
        );
        tween.SetLoops().Play();
    }

    private void TweenArrows(Vector2 scale)
    {
        foreach (var node in _arrowGroup.GetChildren())
        {
            NoteArrow arrow = (NoteArrow)node;
            arrow.Scale = scale;
        }
    }

    public NoteArrow AddArrowToLane(
        ArrowType type,
        int beat,
        Note note,
        Color colorOverride = default
    )
    {
        var newNote = CreateNote(type, note.Name, beat); //TODO: Notes on track have unqiue visuals
        var loopArrow = CreateNote(type, note.Name, beat + BeatsPerLoop); //Create a dummy arrow for looping visuals
        if (colorOverride != default)
        {
            newNote.SelfModulate = colorOverride;
            loopArrow.SelfModulate = colorOverride;
        }
        newNote.NoteRef = note;
        return newNote;
    }

    private NoteArrow CreateNote(ArrowType arrow, string noteName, int beat = 0)
    {
        var noteScene = ResourceLoader.Load<PackedScene>("res://scenes/NoteManager/note.tscn");
        NoteArrow newArrow = noteScene.Instantiate<NoteArrow>();
        newArrow.Init(IH.Arrows[(int)arrow], beat, noteName);
        newArrow.OutlineSprite.Modulate = IH.Arrows[(int)arrow].Color;

        _arrowGroup.AddChild(newArrow);
        newArrow.Bounds = (float)((double)beat / BeatsPerLoop * (ChartLength / 2));
        newArrow.Position += Vector2.Right * newArrow.Bounds * 10; //temporary fix for notes spawning and instantly calling loop from originating at 0,0
        return newArrow;
    }

    public override void _ExitTree()
    {
        GD.Print("[DEBUG] Stopping tweens before exiting the scene...");

        foreach (var tween in GetTree().GetProcessedTweens())
        {
            tween.Stop();
            GD.Print("[DEBUG] Stopped tween.");
        }
    }
}
