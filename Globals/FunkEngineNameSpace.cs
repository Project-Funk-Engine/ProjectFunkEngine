using System;
using System.Linq;
using Godot;

namespace FunkEngine;

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

public struct BattleConfig
{
    public MapRooms RoomType { get; private set; }
    public MapGrid.Room CurRoom { get; private set; }
    public SongData CurSong { get; set; }
    public int TodoEnemyAndChart;
}

public enum BattleEffectTrigger
{
    NotePlaced,
    NoteHit,
    SelfNoteHit,
    OnPickup,
}

public enum Stages
{
    Title,
    Battle,
    Quit,
    Map,
}

public enum MapRooms
{
    Battle,
    Chest,
    Boss,
}

public class MapGrid
{
    private int[,] _map;
    private Room[] _rooms;
    private int _curIdx = 0;
    private int _curRoom = 0;

    public Room[] GetRooms()
    {
        return _rooms;
    }

    public class Room
    {
        public Room(int idx, int x, int y)
        {
            Idx = idx;
            X = x;
            Y = y;
        }

        public void SetType(MapRooms type)
        {
            Type = type;
        }

        public void AddChild(int newIdx)
        {
            if (Children.Contains(newIdx))
                return;
            Children = Children.Append(newIdx).ToArray();
        }

        public int Idx { get; private set; }
        public int[] Children { get; private set; } = Array.Empty<int>();
        public int X { get; private set; }
        public int Y { get; private set; }
        public MapRooms Type { get; private set; }
    }

    public void InitMapGrid(int width, int height, int paths)
    {
        _curIdx = 0;
        _rooms = Array.Empty<Room>();
        _map = new int[width, height]; //x,y

        int startX = (width / 2);
        _rooms = _rooms.Append(new Room(_curIdx, startX, 0)).ToArray();
        _map[startX, 0] = _curIdx++;

        for (int i = 0; i < paths; i++)
        {
            GeneratePath_r(startX, 0, width, height);
        }
        CreateCommonChildren(width, height);
        AddBossRoom(width, height);
    }

    //Start at x, y, assume prev room exists. Picks new x pos within +/- 1, attaches recursively
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
            _rooms[^1].SetType(MapRooms.Battle);
            if (y > 0 && y % 3 == 0)
            {
                _rooms[^1].SetType(MapRooms.Chest);
            }
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
            GD.Print("Added child on same X.");
            room.AddChild(_map[curPos.X, curPos.Y + 1]);
        }
    }

    //Adds a boss room at the end of rooms, all max height rooms connect to it.
    private void AddBossRoom(int width, int height)
    {
        _rooms = _rooms.Append(new Room(_curIdx, width / 2, height)).ToArray();
        _rooms[_curIdx].SetType(MapRooms.Boss);
        for (int i = 0; i < width; i++) //Attach all last rooms to a boss room
        {
            if (_map[i, height - 1] != 0)
            {
                _rooms[_map[i, height - 1]].AddChild(_curIdx);
            }
        }
    }
}

public interface IBattleEvent
{
    void OnTrigger(BattleDirector BD);
    BattleEffectTrigger GetTrigger();
}
