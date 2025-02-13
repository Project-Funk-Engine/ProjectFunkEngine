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

    private Stages _curStage = Stages.Title;
    private Node _curScene;

    private MapGrid _map = new MapGrid();

    //Hold here to persist between changes
    //TODO: Allow for permanent changes and battle temporary stat changes.
    public static PlayerStats PlayerStats;

    public class MapGrid
    {
        private int[,] _map;
        private Room[] _rooms;
        private int _curIdx = 0;
        private int _curRoom = 0;

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

            private int Idx;
            private int[] Children = Array.Empty<int>();
            private int X;
            private int Y;
            private string Type;
        }

        public void InitMapGrid(int width, int height, int paths)
        {
            _curIdx = 0;
            _rooms = Array.Empty<Room>();
            _map = new int[width, height]; //x,y

            int startX = GlobalRng.RandiRange(0, width - 1); //TODO: Replace with seeding
            _rooms = _rooms.Append(new Room(_curIdx, startX, 0)).ToArray();
            _map[startX, 0] = _curIdx++;

            for (int i = 0; i < paths; i++)
            {
                GeneratePath_r(startX, 0, width, height);
            }

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

        private void AddBossRoom(int width, int height)
        {
            _rooms = _rooms.Append(new Room(_curIdx, 0, height)).ToArray();
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
        _map.InitMapGrid(2, 2, 1);
        _seed = GlobalRng.Seed;
        _lastRngState = GlobalRng.State;
        PlayerStats = new PlayerStats();
    }

    public void TransitionStage(Stages nextStage)
    {
        GD.Print(GetTree().CurrentScene);
        switch (nextStage)
        {
            case Stages.Title:
                GetTree().ChangeSceneToFile("res://scenes/SceneTransitions/TitleScreen.tscn");
                break;
            case Stages.Battle:
                StartGame();
                GetTree().ChangeSceneToFile("res://scenes/BattleDirector/test_battle_scene.tscn");
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
