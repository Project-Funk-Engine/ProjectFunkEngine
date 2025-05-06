using System;
using System.Collections.Generic;
using System.Linq;
using FunkEngine.Classes.MidiMaestro;
using Godot;

namespace FunkEngine;

#region Structs
/**
 * <summary>SongData: Basic information defining the statistics of an in-battle song.</summary>
 */
public struct SongData
{
    public int Bpm;
    public double SongLength;
    public int NumLoops;
}

/**
 * <summary>ArrowData: Data representing the necessary information for each arrow checker.</summary>
 */
public struct CheckerData
{
    public Color Color;
    public string Key;
    public NoteChecker Node;
    public ArrowType Type;
}

/**
 * <summary>BattleConfig: Necessary data for a battle.</summary>
 */
public struct BattleConfig
{
    public Stages RoomType;
    public MapGrid.Room BattleRoom;
    public string[] EnemyScenePath;
    public SongTemplate CurSong;
}

/**
 * <summary>NoteArrowData: Data To be stored and transmitted to represent a NoteArrow.</summary>
 */
public struct ArrowData : IEquatable<ArrowData>, IComparable<ArrowData>
{
    public ArrowData(ArrowType type, Beat beat, Note note, double length = 0)
    {
        Beat = beat;
        Type = type;
        NoteRef = note;
        Length = length;
    }

    public Beat Beat;
    public readonly double Length; //in beats, should never be >= loop
    public readonly ArrowType Type;
    public Note NoteRef { get; private set; } = null;

    public static ArrowData Placeholder { get; private set; } =
        new(default, default, new Note(-1, ""));

    public ArrowData BeatFromLength()
    {
        Beat += Length;
        return this;
    }

    public static ArrowData SetNote(ArrowData arrowData, Note note)
    {
        arrowData.NoteRef = note;
        return arrowData;
    }

    public ArrowData IncDecLoop(int amount)
    {
        Beat.IncDecLoop(amount);
        return this;
    }

    public bool Equals(ArrowData other)
    {
        return Beat.Equals(other.Beat) && Type == other.Type;
    }

    public override bool Equals(object obj)
    {
        return obj is ArrowData other && Equals(other);
    }

    public static bool operator ==(ArrowData left, ArrowData right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ArrowData left, ArrowData right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Beat, (int)Type);
    }

    public int CompareTo(ArrowData data) //Only care about beat for comparison
    {
        if ((int)Beat.BeatPos == (int)data.Beat.BeatPos && Beat.Loop == data.Beat.Loop)
        {
            if (Type == data.Type)
            {
                return Beat.CompareTo(data.Beat);
            }
            return Type.CompareTo(data.Type);
        }

        return Beat.CompareTo(data.Beat);
    }
}

/**
 * <summary>Beat: Data representing a beat and its loop num.</summary>
 */
public struct Beat : IEquatable<Beat>, IComparable<Beat>
{
    public int Loop = 0;
    public double BeatPos = 0;
    public static readonly Beat One = new Beat(1);
    public static readonly Beat Zero = new Beat(0);

    public Beat(double beat)
    {
        Loop = (int)(beat / TimeKeeper.BeatsPerLoop);
        BeatPos = beat % TimeKeeper.BeatsPerLoop;
    }

    public Beat(double beat, int loop)
    {
        Loop = loop;
        BeatPos = beat % TimeKeeper.BeatsPerLoop;
    }

    public double GetBeatInSong()
    {
        return BeatPos + Loop * TimeKeeper.BeatsPerLoop % TimeKeeper.BeatsPerSong;
    }

    public Beat IncDecLoop(int amount)
    {
        Loop += amount;
        return this;
    }

    public Beat RoundBeat()
    {
        BeatPos = (int)Math.Round(BeatPos); //This can technically overflow, but causes no bugs yet.
        return this;
    }

    public override string ToString()
    {
        return $"Beat: {BeatPos}, Loop: {Loop}";
    }

    public static bool operator >(Beat beat1, Beat beat2)
    {
        return beat1.Loop > beat2.Loop
            || (beat1.Loop == beat2.Loop && beat1.BeatPos > beat2.BeatPos);
    }

    public static bool operator <(Beat beat1, Beat beat2)
    {
        return beat1.Loop < beat2.Loop
            || (beat1.Loop == beat2.Loop && beat1.BeatPos < beat2.BeatPos);
    }

    public static bool operator <=(Beat beat1, Beat beat2)
    {
        return beat1.Equals(beat2) || beat1 < beat2;
    }

    public static bool operator >=(Beat beat1, Beat beat2)
    {
        return beat1.Equals(beat2) || beat1 > beat2;
    }

    public static Beat operator +(Beat beat1, double beatInc)
    {
        return new Beat(beat1.BeatPos + beatInc).IncDecLoop(beat1.Loop);
    }

    public static Beat operator -(Beat beat1, double beatDec)
    {
        return new Beat(beat1.BeatPos - beatDec).IncDecLoop(beat1.Loop);
    }

    public static Beat operator -(Beat beat1, Beat beat2)
    {
        return new Beat(beat1.BeatPos - beat2.BeatPos).IncDecLoop(beat1.Loop - beat2.Loop);
    }

    public bool Equals(Beat other)
    {
        return Loop == other.Loop && BeatPos.Equals(other.BeatPos);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Loop, BeatPos);
    }

    public int CompareTo(Beat other)
    {
        var loopComparison = Loop.CompareTo(other.Loop);
        return loopComparison != 0 ? loopComparison : BeatPos.CompareTo(other.BeatPos);
    }
}
#endregion

#region Enums
public enum ArrowType
{
    Up = 0,
    Down = 1,
    Left = 2,
    Right = 3,
}

public enum Timing
{
    Miss = 0,
    Bad = 1,
    Okay = 2,
    Good = 3,
    Perfect = 4,
}

public enum Targetting
{
    Player,
    First,
    All,
}

public enum BattleEffectTrigger
{
    NotePlaced,
    NoteHit,
    SelfNoteHit,
    OnPickup,
    OnLoop,
    OnBattleStart,
    OnBattleEnd,
    OnDamageInstance,
    OnDamageTaken,
}

public enum Stages
{
    Battle = 0,
    Chest = 1,
    Boss,
    Quit,
    Map,
    Load,
    Continue,
    Title,
}

public enum Rarity
{
    Breakfast = 5,
    Common = 4,
    Uncommon = 3,
    Rare = 2,
    Epic = 1,
    Legendary = 0,
}
#endregion

/**
 * <summary>MapGrid: Map as data.
 * Essentially a width by height grid. Valid rooms are determined by choosing a random starting room at height: 0, and makes random walks to height: height.
 * Walks go from x,y to {x/x+1/x-1},y+1</summary>
 */
public class MapGrid
{
    private int[,] _map;
    private Room[] _rooms;
    private int _curIdx;

    public Room[] GetRooms()
    {
        return _rooms;
    }

    public class Room
    {
        public int Idx { get; private set; }
        public int[] Children { get; private set; } = [];
        public int X { get; private set; }
        public int Y { get; private set; }
        public Stages Type { get; private set; }

        public Room(int idx, int x, int y)
        {
            Idx = idx;
            X = x;
            Y = y;
        }

        public void SetType(Stages type)
        {
            Type = type;
        }

        public void AddChild(int newIdx)
        {
            if (Children.Contains(newIdx))
                return;
            Children = Children.Append(newIdx).ToArray();
        }
    }

    /**
    * <summary>Initializes the map with max <c>width</c>, max <c>height</c>, and with number of <c>paths</c>.</summary>
    */
    public void InitMapGrid(MapLevels.MapConfig curConfig)
    {
        _curIdx = 0;
        _rooms = [];
        _map = new int[curConfig.Width, curConfig.Height]; //x,y

        int startX = (curConfig.Width / 2);
        _rooms = _rooms.Append(new Room(_curIdx, startX, 0)).ToArray();
        _rooms[0].SetType(Stages.Battle);
        _map[startX, 0] = _curIdx++;

        for (int i = 0; i < curConfig.Paths; i++)
        {
            GeneratePath_r(startX, 0, curConfig);
        }
        CreateCommonChildren(curConfig.Width, curConfig.Height);
        AddBossRoom(curConfig.Width, curConfig.Height);
    }

    /**Start at x, y, assume prev room exists. Picks new x pos within +/- 1, attaches recursively*/
    private void GeneratePath_r(int x, int y, MapLevels.MapConfig curConfig)
    {
        int nextX = StageProducer.GlobalRng.RandiRange(
            Math.Max(x - 1, 0),
            Math.Min(x + 1, curConfig.Width - 1)
        );
        if (_map[nextX, y + 1] == 0)
        {
            _rooms = _rooms.Append(new Room(_curIdx, nextX, y + 1)).ToArray();
            _map[nextX, y + 1] = _curIdx;
            _rooms[_map[x, y]].AddChild(_curIdx++);
            _rooms[^1].SetType(PickRoomType(x, y, curConfig));
        }
        else
        {
            _rooms[_map[x, y]].AddChild(_map[nextX, y + 1]);
        }
        if (y < curConfig.Height - 2)
        {
            GeneratePath_r(nextX, y + 1, curConfig);
        }
    }

    private Stages PickRoomType(int x, int y, MapLevels.MapConfig curConfig)
    {
        //If the y has a set room return it.
        if (curConfig.SetRooms.TryGetValue(y, out Stages result))
        {
            return result;
        }

        //Random roll for the room type.
        float[] validRooms = curConfig.StageOdds;
        foreach ((Stages stage, int height) in curConfig.MinHeights)
        {
            if (y < height)
            {
                validRooms[(int)stage] = 0;
            }
        }
        int idx = (int)StageProducer.GlobalRng.RandWeighted(validRooms);
        return MapLevels.MapConfig.StagsToRoll[idx];
    }

    //Asserts that if there is a room at the same x, but y+1 they are connected
    private void CreateCommonChildren(int width, int height)
    {
        foreach (Room room in _rooms)
        {
            Vector2I curPos = new Vector2I(room.X, room.Y);
            if (room.Y + 1 >= height)
                continue;
            if (_map[curPos.X, curPos.Y + 1] == 0)
                continue;
            room.AddChild(_map[curPos.X, curPos.Y + 1]);
        }
    }

    //Adds a boss room at the end of rooms, all max height rooms connect to it.
    private void AddBossRoom(int width, int height)
    {
        _rooms = _rooms.Append(new Room(_curIdx, width / 2, height)).ToArray();
        _rooms[_curIdx].SetType(Stages.Boss);
        for (int i = 0; i < width; i++) //Attach all last rooms to a boss room
        {
            if (_map[i, height - 1] != 0)
            {
                _rooms[_map[i, height - 1]].AddChild(_curIdx);
            }
        }
    }
}

#region Interfaces

public class BattleEventArgs(BattleDirector director) : EventArgs
{
    public BattleDirector BD = director;
}

/**
 * <summary>A BattleDirector driven battle event. Needs an enum defined trigger.</summary>
 */
public interface IBattleEvent
{
    void OnTrigger(BattleEventArgs e);
    BattleEffectTrigger GetTrigger();
}

public interface IDisplayable
{
    string Name { get; set; }
    Texture2D Texture { get; set; }
}

public interface IFocusableMenu
{
    IFocusableMenu Prev { get; set; }
    void OpenMenu(IFocusableMenu parentMenu);
    void PauseFocus();
    void ResumeFocus();
    void ReturnToPrev();
}
#endregion
