using System;
using System.Linq;
using FunkEngine.Classes.MidiMaestro;
using Godot;

namespace FunkEngine;

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
public struct ArrowData
{
    public Color Color;
    public string Key;
    public NoteChecker Node;
    public ArrowType Type;
}

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

/**
 * <summary>BattleConfig: Necessary data for a battle.</summary>
 */
public struct BattleConfig
{
    public Stages RoomType;
    public MapGrid.Room BattleRoom;
    public string EnemyScenePath;
    public SongTemplate CurSong;
}

public enum BattleEffectTrigger
{
    NotePlaced,
    EnemyNoteHit,
    SelfNoteHit,
    OnPickup,
    OnLoop,
}

public enum Stages
{
    Title,
    Battle,
    Chest,
    Boss,
    Quit,
    Map,
    Load,
}

/**
 * <summary>MapGrid: Map as data.
 * Essentially a width by height grid. Valid rooms are determined by choosing a random starting room at height: 0, and makes random walks to height: height.
 * Walks go from x,y to {x/x+1/x-1},y+1</summary>
 */
public class MapGrid
{
    private int[,] _map;
    private Room[] _rooms;
    private int _curIdx = 0;

    public Room[] GetRooms()
    {
        return _rooms;
    }

    public class Room
    {
        public int Idx { get; private set; }
        public int[] Children { get; private set; } = Array.Empty<int>();
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
    public void InitMapGrid(int width, int height, int paths)
    {
        _curIdx = 0;
        _rooms = Array.Empty<Room>();
        _map = new int[width, height]; //x,y

        int startX = (width / 2);
        _rooms = _rooms.Append(new Room(_curIdx, startX, 0)).ToArray();
        _rooms[0].SetType(Stages.Battle);
        _map[startX, 0] = _curIdx++;

        for (int i = 0; i < paths; i++)
        {
            GeneratePath_r(startX, 0, width, height);
        }
        CreateCommonChildren(width, height);
        AddBossRoom(width, height);
    }

    /**Start at x, y, assume prev room exists. Picks new x pos within +/- 1, attaches recursively*/
    private void GeneratePath_r(int x, int y, int width, int height)
    {
        int nextX = StageProducer.GlobalRng.RandiRange(
            Math.Max(x - 1, 0),
            Math.Min(x + 1, width - 1)
        );
        if (_map[nextX, y + 1] == 0)
        {
            _rooms = _rooms.Append(new Room(_curIdx, nextX, y + 1)).ToArray();
            _map[nextX, y + 1] = _curIdx;
            _rooms[_map[x, y]].AddChild(_curIdx++);
            _rooms[^1].SetType(PickRoomType(x, y));
        }
        else
        {
            _rooms[_map[x, y]].AddChild(_map[nextX, y + 1]);
        }
        if (y < height - 2)
        {
            GeneratePath_r(nextX, y + 1, width, height);
        }
    }

    private Stages PickRoomType(int x, int y)
    {
        if (y % 3 == 0)
            return Stages.Chest;
        if (StageProducer.GlobalRng.Randf() < .08)
            return Stages.Chest;
        return Stages.Battle;
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

/**
 * <summary>A BattleDirector driven battle event. Needs an enum defined trigger.</summary>
 */
public interface IBattleEvent
{
    void OnTrigger(BattleDirector BD);
    BattleEffectTrigger GetTrigger();
}

public interface IDisplayable
{
    string Name { get; set; }
    string Tooltip { get; set; }
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
