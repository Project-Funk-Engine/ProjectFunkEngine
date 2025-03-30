using System;

namespace FunkEngine;

using Godot;

public partial class NoteArrow : Sprite2D
{ //TextRect caused issues later :)
    public static readonly string LoadPath = "res://Scenes/NoteManager/NoteArrow.tscn";
    private const float LeftBound = -200f;

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
        ZIndex = 1;
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

    public void Init(ArrowData parentArrowData, NoteArrowData noteArrowData, double beatTime)
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

    public void Recycle()
    {
        Visible = true;
        ProcessMode = ProcessModeEnum.Inherit;
        if (IsHit)
        {
            Modulate /= .7f;
        }
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

    private void CheckMissed()
    {
        if (IsHit || !(TimeKeeper.LastBeat - Beat > Beat.One))
            return;
        Missed?.Invoke(this);
    }

    public delegate void KillEventHandler(NoteArrow note);
    public event KillEventHandler QueueForPool;

    private void BeatChecks()
    {
        CheckMissed();
        CheckHittable();
    }

    public override void _Process(double delta)
    {
        BeatChecks(); //Process happens down the tree, beat checks should happen first, so as close to beat update
        Vector2 newPos = Position;
        newPos.X = GetNewPosX();
        if (!float.IsNaN(newPos.X))
            Position = newPos;
        if (Position.X < LeftBound)
        {
            if (!IsHit)
                Missed?.Invoke(this);
            QueueForPool?.Invoke(this);
        }
    }
}
