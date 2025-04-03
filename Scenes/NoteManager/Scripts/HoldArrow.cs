using System;
using FunkEngine;
using Godot;

public partial class HoldArrow : NoteArrow
{
    public new const string LoadPath = "res://Scenes/NoteManager/HoldArrow.tscn";

    [Export]
    private NinePatchRect _trail;

    private double Length => Data.Length;
    public Beat EndBeat => Data.Beat + Data.Length;
    private bool _isReleased;

    public override void Init(CheckerData parentChecker, ArrowData arrowData, double beatTime)
    {
        base.Init(parentChecker, arrowData, beatTime);
        _trail.Size = new Vector2(
            (float)(Length / TimeKeeper.BeatsPerLoop * TimeKeeper.ChartWidth),
            _trail.Size.Y
        );
        _trail.Modulate = parentChecker.Color;
    }

    public override void Recycle()
    {
        base.Recycle();
        if (_isReleased)
            _trail.Modulate /= .7f;
        _isReleased = false;
    }

    public void NoteRelease()
    {
        if (_isReleased)
            return;
        _trail.Modulate *= .7f;
        _isReleased = true;
    }

    protected override void CheckMissed()
    {
        if (_isReleased)
            return; //Released => fully handled, let it kill itself
        if (!IsHit && (TimeKeeper.LastBeat - Beat > Beat.One)) //not hit, mostly normal miss handling
        {
            NoteRelease();
            RaiseMissed(this);
        }
        if (IsHit && (TimeKeeper.LastBeat - EndBeat > new Beat(Note.TimingMax))) //hit, only miss if held too long, handle timing on end beat
        {
            NoteRelease();
            RaiseMissed(this);
        }
    }

    protected override void PosChecks()
    {
        if (Position.X < LeftBound - _trail.Size.X || TimeKeeper.LastBeat - EndBeat > BeatBound) //i hate that we have to do this for now
        {
            if (!IsHit)
                RaiseMissed(this);
            NoteRelease();
            RaiseKill(this);
        }
    }

    public override bool IsInRange(Beat incomingBeat)
    {
        return (int)Math.Round(incomingBeat.BeatPos) >= (int)Math.Round(Beat.BeatPos)
            && (int)Math.Round(incomingBeat.BeatPos) <= (int)Math.Round(EndBeat.BeatPos);
    }
}
