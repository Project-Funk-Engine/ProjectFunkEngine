using System;
using System.Collections.Generic;
using FunkEngine;
using Godot;

public class MapLevels
{
    public struct MapConfig
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Paths { get; private set; }

        /// <summary>
        /// Rooms that exist at set levels, only one room can be set per y-level.
        /// </summary>
        public Dictionary<int, Stages> SetRooms { get; private set; } =
            new()
            {
                { 0, Stages.Battle }, //The first, e.g. y = 0 room, should always be a battle.
            };

        /// <summary>
        /// Adds a set room type to be generated guaranteed. Additional entries in the same y-level are ignored.
        /// This ignores minimum y-heights.
        /// </summary>
        /// <param name="height">The y-level of the rooms</param>
        /// <param name="roomType">The room type to be set.</param>
        public MapConfig AddSetRoom(int height, Stages roomType)
        {
            SetRooms.TryAdd(height, roomType);
            return this;
        }

        public static readonly Stages[] StagesToRoll = new[]
        {
            Stages.Battle,
            Stages.Chest,
            Stages.Elite,
            Stages.Event,
            Stages.Shop,
        };

        /// <summary>
        /// The odds for each stage to appear in a non-set room position.
        /// </summary>
        public float[] StageOdds = new float[5];

        /// <summary>
        /// The config to generate a map from.
        /// </summary>
        /// <param name="width">The maximum width of a map. Generally a max of ~12 is reasonable.</param>
        /// <param name="height">The maximum height of a map.</param>
        /// <param name="paths">The number of paths to draw through the mpa, this effects how many onnections there are.</param>
        /// <param name="odds">The odds for each room type.</param>
        public MapConfig(int width, int height, int paths, float[] odds)
        {
            Width = width;
            Height = height;
            Paths = paths;
            for (int i = 0; i < StagesToRoll.Length; i++)
            {
                StageOdds[i] = odds[i];
            }
        }

        /// <summary>
        /// Rooms that exist at set levels, only one room can be set per y-level.
        /// </summary>
        public Dictionary<Stages, int> MinHeights { get; private set; } = new();

        /// <summary>
        /// Adds a minimum y-height for a room type. Can only have one set per room type
        /// </summary>
        /// <param name="height">The y-level of the room type</param>
        /// <param name="roomType">The room type to be set.</param>
        public MapConfig AddMinHeight(Stages roomType, int height)
        {
            MinHeights.TryAdd(roomType, height);
            return this;
        }
    }

    private MapLevels(
        int id,
        MapConfig config,
        int[] battleSongs,
        int[] eliteSongs,
        int[] bossSongs,
        int[] eventIds,
        int nextLevelId = -1,
        string backgroundPath = "res://SharedAssets/BackGround_Full.png"
    )
    {
        Id = id;
        CurMapConfig = config;
        NormalBattles = battleSongs;
        EliteBattles = eliteSongs;
        BossBattles = bossSongs;
        EventIds = eventIds;
        NextLevel = nextLevelId;
        BackgroundPath = backgroundPath;
    }

    public int Id { get; private set; }
    public string BackgroundPath { get; private set; }
    private MapConfig CurMapConfig;
    private int NextLevel;

    //These tie into the Scribe SongDictionary
    public int[] NormalBattles { get; private set; }
    public int[] EliteBattles { get; private set; }
    public int[] BossBattles { get; private set; }
    public int[] EventIds { get; private set; }

    #region Preset Levels
    //Odds: Battle, Chest, Elite, Event, Shop
    private static readonly MapConfig FirstMapConfig = new MapConfig(7, 9, 3, [11, 2, 5, 4, 4])
        .AddSetRoom(3, Stages.Chest)
        .AddMinHeight(Stages.Chest, 2)
        .AddMinHeight(Stages.Shop, 3)
        .AddMinHeight(Stages.Elite, 3)
        .AddMinHeight(Stages.Event, 2);

    private static readonly MapConfig TutorialMapConfig = new MapConfig(1, 2, 1, [10, 0, 0, 0, 0]);

    private static readonly MapLevels[] PresetLevels = new[]
    {
        new MapLevels(0, TutorialMapConfig, [4], [0], [5], [], 1),
        new MapLevels(1, FirstMapConfig, [1, 2, 11, 12, 15, 17], [3, 6, 13], [0], [], -1),
    };
    #endregion

    public MapConfig GetCurrentConfig()
    {
        return CurMapConfig;
    }

    public bool HasMoreMaps()
    {
        return NextLevel != -1;
    }

    public MapLevels GetNextLevel()
    {
        return !HasMoreMaps() ? null : PresetLevels[NextLevel];
    }

    public static MapLevels GetLevelFromId(int id)
    {
        return PresetLevels[id];
    }
}
