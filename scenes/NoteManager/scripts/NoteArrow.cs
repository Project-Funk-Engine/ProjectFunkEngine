using System;

namespace FunkEngine;

using Godot;

public partial class NoteArrow : Sprite2D
{ //TextRect caused issues later :)
    public static readonly string LoadPath = "res://Scenes/NoteManager/NoteArrow.tscn";
    protected const float LeftBound = -200f;

    [Export]
    public Sprite2D OutlineSprite;

    [Export]
    public Sprite2D IconSprite;

    public NoteArrowData Data;
    public Beat Beat => Data.Beat;
    public ArrowType Type => Data.Type;

    private double _beatTime;
    public bool IsHit = false;
    public bool IsQueued = false;

    public override void _Ready()
    {
        ZIndex = 2;
    }

    public virtual bool IsInRange(Beat incomingBeat)
    {
        return (int)Math.Round(Beat.BeatPos) == (int)Math.Round(incomingBeat.BeatPos);
    }

    private float GetNewPosX()
    {
        double interval = TimeKeeper.ChartLength;
        double relativePosition =
            (TimeKeeper.CurrentTime - _beatTime) / TimeKeeper.LoopLength * TimeKeeper.ChartLength;

        return (float)(
            TimeKeeper.PosMod(-relativePosition - interval / 2, interval) - interval / 2
        );
    }

    public virtual void Init(
        ArrowData parentArrowData,
        NoteArrowData noteArrowData,
        double beatTime
    )
    {
        Data = noteArrowData;
        _beatTime = beatTime;

        Position = new Vector2(GetNewPosX(), parentArrowData.Node.GlobalPosition.Y);
        RotationDegrees = parentArrowData.Node.RotationDegrees;
        IconSprite.Texture = noteArrowData.NoteRef.Texture;
        IconSprite.Rotation = -Rotation;
        OutlineSprite.Modulate = parentArrowData.Color;
    }

    public void NoteHit()
    {
        if (IsHit)
            return;
        Modulate *= .7f;
        IsHit = true;
    }

    public virtual void Recycle()
    {
        Visible = true;
        ProcessMode = ProcessModeEnum.Inherit;
        if (IsHit)
            Modulate /= .7f;
        IsHit = false;
        IsQueued = false;
    }

    public delegate void HittableEventHandler(NoteArrow note);
    public event HittableEventHandler QueueForHit;

    private void CheckHittable()
    {
        if (IsQueued || !(Beat - TimeKeeper.LastBeat <= Beat.One))
            return;
        IsQueued = true;
        QueueForHit?.Invoke(this);
    }

    public delegate void MissedEventHandler(NoteArrow note);
    public event MissedEventHandler Missed;

    protected void RaiseMissed(NoteArrow note)
    {
        Missed?.Invoke(note);
    }

    protected virtual void CheckMissed()
    {
        if (IsHit || !(TimeKeeper.LastBeat - Beat > Beat.One))
            return;
        RaiseMissed(this);
    }

    public delegate void KillEventHandler(NoteArrow note);
    public event KillEventHandler QueueForPool;

    protected void RaiseKill(NoteArrow note)
    {
        QueueForPool?.Invoke(note);
    }

    private void BeatChecks()
    {
        CheckMissed();
        CheckHittable();
    }

    protected virtual void PosChecks()
    {
        if (Position.X < LeftBound)
        {
            if (!IsHit)
                RaiseMissed(this);
            RaiseKill(this);
        }
    }

    public override void _Process(double delta)
    {
        BeatChecks(); //beat checks first
        Vector2 newPos = Position;
        newPos.X = GetNewPosX();
        if (!float.IsNaN(newPos.X))
            Position = newPos;
        PosChecks();
    }
}
