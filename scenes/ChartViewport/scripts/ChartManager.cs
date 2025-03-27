using System;
using FunkEngine;
using Godot;

/**<summary>Manager for handling the visual aspects of a battle. Placing visual NoteArrows, looping visuals, and combo text.</summary>
 */
public partial class ChartManager : SubViewportContainer
{
    [Export]
    public InputHandler IH;

    [Export]
    public CanvasGroup ChartLoopables;

    private Node _arrowGroup;

    [Signal]
    public delegate void NotePressedEventHandler(ArrowType arrowType);

    [Signal]
    public delegate void NoteReleasedEventHandler(ArrowType arrowType);

    //Might move this to be song specific?
    private double ChartLength = 1;

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

        //99% sure chart length can never be less than (chart viewport width) * 2,
        //otherwise there isn't room for things to loop from off and on screen
        ChartLength = Math.Max(
            (float)_loopLen * (float)Math.Ceiling(Size.X * 2 / _loopLen),
            //Also minimize rounding point imprecision, improvement is qualitative
            (float)_loopLen * (float)Math.Floor(ChartLength / _loopLen)
        );

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
        Color colorOverride = default,
        NoteArrow pooledArrow = null
    )
    {
        var newNote = CreateNote(type, note, beat, pooledArrow);
        if (colorOverride != default)
            newNote.SelfModulate = colorOverride;
        newNote.NoteRef = note;
        return newNote;
    }

    private NoteArrow CreateNote(ArrowType arrow, Note note, int beat, NoteArrow pooledArrow)
    {
        NoteArrow newArrow;
        if (pooledArrow == null)
        {
            var noteScene = ResourceLoader.Load<PackedScene>(NoteArrow.LoadPath);
            newArrow = noteScene.Instantiate<NoteArrow>();
            _arrowGroup.AddChild(newArrow);
        }
        else
        {
            newArrow = pooledArrow;
            newArrow.Recycle();
        }
        newArrow.BeatTime = (float)(beat / TrueBeatsPerLoop * _loopLen);

        newArrow.Init(IH.Arrows[(int)arrow], beat, note);
        newArrow.OutlineSprite.Modulate = IH.Arrows[(int)arrow].Color;

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
