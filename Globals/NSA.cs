using System;
using System.Collections;
using System.IO;
using GameAnalyticsSDK.Net;
using Godot;
using Environment = System.Environment;

public partial class NSA : Node
{
    private double avgFPS = 0;
    private double previousAvgFPS = 0;

    //Gonna store stuff so we dont trash FPS with file IO
    static ArrayList logs = new ArrayList();

    public override void _EnterTree()
    {
        GameAnalytics.SetEnabledInfoLog(true);
        GameAnalytics.SetEnabledVerboseLog(true);
        GameAnalytics.ConfigureBuild("Midnight Riff: Milestone 2 DEV");
        GameAnalytics.Initialize(
            "cf1339a85b1909b3c92ca0d4e017fd31",
            "b32a52cb05696057075ec19d8a756bf1594563ad"
        );
        avgFPS = Engine.GetFramesPerSecond();
    }

    public override void _ExitTree()
    {
        GameAnalytics.EndSession();
        base._ExitTree();

        ResetTextLog();
    }

    public override void _Process(double delta)
    {
        //Log if we drop below 10 FPS
        if (Engine.GetFramesPerSecond() < 10)
        {
            addTextLog("Low FPS: " + Engine.GetFramesPerSecond());
            GameAnalytics.AddDesignEvent("LowFPS : " + Engine.GetFramesPerSecond());
        }

        //Log if our average FPS drops below previous and is below 59
        previousAvgFPS = avgFPS;
        avgFPS = (avgFPS + Engine.GetFramesPerSecond()) / 2;

        if (avgFPS < previousAvgFPS && avgFPS < 59)
        {
            GameAnalytics.AddDesignEvent("AvgFPSDecreased:" + avgFPS);
            addTextLog("Average FPS below threshold: " + avgFPS);
        }
    }

    /// <summary>
    /// Sends the level end event to GameAnalytics.
    /// </summary>
    /// <param name="worldName">The more global name for the level. For example: Forest or City</param>
    /// <param name="levelName">The local name for the level</param>
    /// <param name="win">True if the battle was won, False otherwise</param>
    /// <param name="remainingHealth">The remaining health of the player at the end of the battle</param>
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
        addTextLog(
            "Completed Level in world: "
                + worldName
                + " level: "
                + levelName
                + " with remaining health:  "
                + remainingHealth
                + " did they win?: "
                + win
        );
    }

    /// <summary>
    /// Sends the level start event to GameAnalytics.
    /// </summary>
    /// <param name="worldName">The more global name for the level. For example: Forest or City</param>
    /// <param name="levelName">The local name for the level</param>
    public static void LogLevelStart(string worldName, string levelName, string enemyName)
    {
        GameAnalytics.AddProgressionEvent(EGAProgressionStatus.Start, worldName, levelName);
        addTextLog(
            "Starting Level in world: "
                + worldName
                + " level: "
                + levelName
                + " enemy: "
                + enemyName
        );
    }

    /// <summary>
    /// Sends the key remap event to GameAnalytics.
    /// </summary>
    /// <param name="inputType">The type of interface being remapped. Please keep as "keyboard" or "controller"</param>
    /// <param name="keyName">The name of the key being mapped</param>
    /// <param name="setKey">the int value of the key. I can't do this as anything else.</param>
    public static void LogRemapEvent(string inputType, string keyName, int setKey)
    {
        GameAnalytics.AddDesignEvent(inputType + "Remap:" + keyName, setKey);
        addTextLog("Remapping " + inputType + " key: " + keyName + " to: " + setKey);
    }

    public static void addTextLog(string text)
    {
        logs.Add(DateTime.Now + ": " + text);
    }

    public static void ResetTextLog()
    {
        //write the logs to file
        string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        using (
            StreamWriter outputFile = new StreamWriter(
                Path.Combine(docPath, $"Log {DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt")
            )
        )
        {
            foreach (string line in logs)
                outputFile.WriteLine(line);
        }

        //clear the logs
        logs.Clear();
    }
}
