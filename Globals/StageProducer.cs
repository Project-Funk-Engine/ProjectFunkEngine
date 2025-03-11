using System;
using System.Linq;
using FunkEngine;
using Godot;

public partial class StageProducer : Node
{
    //Generate a map, starting as a width x height grid, pick a starting spot and do (path) paths from that to the last
    //row, connecting the path, then connect all at the end to the boss room.
    public static RandomNumberGenerator GlobalRng = new RandomNumberGenerator();
    public static bool IsInitialized;

    private Stages _curStage = Stages.Title;
    private Node _curScene;
    public static int CurRoom { get; private set; }

    public static Vector2I MapSize { get; private set; } = new Vector2I(7, 6); //For now, make width an odd number
    public static MapGrid Map { get; } = new MapGrid();

    public static BattleConfig Config;

    //Hold here to persist between changes
    public static PlayerStats PlayerStats;

    public static CanvasLayer ContrastFilter;

    public override void _EnterTree()
    {
        InitFromCfg();
    }

    private void InitFromCfg()
    {
        OptionsMenu.ChangeVolume(
            SaveSystem.GetConfigValue(SaveSystem.ConfigSettings.Volume).As<float>()
        );
        TranslationServer.SetLocale(
            SaveSystem.GetConfigValue(SaveSystem.ConfigSettings.LanguageKey).As<string>()
        );
        ContrastFilter = GD.Load<PackedScene>("res://Globals/ContrastFilter/ContrastFilter.tscn")
            .Instantiate<CanvasLayer>();
        ContrastFilter.Visible = SaveSystem
            .GetConfigValue(SaveSystem.ConfigSettings.HighContrast)
            .AsBool();
        GetTree().Root.CallDeferred("add_child", ContrastFilter);
    }

    public void StartGame()
    {
        GlobalRng.Randomize();
        GenerateMapConsistent();
        PlayerStats = new PlayerStats();

        CurRoom = Map.GetRooms()[0].Idx;
        IsInitialized = true;
    }

    public bool LoadGame()
    {
        SaveSystem.SaveFile sv = SaveSystem.LoadGame();
        if (sv == null)
        {
            GD.PushWarning("Can't load game, either file 404 or invalid file.");
            return false;
        }
        GlobalRng.Seed = sv.RngSeed;
        GenerateMapConsistent();
        GlobalRng.State = sv.RngState;
        CurRoom = sv.LastRoomIdx;

        PlayerStats = new PlayerStats();
        PlayerStats.CurNotes = Array.Empty<Note>();
        foreach (int noteId in sv.NoteIds)
        {
            PlayerStats.AddNote(Scribe.NoteDictionary[noteId]);
        }
        foreach (int relicId in sv.RelicIds)
        {
            PlayerStats.AddRelic(Scribe.RelicDictionary[relicId]);
        }
        PlayerStats.CurrentHealth = sv.PlayerHealth;
        IsInitialized = true;
        return true;
    }

    private void GenerateMapConsistent()
    {
        GlobalRng.State = GlobalRng.Seed << 5 / 2;
        Map.InitMapGrid(MapSize.X, MapSize.Y, 3);
    }

    public static MapGrid.Room GetCurRoom()
    {
        return Map.GetRooms()[CurRoom];
    }

    public static void ChangeCurRoom(int room)
    {
        CurRoom = room;
    }

    public void TransitionFromRoom(int nextRoomIdx)
    {
        TransitionStage(Map.GetRooms()[nextRoomIdx].Type, nextRoomIdx);
    }

    public void TransitionStage(Stages nextStage, int nextRoomIdx = -1)
    {
        GetTree().Root.RemoveChild(ContrastFilter);
        switch (nextStage)
        {
            case Stages.Title:
                IsInitialized = false;
                GetTree().ChangeSceneToFile("res://scenes/SceneTransitions/TitleScreen.tscn");
                break;
            case Stages.Battle:
                Config = MakeConfig(nextStage, nextRoomIdx);
                GetTree().ChangeSceneToFile("res://scenes/BattleDirector/test_battle_scene.tscn");
                break;
            case Stages.Boss:
                Config = MakeConfig(nextStage, nextRoomIdx);
                GetTree().ChangeSceneToFile("res://scenes/BattleDirector/test_battle_scene.tscn");
                break;
            case Stages.Chest:
                Config = MakeConfig(nextStage, nextRoomIdx);
                GetTree().ChangeSceneToFile("res://scenes/ChestScene/ChestScene.tscn");
                break;
            case Stages.Map:
                GetTree().ChangeSceneToFile("res://scenes/Maps/cartographer.tscn");
                if (!IsInitialized)
                {
                    StartGame();
                }
                break;
            case Stages.Load:
                if (!LoadGame())
                    StartGame();
                GetTree().ChangeSceneToFile("res://scenes/Maps/cartographer.tscn");
                break;
            case Stages.Quit:
                GetTree().Quit();
                return;
            default:
                GD.PushError($"Error Scene Transition is {nextStage}");
                break;
        }

        //Apply grayscale shader to all scenes
        GetTree().Root.AddChild(ContrastFilter);
        _curStage = nextStage;
    }

    private BattleConfig MakeConfig(Stages nextRoom, int nextRoomIdx)
    {
        BattleConfig result = new BattleConfig();
        result.BattleRoom = Map.GetRooms()[nextRoomIdx];
        result.RoomType = nextRoom;
        switch (nextRoom)
        {
            case Stages.Battle:
                int songIdx = GlobalRng.RandiRange(1, 2);
                result.CurSong = Scribe.SongDictionary[songIdx];
                result.EnemyScenePath = Scribe.SongDictionary[songIdx].EnemyScenePath;
                break;
            case Stages.Boss:
                result.EnemyScenePath = "res://scenes/Puppets/Enemies/BossBlood/Boss1.tscn";
                result.CurSong = Scribe.SongDictionary[0];
                break;
        }

        return result;
    }
}
