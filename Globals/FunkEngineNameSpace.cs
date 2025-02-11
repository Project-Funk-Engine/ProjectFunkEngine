using Godot;

namespace FunkEngine;

public enum ArrowType
{
    Up = 0,
    Down = 1,
    Left = 2,
    Right = 3,
}

public enum BattleEffectTrigger
{
    NotePlaced,
    NoteHit,
    SelfNoteHit,
}

public enum Timing
{
    Miss = 0,
    Bad = 1,
    Okay = 2,
    Good = 3,
    Perfect = 4,
}

public enum Stages
{
    Title,
    Battle,
    Quit,
    Map,
}

public struct SongData
{
    public int Bpm;
    public double SongLength;
    public int NumLoops;
}

public struct ArrowData
{
    public Color Color;
    public string Key;
    public NoteChecker Node;
    public ArrowType Type;
}

public interface IBattleEvent
{
    void OnTrigger(BattleDirector BD);
    BattleEffectTrigger GetTrigger();
}
