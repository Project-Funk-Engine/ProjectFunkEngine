using System;
using System.Linq;
using FunkEngine;
using Godot;
using Godot.Collections;

public partial class NoteChart : Resource
{ //Godot is unhappy with this sometimes.
    const float Precision = 0.0001f;

    [Export]
    public int Bpm { get; set; }

    [Export]
    public int NumLoops { get; set; }

    //178 is average for most fights
    //<= 0 means go with default chart speed based on song length
    [Export]
    public float SongSpeed { get; set; } = -1;

    [Export]
    public string SongMapLocation { get; set; } = "";

    [Export]
    Array<NoteInfo> UpLaneData = [];

    [Export]
    Array<NoteInfo> DownLaneData = [];

    [Export]
    Array<NoteInfo> LeftLaneData = [];

    [Export]
    Array<NoteInfo> RightLaneData = [];

    public void Reset()
    {
        UpLaneData = [];
        DownLaneData = [];
        LeftLaneData = [];
        RightLaneData = [];
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
