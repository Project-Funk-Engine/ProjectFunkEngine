using System;
using System.Linq;
using FunkEngine;
using Godot;
using Godot.Collections;

public partial class NoteChart : Resource
{ //Godot is unhappy with this sometimes.
    const float Precision = 0.0001f;

    [Export]
    Array<NoteInfo> UpLaneData = new Array<NoteInfo>();

    [Export]
    Array<NoteInfo> DownLaneData = new Array<NoteInfo>();

    [Export]
    Array<NoteInfo> LeftLaneData = new Array<NoteInfo>();

    [Export]
    Array<NoteInfo> RightLaneData = new Array<NoteInfo>();

    public void Reset()
    {
        UpLaneData = new Array<NoteInfo>();
        DownLaneData = new Array<NoteInfo>();
        LeftLaneData = new Array<NoteInfo>();
        RightLaneData = new Array<NoteInfo>();
    }

    public void SaveChart(string path)
    {
        ResourceSaver.Save(this, path);
    }

    public Array<NoteInfo> GetLane(ArrowType arrowType)
    {
        return arrowType switch
        {
            ArrowType.Up => UpLaneData,
            ArrowType.Down => DownLaneData,
            ArrowType.Left => LeftLaneData,
            ArrowType.Right => RightLaneData,
            _ => throw new ArgumentOutOfRangeException(nameof(arrowType), arrowType, null),
        };
    }

    public void RemoveNote(ArrowType type, float beat)
    {
        if (beat == 0)
            return; //All my homies hate beat 0
        for (int i = 0; i < GetLane(type).Count; i++)
        {
            if (Math.Abs(GetLane(type)[i].Beat - beat) > Precision)
                continue;
            GetLane(type).RemoveAt(i);
            return;
        }
    }

    public void AddNote(ArrowType type, float beat, float len = 0)
    {
        if (beat == 0)
            return; //All my homies hate beat 0
        if (GetLane(type).Any(note => Math.Abs(note.Beat - beat) < Precision)) //Fuck it, traverse the whole array.
        {
            return;
        }
        GetLane(type).Add(new NoteInfo().Create(beat, len));
    }
}
