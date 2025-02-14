using System;
using System.Linq;
using FunkEngine;
using Godot;

public partial class StageProducer : Node
{
    //Generate a map, starting as a width x height grid, pick a starting spot and do (path) paths from that to the last
    //row, connecting the path, then connect all at the end to the boss room.
    public static RandomNumberGenerator GlobalRng = new RandomNumberGenerator();
    private ulong _seed;
    private ulong _lastRngState;
    private bool _isInitialized = false;

    private Stages _curStage = Stages.Title;
    private Node _curScene;
    public static MapGrid.Room CurRoom { get; private set; }
    public static Vector2I MapSize { get; private set; } = new Vector2I(3, 2); //For now, make width an odd number

    public static MapGrid Map { get; } = new MapGrid();

    //Hold here to persist between changes
    //TODO: Allow for permanent changes and battle temporary stat changes.
    public static PlayerStats PlayerStats;

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

            public void SetType(string type)
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
            private string Type;
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
            int nextX = GlobalRng.RandiRange(Math.Max(x - 1, 0), Math.Min(x + 1, width - 1));
            if (_map[nextX, y + 1] == 0)
            {
                _rooms = _rooms.Append(new Room(_curIdx, nextX, y + 1)).ToArray();
                _map[nextX, y + 1] = _curIdx;
                _rooms[_map[x, y]].AddChild(_curIdx++);
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
            _rooms[_curIdx].SetType("Boss");
            for (int i = 0; i < width; i++) //Attach all last rooms to a boss room
            {
                if (_map[i, height - 1] != 0)
                {
                    _rooms[_map[i, height - 1]].AddChild(_curIdx);
                }
            }
        }
    }

    public void StartGame()
    {
        Map.InitMapGrid(MapSize.X, MapSize.Y, 1);
        _seed = GlobalRng.Seed;
        _lastRngState = GlobalRng.State;
        PlayerStats = new PlayerStats();

        CurRoom = Map.GetRooms()[0];
        _isInitialized = true;
    }

    public void TransitionFromRoom(int nextRoomIdx)
    {
        //CurRoom = Map.GetRooms()[nextRoomIdx];
        TransitionStage(Stages.Battle);
    }

    public void TransitionStage(Stages nextStage)
    {
        GD.Print(GetTree().CurrentScene);
        switch (nextStage)
        {
            case Stages.Title:
                _isInitialized = false;
                GetTree().ChangeSceneToFile("res://scenes/SceneTransitions/TitleScreen.tscn");
                break;
            case Stages.Battle:
                GetTree().ChangeSceneToFile("res://scenes/BattleDirector/test_battle_scene.tscn");
                break;
            case Stages.Map:
                GetTree().ChangeSceneToFile("res://scenes/Maps/cartographer.tscn");
                if (!_isInitialized)
                {
                    StartGame();
                }
                break;
            case Stages.Quit:
                GD.Print("Exiting game");
                GetTree().Quit();
                return;
            default:
                GD.Print($"Error Scene Transition is {nextStage}");
                break;
        }

        _curStage = nextStage;
    }
}
