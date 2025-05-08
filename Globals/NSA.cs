using System;
using GameAnalyticsSDK.Net;
using Godot;

public partial class NSA : Node
{
    public override void _EnterTree()
    {
        GameAnalytics.SetEnabledInfoLog(true);
        GameAnalytics.SetEnabledVerboseLog(true);
        GameAnalytics.ConfigureBuild("Midnight Riff: Milestone 2 DEV");
        GameAnalytics.Initialize(
            "cf1339a85b1909b3c92ca0d4e017fd31",
            "b32a52cb05696057075ec19d8a756bf1594563ad"
        );
    }

    public override void _ExitTree()
    {
        GameAnalytics.EndSession();
        base._ExitTree();
    }

    /**
     * Tells everyone you lost the game
     */
    public static void LogLevelEnd(
        string worldName,
        string levelName,
        bool win,
        int remainingHealth = -1
    )
    {
        GameAnalytics.AddProgressionEvent(
            win ? EGAProgressionStatus.Complete : EGAProgressionStatus.Fail,
            worldName,
            levelName,
            remainingHealth
        );
    }

    /// <summary>
    /// Sends the level start event to GameAnalytics.
    /// </summary>
    /// <param name="worldName">The more global name for the level. For example: Forest or City</param>
    /// <param name="levelName">The local name for the level</param>
    public static void LogLevelStart(string worldName, string levelName)
    {
        GameAnalytics.AddProgressionEvent(EGAProgressionStatus.Start, worldName, levelName);
    }
}
