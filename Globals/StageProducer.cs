using System;
using System.Threading.Tasks;
using FunkEngine;
using FunkEngine.Classes.MidiMaestro;
using Godot;

/**
 * <summary>StageProducer: Handles scene transitions and persistent gameplay data.</summary>
 */
public partial class StageProducer : Node
{
    public static StageProducer LiveInstance { get; private set; }
    public static bool IsInitialized;

    public static readonly RandomNumberGenerator GlobalRng = new();

    public static MapLevels CurLevel { get; private set; }

    public static MapGrid Map { get; private set; } = new();
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
        //Fudge seed state, to get consistent maps across new/loaded games, might be bad practice
        GlobalRng.State = GlobalRng.Seed << 5 / 2;
        Map.InitMapGrid(CurLevel.GetCurrentConfig());
    }

    private void StartNewGame()
    {
        GlobalRng.Randomize();
        if ((bool)SaveSystem.GetConfigValue(SaveSystem.ConfigSettings.FirstTime))
            CurLevel = MapLevels.GetLevelFromId(0);
        else
            CurLevel = MapLevels.GetLevelFromId(1);
        GenerateMapConsistent();

        PlayerStats = new PlayerStats();

        CurRoom = Map.GetRooms()[0].Idx;
        Scribe.InitRelicPools();
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
        CurLevel = MapLevels.GetLevelFromId(sv.Area);
        GenerateMapConsistent();
        GlobalRng.State = sv.RngState;
        CurRoom = sv.LastRoomIdx;

        Scribe.InitRelicPools();

        PlayerStats = new PlayerStats();
        PlayerStats.CurNotes = [];
        foreach (int noteId in sv.NoteIds)
        {
            PlayerStats.AddNote(Scribe.NoteDictionary[noteId]);
        }
        foreach (int relicId in sv.RelicIds)
        {
            PlayerStats.AddRelic(Scribe.RelicDictionary[relicId]);
        }
        PlayerStats.CurrentHealth = sv.PlayerHealth;
        PlayerStats.Money = sv.Money;
        PlayerStats.Shortcuts = sv.Shortcuts;
        PlayerStats.MaxComboBar = sv.PlayerMaxCombo;
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

    /// <summary>
    /// To be used from Cartographer. Preloads the scene during transition animation. This removes the occasionally noticeable load time for the scene change.
    /// </summary>
    /// <param name="nextRoomIdx">Index of the next room in the map to get the stage from.</param>
    public void PreloadScene(int nextRoomIdx)
    {
        Stages nextStage = Map.GetRooms()[nextRoomIdx].Type;
        Config = MakeBattleConfig(nextStage, nextRoomIdx);
        switch (nextStage)
        {
            case Stages.Elite:
            case Stages.Battle:
            case Stages.Boss:
                _loadTask = Task.Run(() =>
                {
                    _preloadStage = GD.Load<PackedScene>(BattleDirector.LoadPath)
                        .Instantiate<Node>();
                });
                break;
            case Stages.Shop:
            case Stages.Event:
                _loadTask = Task.Run(() =>
                {
                    _preloadStage = GD.Load<PackedScene>(EventScene.LoadPath).Instantiate<Node>();
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
            case Stages.Event:
            case Stages.Elite:
            case Stages.Shop:
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
            case Stages.Continue:
                ProgressLevels();
                GetTree().ChangeSceneToFile("res://Scenes/Maps/InBetween.tscn");
                break;
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
                int songIdx = stageRng.RandiRange(0, CurLevel.NormalBattles.Length - 1);
                result.CurSong = Scribe.SongDictionary[CurLevel.NormalBattles[songIdx]];
                result.EnemyScenePath = Scribe
                    .SongDictionary[CurLevel.NormalBattles[songIdx]]
                    .EnemyScenePath;
                break;
            case Stages.Elite:
                int elitIdx = stageRng.RandiRange(0, CurLevel.EliteBattles.Length - 1);
                result.CurSong = Scribe.SongDictionary[CurLevel.EliteBattles[elitIdx]];
                result.EnemyScenePath = Scribe
                    .SongDictionary[CurLevel.EliteBattles[elitIdx]]
                    .EnemyScenePath;
                break;
            case Stages.Boss:
                int bossIdx = stageRng.RandiRange(0, CurLevel.BossBattles.Length - 1);
                result.CurSong = Scribe.SongDictionary[CurLevel.BossBattles[bossIdx]];
                result.EnemyScenePath = Scribe
                    .SongDictionary[CurLevel.BossBattles[bossIdx]]
                    .EnemyScenePath;
                break;
            case Stages.Event:
            case Stages.Shop:
            case Stages.Chest:
                break;
            default:
                GD.PushError($"Error making Battle Config for invalid room type: {nextRoom}");
                break;
        }
        CurRoom = nextRoomIdx;
        return result;
    }

    //Putting this here in an autoload.
    public override void _Input(InputEvent @event)
    {
        //Consume controller input, if window out of focus.
        //This handles ui_input, other scenes need to consume their own.
        if (!GetWindow().HasFocus())
        {
            GetViewport().SetInputAsHandled();
            return;
        }
    }

    #region Area Management

    /// <summary>
    /// There should always be a mapconfig for each area. It's preferable to crash later if there isn't even a placeholder config.
    /// </summary>
    /// <returns>True if there is another area.</returns>
    public static bool IsMoreLevels()
    {
        return CurLevel.HasMoreMaps();
    }

    public void ProgressLevels()
    {
        GD.Print(CurLevel.Id);
        CurLevel = CurLevel.GetNextLevel();

        Map = new();
        GenerateMapConsistent();
        CurRoom = Map.GetRooms()[0].Idx;
    }

    #endregion
}
