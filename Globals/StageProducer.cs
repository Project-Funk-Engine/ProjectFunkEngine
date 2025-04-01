using System;
using System.Threading.Tasks;
using FunkEngine;
using Godot;

/**
 * <summary>StageProducer: Handles scene transitions and persistent gameplay data.</summary>
 */
public partial class StageProducer : Node
{
    public static StageProducer LiveInstance { get; private set; }
    public static bool IsInitialized;

    public static readonly RandomNumberGenerator GlobalRng = new();

    public static Vector2I MapSize { get; } = new(7, 6); //For now, make width an odd number
    public static MapGrid Map { get; } = new();
    private Stages _curStage = Stages.Title;
    public static int CurRoom { get; private set; }

    private Node _curScene;
    private Node _preloadStage;

    public static BattleConfig Config { get; private set; }

    public static PlayerStats PlayerStats { get; private set; } //Hold here to persist between changes

    public static CanvasLayer ContrastFilter { get; private set; }

    #region Initialization
    public override void _EnterTree()
    {
        InitFromCfg();
        LiveInstance = this;
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

    private void GenerateMapConsistent()
    {
        GlobalRng.State = GlobalRng.Seed << 5 / 2; //Fudge seed state, to get consistent maps across new/loaded games
        Map.InitMapGrid(MapSize.X, MapSize.Y, 3);
    }

    private void StartNewGame()
    {
        GlobalRng.Randomize();
        GenerateMapConsistent();

        PlayerStats = new PlayerStats();

        CurRoom = Map.GetRooms()[0].Idx;
        IsInitialized = true;
    }

    private bool LoadGame()
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
    #endregion

    public static MapGrid.Room GetCurRoom()
    {
        return Map.GetRooms()[CurRoom];
    }

    #region Transitions
    public void TransitionFromRoom(int nextRoomIdx)
    {
        TransitionStage(Map.GetRooms()[nextRoomIdx].Type, nextRoomIdx);
    }

    private Task _loadTask;

    /**
     * <summary>To be used from Cartographer. Preloads the scene during transition animation.
     * This removes the occasionally noticeable load time for the scene change.</summary>
     */
    public void PreloadScene(int nextRoomIdx)
    {
        Stages nextStage = Map.GetRooms()[nextRoomIdx].Type;
        Config = MakeBattleConfig(nextStage, nextRoomIdx);
        switch (nextStage)
        {
            case Stages.Battle:
            case Stages.Boss:
                _loadTask = Task.Run(() =>
                {
                    _preloadStage = GD.Load<PackedScene>(BattleDirector.LoadPath)
                        .Instantiate<Node>();
                });
                break;
            case Stages.Chest:
                _loadTask = Task.Run(() =>
                {
                    _preloadStage = GD.Load<PackedScene>(ChestScene.LoadPath).Instantiate<Node>();
                });
                break;
            default:
                GD.PushError($"Error Scene Transition is {nextStage}");
                break;
        }
    }

    public void TransitionStage(Stages nextStage, int nextRoomIdx = -1)
    {
        GetTree().Root.RemoveChild(ContrastFilter);
        switch (nextStage)
        {
            case Stages.Title:
                IsInitialized = false;
                GetTree().ChangeSceneToFile(TitleScreen.LoadPath);
                break;
            case Stages.Battle: //Currently these are only ever entered from map. Be aware if we change
            case Stages.Boss: //this, scenes either need to be preloaded first, or a different setup is needed.
            case Stages.Chest:
                _loadTask.Wait(); //Should always finish by the time it gets here, this guarantees it.
                GetTree().GetCurrentScene().Free();
                GetTree().Root.AddChild(_preloadStage);
                GetTree().SetCurrentScene(_preloadStage);
                break;
            case Stages.Map:
                GetTree().ChangeSceneToFile(Cartographer.LoadPath);
                if (!IsInitialized)
                {
                    StartNewGame();
                }
                break;
            case Stages.Load:
                if (!LoadGame())
                    StartNewGame();
                GetTree().ChangeSceneToFile(Cartographer.LoadPath);
                break;
            case Stages.Quit:
                GetTree().Quit();
                return;
            default:
                GD.PushError($"Error Scene Transition is {nextStage}");
                break;
        }

        _preloadStage = null;
        //Apply grayscale shader to all scenes
        GetTree().Root.AddChild(ContrastFilter);
        _curStage = nextStage;
    }
    #endregion

    private BattleConfig MakeBattleConfig(Stages nextRoom, int nextRoomIdx)
    {
        BattleConfig result = new BattleConfig
        {
            BattleRoom = Map.GetRooms()[nextRoomIdx],
            RoomType = nextRoom,
        };
        RandomNumberGenerator stageRng = new RandomNumberGenerator();
        stageRng.SetSeed(GlobalRng.Seed + (ulong)nextRoomIdx);
        switch (nextRoom)
        {
            case Stages.Battle:
                int songIdx = stageRng.RandiRange(1, 2);
                result.CurSong = Scribe.SongDictionary[songIdx];
                result.EnemyScenePath = Scribe.SongDictionary[songIdx].EnemyScenePath;
                break;
            case Stages.Boss:
                result.EnemyScenePath = P_BossBlood.LoadPath;
                result.CurSong = Scribe.SongDictionary[0];
                break;
            case Stages.Chest:
                break;
            default:
                GD.PushError($"Error making Battle Config for invalid room type: {nextRoom}");
                break;
        }
        CurRoom = nextRoomIdx;
        return result;
    }
}
