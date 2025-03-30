using System;
using System.Collections.Generic;
using System.Linq;
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

    public delegate void InputEventHandler(NoteArrowData noteData);
    public event InputEventHandler ArrowFromInput;

    private readonly List<NoteArrow> _arrowPool = new List<NoteArrow>();

    private readonly List<NoteArrow>[] _queuedArrows = new List<NoteArrow>[]
    {
        new(),
        new(),
        new(),
        new(),
    };

    private double _loopLen; //secs
    public double TrueBeatsPerLoop;

    private double _chartLength = 2500; //Play with this

    public void OnNotePressed(ArrowType type)
    {
        if (TimeKeeper.LastBeat.CompareTo(Beat.Zero) == 0)
            return;
        ArrowFromInput?.Invoke(NextArrowFrom(type));
    }

    public override void _Ready()
    {
        _arrowGroup = ChartLoopables.GetNode<Node>("ArrowGroup");

        IH.Connect(nameof(InputHandler.NotePressed), new Callable(this, nameof(OnNotePressed)));
        //IH.Connect(nameof(InputHandler.NoteReleased), new Callable(this, nameof(OnNoteReleased)));
    }

    private bool _initialized = false;

    public void Initialize(SongData songData)
    {
        if (_initialized)
            return;
        _loopLen = songData.SongLength / songData.NumLoops;
        TimeKeeper.LoopLength = _loopLen;

        TrueBeatsPerLoop = (_loopLen / (60f / songData.Bpm));
        TimeKeeper.BeatsPerLoop = TrueBeatsPerLoop;

        //99% sure chart length can never be less than (chart viewport width) * 2,
        //otherwise there isn't room for things to loop from off and on screen
        _chartLength = Math.Max(
            _loopLen * Math.Ceiling(Size.X * 2 / _loopLen),
            //Also minimize rounding point imprecision, improvement is qualitative
            _loopLen * Math.Floor(_chartLength / _loopLen)
        );

        TimeKeeper.ChartLength = _chartLength;
        TimeKeeper.Bpm = songData.Bpm;

        _initialized = true;
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

    public void AddNoteArrow(NoteArrowData noteArrowData, bool preHit = false)
    {
        NoteArrow noteArrow = _arrowPool.Count == 0 ? InstatiateNewArrow() : DePoolArrow();
        noteArrow.Init(
            IH.Arrows[(int)noteArrowData.Type],
            noteArrowData,
            TimeFromBeat(noteArrowData.Beat)
        );
        if (noteArrowData.NoteRef.IsPlayerNote())
            noteArrow.SelfModulate = PlayerPuppet.NoteColor;
        if (preHit)
            noteArrow.NoteHit();
    }

    private NoteArrow InstatiateNewArrow()
    {
        NoteArrow result = ResourceLoader
            .Load<PackedScene>(NoteArrow.LoadPath)
            .Instantiate<NoteArrow>();
        result.Missed += OnArrowMissed;
        result.QueueForHit += OnArrowHittable;
        result.QueueForPool += PoolArrow;
        _arrowGroup.AddChild(result);
        return result;
    }

    private void OnArrowHittable(NoteArrow noteArrow)
    {
        _queuedArrows[(int)noteArrow.Type].Add(noteArrow);
    }

    private void OnArrowMissed(NoteArrow noteArrow)
    {
        noteArrow.NoteHit();
        ArrowFromInput?.Invoke(noteArrow.Data);
    }

    private void PoolArrow(NoteArrow noteArrow)
    {
        int index = _queuedArrows[(int)noteArrow.Type].IndexOf(noteArrow);
        if (index != -1)
            _queuedArrows[(int)noteArrow.Type].RemoveAt(index);
        noteArrow.ProcessMode = ProcessModeEnum.Disabled;
        noteArrow.Visible = false;
        _arrowPool.Add(noteArrow);
    }

    private NoteArrow DePoolArrow()
    {
        NoteArrow res = _arrowPool[0];
        _arrowPool.RemoveAt(0);
        res.Recycle();
        res.SelfModulate = Colors.White;
        return res;
    }

    public double TimeFromBeat(Beat beat)
    {
        return (beat.BeatPos / TrueBeatsPerLoop * _loopLen);
    }

    //TODO
    private NoteArrowData NextArrowFrom(ArrowType type)
    {
        NoteArrowData placeableNote = new NoteArrowData(
            type,
            TimeKeeper.LastBeat.RoundBeat(),
            null
        );
        if (_queuedArrows[(int)type].Count == 0)
            return placeableNote; //Empty return null, place note action
        List<NoteArrow> activeArrows = _queuedArrows[(int)type]
            .Where(arrow =>
                !arrow.IsHit
                && Math.Abs((arrow.Beat - TimeKeeper.LastBeat).BeatPos) <= Note.TimingMax
            )
            .OrderBy(arrow => Math.Abs((arrow.Beat - TimeKeeper.LastBeat).BeatPos))
            .ToList();
        if (activeArrows.Count != 0) //There is an active note in hittable range activate it and pass it
        {
            activeArrows[0].NoteHit();
            return activeArrows[0].Data;
        }

        int index = _queuedArrows[(int)type]
            .FindIndex(arrow =>
                (int)Math.Round(arrow.Beat.BeatPos) == (int)Math.Round(TimeKeeper.LastBeat.BeatPos)
            );
        if (index != -1) //There is an inactive note in the whole beat, pass it something so no new note is placed
            return NoteArrowData.Placeholder;
        return placeableNote; //No truly hittable notes, and no notes in current beat
    }

    public void ComboText(Timing timed, ArrowType arrow, int currentCombo)
    {
        TextParticle newText = new TextParticle();
        AddChild(newText);
        newText.Position = IH.Arrows[(int)arrow].Node.Position - newText.Size / 2;
        IH.FeedbackEffect(arrow, timed);
        newText.Text = Tr("BATTLE_ROOM_" + timed.ToString().ToUpper()) + $" {currentCombo}";
    }
}
