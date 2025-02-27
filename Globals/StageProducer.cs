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

    private Stages _curStage = Stages.Title; //TODO: State Machine kinda deal?
    private Node _curScene;
    public static MapGrid.Room CurRoom { get; private set; }
    public static Vector2I MapSize { get; private set; } = new Vector2I(7, 6); //For now, make width an odd number

    public static MapGrid Map { get; } = new MapGrid();

    public static BattleConfig Config;

    //Hold here to persist between changes
    //TODO: Allow for permanent changes and battle temporary stat changes.
    public static PlayerStats PlayerStats;

    public void StartGame()
    {
        Map.InitMapGrid(MapSize.X, MapSize.Y, 3);
        _seed = GlobalRng.Seed;
        _lastRngState = GlobalRng.State;
        PlayerStats = new PlayerStats();

        CurRoom = Map.GetRooms()[0];
        _isInitialized = true;
    }

    public static void ChangeCurRoom(MapGrid.Room room)
    {
        CurRoom = room;
    }

    public void TransitionFromRoom(int nextRoomIdx)
    {
        TransitionStage(Map.GetRooms()[nextRoomIdx].Type, nextRoomIdx);
    }

    public void TransitionStage(Stages nextStage, int nextRoomIdx = -1)
    {
        GD.Print(GetTree().CurrentScene);
        switch (nextStage)
        {
            case Stages.Title:
                _isInitialized = false;
                GetTree().ChangeSceneToFile("res://scenes/SceneTransitions/TitleScreen.tscn");
                break;
            case Stages.Battle:
                Config = MakeConfig(nextStage, nextRoomIdx);
                GetTree().ChangeSceneToFile("res://scenes/BattleDirector/test_battle_scene.tscn");
                break;
            case Stages.Controls:
                GetTree().ChangeSceneToFile("res://scenes/Remapping/Remap.tscn");
                break;
            case Stages.Chest:
                Config = MakeConfig(nextStage, nextRoomIdx);
                GetTree().ChangeSceneToFile("res://scenes/ChestScene/ChestScene.tscn");
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

    private BattleConfig MakeConfig(Stages nextRoom, int nextRoomIdx)
    {
        BattleConfig result = new BattleConfig();
        result.BattleRoom = Map.GetRooms()[nextRoomIdx];
        result.RoomType = nextRoom;
        if (nextRoom is Stages.Battle or Stages.Boss)
        {
            result.CurSong = Scribe.SongDictionary[1];
        }

        return result;
    }
}
