using System.Collections.Generic;
using System.Linq;
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
    public static List<int> BattlePool { get; private set; } = [];

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
        Savekeeper.Saving += SerializeRun;
        Savekeeper.Saving += SerializePersist;
        Savekeeper.LoadFromFile();
        DeserializePersist();

        InitFromCfg();
        LiveInstance = this;

        GD.Load<PackedScene>(BattleDirector.LoadPath);
        GD.Load<PackedScene>(Cartographer.LoadPath);
        GD.Load<PackedScene>(ShopScene.LoadPath);
        GD.Load<PackedScene>(EventScene.LoadPath);
        GD.Load<PackedScene>(ChestScene.LoadPath);
        GD.Load<PackedScene>(TitleScreen.EffectsLoadPath);
    }

    public void InitFromCfg()
    {
        OptionsMenu.ChangeVolume(
            Configkeeper.GetConfigValue(Configkeeper.ConfigSettings.Volume).As<float>()
        );
        TranslationServer.SetLocale(
            Configkeeper.GetConfigValue(Configkeeper.ConfigSettings.LanguageKey).As<string>()
        );
        ContrastFilter = GD.Load<PackedScene>("res://Globals/ContrastFilter/ContrastFilter.tscn")
            .Instantiate<CanvasLayer>();
        ContrastFilter.Visible = Configkeeper
            .GetConfigValue(Configkeeper.ConfigSettings.HighContrast)
            .AsBool();
        GetTree().Root.CallDeferred("add_child", ContrastFilter);
        InputHandler.UseArrows = Configkeeper
            .GetConfigValue(Configkeeper.ConfigSettings.TypeIsArrow)
            .AsBool();
        BattleDirector.VerticalScroll = Configkeeper
            .GetConfigValue(Configkeeper.ConfigSettings.VerticalScroll)
            .AsBool();
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
        if (GetPersistantVal(PersistKeys.TutorialDone) == 0)
            CurLevel = MapLevels.GetLevelFromId(0);
        else
            CurLevel = MapLevels.GetLevelFromId(1);
        GenerateMapConsistent();

        PlayerStats = new PlayerStats();

        CurRoom = Map.GetRooms()[0].Idx;
        BattlePool = [];
        EventScene.EventPool = [];
        Scribe.InitRelicPools();
        IsInitialized = true;
        MapGrid.ForceEliteBattles = false;
    }

    private bool LoadGame()
    {
        if (!DeserializeRun())
        {
            GD.PushWarning("Can't load game, either file 404 or invalid file.");
            return false;
        }

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
            case Stages.Event:
                _loadTask = Task.Run(() =>
                {
                    _preloadStage = GD.Load<PackedScene>(EventScene.LoadPath).Instantiate<Node>();
                });
                break;
            case Stages.Shop:
                _loadTask = Task.Run(() =>
                {
                    _preloadStage = GD.Load<PackedScene>(ShopScene.LoadPath).Instantiate<Node>();
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
        LastStage = _curStage;
        _curStage = nextStage;
    }

    public Stages LastStage; //Hacky, purely to have title screen return to custom menu.

    public void TransitionToCustom(SongTemplate song)
    {
        GetTree().Root.RemoveChild(ContrastFilter);
        GetTree().Root.AddChild(ContrastFilter);
        GlobalRng.Randomize();
        PlayerStats = new PlayerStats();
        Config = new BattleConfig
        {
            BattleRoom = null,
            RoomType = Stages.Custom,
            CurSong = song,
            EnemyScenePath = song.EnemyScenePath,
        };
        GetTree().ChangeSceneToFile(BattleDirector.LoadPath);
        _curStage = Stages.Custom;
    }

    #endregion

    private void RefreshBattlePool()
    {
        BattlePool = new List<int>(CurLevel.NormalBattles);
        for (int i = 0; i < BattlePool.Count - 2; i++)
        {
            int randIdx = GlobalRng.RandiRange(0, CurLevel.NormalBattles.Length - 1);
            (BattlePool[i], BattlePool[randIdx]) = (BattlePool[randIdx], BattlePool[i]); //rad
        }
    }

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
                if (BattlePool == null || BattlePool.Count == 0)
                    RefreshBattlePool();
                int songIdx = stageRng.RandiRange(0, BattlePool.Count - 1);
                result.CurSong = Scribe.SongDictionary[BattlePool[songIdx]];
                result.EnemyScenePath = Scribe.SongDictionary[BattlePool[songIdx]].EnemyScenePath;
                BattlePool.RemoveAt(songIdx);
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
        if (ControlSettings.IsOutOfFocus(this))
        {
            GetViewport().SetInputAsHandled();
            return;
        }
        if (@event is InputEventKey eventKey && eventKey.Pressed && !eventKey.Echo)
        {
            if (eventKey.Keycode == Key.F9)
            {
                Image screen = GetViewport().GetTexture().GetImage();
                if (!DirAccess.DirExistsAbsolute("user://Screenshots"))
                    DirAccess.MakeDirAbsolute("user://Screenshots");
                screen.SavePng(
                    "user://Screenshots/"
                        + Time.GetDatetimeStringFromSystem().Replace(":", "_")
                        + ".png"
                );
            }
            else if (eventKey.Keycode == Key.F11)
            {
                DisplayServer.WindowSetMode(
                    DisplayServer.WindowGetMode() == DisplayServer.WindowMode.ExclusiveFullscreen
                        ? DisplayServer.WindowMode.Windowed
                        : DisplayServer.WindowMode.ExclusiveFullscreen
                );
            }
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
        CurLevel = CurLevel.GetNextLevel();

        Map = new();
        GenerateMapConsistent();
        CurRoom = Map.GetRooms()[0].Idx;
        BattlePool = [];
    }

    #endregion

    #region Persistent Data
    private const string PersistenceHeader = "PersistVals";

    public enum PersistKeys
    {
        TutorialDone = 0,
        HasWon = 1,
    } //Relative order needs to be preserved between versions.

    private static int[] PersistantValues { get; set; } = [0, 0]; //Dumb and hacky right now. Literally doing this to avoid bool spam for now.
    private const string PersistentIntValName = "PersistInts";

    public static int GetPersistantVal(PersistKeys key)
    {
        return PersistantValues[(int)key];
    }

    public static void UpdatePersistantValues(PersistKeys key, int newVal)
    {
        PersistantValues[(int)key] = newVal;
        SerializePersist();
        Savekeeper.SaveToFile();
    }

    private static void SerializePersist()
    {
        string saveString = "";
        saveString += Savekeeper.FormatArray(PersistentIntValName, PersistantValues);
        Savekeeper.GameSaveObjects[PersistenceHeader] = saveString;
    }

    private void DeserializePersist()
    {
        if (!Savekeeper.GameSaveObjects.TryGetValue(PersistenceHeader, out var loadPers))
        {
            GD.PushWarning("Savekeeper does not contain persistence key!");
            return;
        }

        int idx = 0;
        var success = Savekeeper.ParseArray<int>(PersistentIntValName, loadPers, idx, int.TryParse);
        if (success.Success)
        {
            int[] tempVals = success.Value;
            for (int i = 0; i < tempVals.Length && i < PersistantValues.Length; i++) //Manually update to safeguard against saves breaking when values are added.
                PersistantValues[i] = tempVals[i];
            return;
        }
        GD.PushWarning(
            $"Error deserializing persistent values: {loadPers} Error: {success.Message}"
        );
    }
    #endregion

    #region Saving

    enum RunSaveValues
    { //Maintain in order of needing to be saved & loaded
        RngSeed,
        Area,
        BattlePool,
        EventPool,
        RngState,
        LastRoomIdx,
        NoteIds,
        RelicIds,
        PlayerHealth,
        Money,
        Shortcuts,
        PlayerMaxCombo,
    }

    private void SerializeRun()
    {
        if (!IsInitialized)
            return;
        string saveString = "";
        saveString +=
            Savekeeper.Format(RunSaveValues.RngSeed.ToString(), GlobalRng.Seed)
            + Savekeeper.Format(RunSaveValues.Area.ToString(), CurLevel.Id)
            + Savekeeper.FormatArray(RunSaveValues.BattlePool.ToString(), BattlePool.ToArray())
            + Savekeeper.FormatArray(
                RunSaveValues.EventPool.ToString(),
                EventScene.EventPool.ToArray()
            )
            + Savekeeper.Format(RunSaveValues.RngState.ToString(), GlobalRng.State)
            + Savekeeper.Format(RunSaveValues.LastRoomIdx.ToString(), CurRoom)
            + Savekeeper.FormatArray(
                RunSaveValues.NoteIds.ToString(),
                PlayerStats.CurNotes.Select(r => r.Id).ToArray()
            )
            + Savekeeper.FormatArray(
                RunSaveValues.RelicIds.ToString(),
                PlayerStats.CurRelics.Select(r => r.Id).ToArray()
            )
            + Savekeeper.Format(RunSaveValues.PlayerHealth.ToString(), PlayerStats.CurrentHealth)
            + Savekeeper.Format(RunSaveValues.Money.ToString(), PlayerStats.Money)
            + Savekeeper.Format(RunSaveValues.Shortcuts.ToString(), PlayerStats.Shortcuts)
            + Savekeeper.Format(RunSaveValues.PlayerMaxCombo.ToString(), PlayerStats.MaxComboBar);

        Savekeeper.GameSaveObjects[Savekeeper.DefaultRunSaveHeader] = saveString;
    }

    private bool DeserializeRun() //TODO: This is really verbose and bad.
    {
        if (!Savekeeper.GameSaveObjects.ContainsKey(Savekeeper.DefaultRunSaveHeader))
            return false;
        int idx = 0;
        string loadRun = Savekeeper.GameSaveObjects[Savekeeper.DefaultRunSaveHeader];

        var ulongSuccess = Savekeeper.Parse<ulong>(
            RunSaveValues.RngSeed.ToString(),
            loadRun,
            idx,
            ulong.TryParse
        );
        if (!ulongSuccess.Success)
            return false;
        GlobalRng.Seed = ulongSuccess.Value;
        idx = ulongSuccess.NextIdx;

        var intSuccess = Savekeeper.Parse<int>(
            RunSaveValues.Area.ToString(),
            loadRun,
            idx,
            int.TryParse
        );
        if (!intSuccess.Success)
            return false;
        CurLevel = MapLevels.GetLevelFromId(intSuccess.Value);
        idx = intSuccess.NextIdx;

        var bPoolSuccess = Savekeeper.ParseArray<int>(
            RunSaveValues.BattlePool.ToString(),
            loadRun,
            idx,
            int.TryParse
        );
        if (bPoolSuccess.Success)
        {
            BattlePool = bPoolSuccess.Value.ToList();
            idx = bPoolSuccess.NextIdx;
        }
        else
        {
            GD.PushWarning("Could not parse battle pool!");
            BattlePool = [];
        }

        var ePoolSuccess = Savekeeper.ParseArray<int>(
            RunSaveValues.EventPool.ToString(),
            loadRun,
            idx,
            int.TryParse
        );
        if (ePoolSuccess.Success)
        {
            EventScene.EventPool = ePoolSuccess.Value.ToList();
            idx = ePoolSuccess.NextIdx;
        }
        else
        {
            GD.PushWarning("Could not parse event pool!");
            EventScene.EventPool = [];
        }

        GenerateMapConsistent();

        ulongSuccess = Savekeeper.Parse<ulong>(
            RunSaveValues.RngState.ToString(),
            loadRun,
            idx,
            ulong.TryParse
        );
        if (!ulongSuccess.Success)
            return false;
        GlobalRng.State = ulongSuccess.Value;
        idx = ulongSuccess.NextIdx;

        intSuccess = Savekeeper.Parse<int>(
            RunSaveValues.LastRoomIdx.ToString(),
            loadRun,
            idx,
            int.TryParse
        );
        if (!intSuccess.Success)
            return false;
        CurRoom = intSuccess.Value;
        idx = intSuccess.NextIdx;

        Scribe.InitRelicPools();
        PlayerStats = new PlayerStats();
        PlayerStats.CurNotes = [];

        var noteSuccess = Savekeeper.ParseArray<int>(
            RunSaveValues.NoteIds.ToString(),
            loadRun,
            idx,
            int.TryParse
        );
        if (!noteSuccess.Success)
            return false;
        foreach (int noteId in noteSuccess.Value)
        {
            PlayerStats.AddNote(Scribe.NoteDictionary[noteId]);
        }
        idx = noteSuccess.NextIdx;

        var relicSuccess = Savekeeper.ParseArray<int>(
            RunSaveValues.RelicIds.ToString(),
            loadRun,
            idx,
            int.TryParse
        );
        if (!relicSuccess.Success)
            return false;
        foreach (int relicId in relicSuccess.Value)
        {
            PlayerStats.AddRelic(Scribe.RelicDictionary[relicId]);
        }
        idx = relicSuccess.NextIdx;

        intSuccess = Savekeeper.Parse<int>(
            RunSaveValues.PlayerHealth.ToString(),
            loadRun,
            idx,
            int.TryParse
        );
        if (!intSuccess.Success)
            return false;
        PlayerStats.CurrentHealth = intSuccess.Value;
        idx = intSuccess.NextIdx;

        intSuccess = Savekeeper.Parse<int>(
            RunSaveValues.Money.ToString(),
            loadRun,
            idx,
            int.TryParse
        );
        if (!intSuccess.Success)
            return false;
        PlayerStats.Money = intSuccess.Value;
        idx = intSuccess.NextIdx;

        intSuccess = Savekeeper.Parse<int>(
            RunSaveValues.Shortcuts.ToString(),
            loadRun,
            idx,
            int.TryParse
        );
        if (!intSuccess.Success)
            return false;
        PlayerStats.Shortcuts = intSuccess.Value;
        idx = intSuccess.NextIdx;

        intSuccess = Savekeeper.Parse<int>(
            RunSaveValues.PlayerMaxCombo.ToString(),
            loadRun,
            idx,
            int.TryParse
        );
        if (!intSuccess.Success)
            return false;
        PlayerStats.MaxComboBar = intSuccess.Value;

        return true;
    }

    #endregion
}
