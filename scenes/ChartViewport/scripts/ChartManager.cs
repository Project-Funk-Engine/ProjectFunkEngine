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

    private readonly List<NoteArrow> _arrowPool = new();
    private readonly List<HoldArrow> _holdPool = new();

    private readonly HoldArrow[] _currentHolds = new HoldArrow[4];

    private readonly List<NoteArrow>[] _queuedArrows = { new(), new(), new(), new() };

    private double _loopLen; //secs
    public double TrueBeatsPerLoop;

    private double _chartLength = 2500; //Play with this

    public void OnNotePressed(ArrowType type)
    {
        //No beat zero, also if there is a current hold, don't handle a re input
        if (TimeKeeper.LastBeat.CompareTo(Beat.Zero) == 0 || _currentHolds[(int)type] != null)
            return;
        ArrowFromInput?.Invoke(NextArrowFrom(type));
    }

    public void OnNoteReleased(ArrowType type)
    {
        if (_currentHolds[(int)type] == null)
            return;
        HandleRelease(type);
    }

    private void HandleRelease(ArrowType type)
    {
        HoldArrow hold = _currentHolds[(int)type];
        hold.NoteRelease();
        _currentHolds[(int)type] = null;
        NoteArrowData incrData = hold.Data;
        ArrowFromInput?.Invoke(incrData.BeatFromLength());
    }

    public override void _Ready()
    {
        _arrowGroup = ChartLoopables.GetNode<Node>("ArrowGroup");

        IH.Connect(nameof(InputHandler.NotePressed), new Callable(this, nameof(OnNotePressed)));
        IH.Connect(nameof(InputHandler.NoteReleased), new Callable(this, nameof(OnNoteReleased)));
    }

    private bool _initialized;

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
        bool isHold = noteArrowData.Length > 0;
        NoteArrow noteArrow;
        if (isHold)
            noteArrow = _holdPool.Count == 0 ? InstatiateNewArrow(true) : DePoolArrow(true);
        else
            noteArrow = _arrowPool.Count == 0 ? InstatiateNewArrow() : DePoolArrow();

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

    private NoteArrow InstatiateNewArrow(bool isHold = false)
    {
        string path = isHold ? HoldArrow.LoadPath : NoteArrow.LoadPath;
        NoteArrow result = ResourceLoader.Load<PackedScene>(path).Instantiate<NoteArrow>();
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
        if (noteArrow is HoldArrow)
            _currentHolds[(int)noteArrow.Type] = null;
        ArrowFromInput?.Invoke(noteArrow.Data);
    }

    private void PoolArrow(NoteArrow noteArrow)
    {
        int index = _queuedArrows[(int)noteArrow.Type].IndexOf(noteArrow);
        if (index != -1)
            _queuedArrows[(int)noteArrow.Type].RemoveAt(index);
        noteArrow.ProcessMode = ProcessModeEnum.Disabled;
        noteArrow.Visible = false;
        if (noteArrow is HoldArrow holdArrow)
            _holdPool.Add(holdArrow);
        else
            _arrowPool.Add(noteArrow);
    }

    private NoteArrow DePoolArrow(bool isHold = false)
    {
        NoteArrow res;
        if (isHold)
        {
            res = _holdPool[0];
            _holdPool.RemoveAt(0);
        }
        else
        {
            res = _arrowPool[0];
            _arrowPool.RemoveAt(0);
        }

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
            .OrderBy(arrow => Math.Abs((arrow.Beat - TimeKeeper.LastBeat).BeatPos)) //Sort by closest to cur beat
            .ToList();
        if (activeArrows.Count != 0) //There is an active note in hittable range activate it and pass it
        {
            activeArrows[0].NoteHit();
            if (activeArrows[0] is HoldArrow holdArrow) //Best active arrow is hold
                _currentHolds[(int)type] = holdArrow;
            return activeArrows[0].Data;
        }

        int index = _queuedArrows[(int)type]
            .FindIndex(arrow => arrow.IsInRange(TimeKeeper.LastBeat));
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
