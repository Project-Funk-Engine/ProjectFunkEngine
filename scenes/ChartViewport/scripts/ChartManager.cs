using System;
using System.Linq;
using FunkEngine;
using Godot;

/**<summary>Manager for handling the visual aspects of a battle. Placing visual NoteArrows, looping visuals, and combo text.</summary>
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
    //Might move this to be song specific? For now, should never go below ~2000, else visual break because there isn't enough room to loop.
    private double ChartLength = 5000;
    private double _loopLen; //secs
    public double TrueBeatsPerLoop;
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
        TrueBeatsPerLoop = (_loopLen / (60f / songData.Bpm));
        BeatsPerLoop = (int)TrueBeatsPerLoop;
        ChartLength = (float)_loopLen * (float)Math.Floor(ChartLength / _loopLen);
        TimeKeeper.ChartLength = (float)ChartLength;
        TimeKeeper.Bpm = songData.Bpm;

        _arrowGroup = ChartLoopables.GetNode<Node>("ArrowGroup");

        IH.Connect(nameof(InputHandler.NotePressed), new Callable(this, nameof(OnNotePressed)));
        IH.Connect(nameof(InputHandler.NoteReleased), new Callable(this, nameof(OnNoteReleased)));
    }

    public void BeginTweens()
    {
        //This could be good as a function to call on something, to have many things animated to the beat.
        var tween = CreateTween();
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
        var newNote = CreateNote(type, note, beat);
        var loopArrow = CreateNote(type, note, beat, 1); //Create a dummy arrow for looping visuals
        if (colorOverride != default)
        {
            newNote.SelfModulate = colorOverride;
            loopArrow.SelfModulate = colorOverride;
        }
        newNote.NoteRef = note;
        return newNote;
    }

    private NoteArrow CreateNote(ArrowType arrow, Note note, int beat = 0, int loopOffset = 0)
    {
        var noteScene = ResourceLoader.Load<PackedScene>("res://Scenes/NoteManager/NoteArrow.tscn");
        NoteArrow newArrow = noteScene.Instantiate<NoteArrow>();
        newArrow.Bounds = (float)(
            beat / TrueBeatsPerLoop * (ChartLength / 2) + loopOffset * (ChartLength / 2)
        );
        newArrow.Init(IH.Arrows[(int)arrow], beat, note);
        newArrow.OutlineSprite.Modulate = IH.Arrows[(int)arrow].Color;

        _arrowGroup.AddChild(newArrow);
        return newArrow;
    }

    public void ComboText(string text, ArrowType arrow, int currentCombo)
    {
        TextParticle newText = new TextParticle();
        AddChild(newText);
        newText.Position = IH.Arrows[(int)arrow].Node.Position - newText.Size / 2;
        IH.FeedbackEffect(arrow, text);
        newText.Text = text + $" {currentCombo}";
    }
}
