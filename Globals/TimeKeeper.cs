using FunkEngine;
using Godot;

/**
 * <summary>TimeKeeper: Global scale current song position.</summary>
 */
public partial class TimeKeeper : Node
{
    public static double CurrentTime = 0;
    public static float ChartLength;
    public static float LoopLength;
    public static float Bpm;
    public static Beat LastBeat { get; set; }
    public static double BeatsPerLoop;

    public static void InitVals(float bpm)
    {
        CurrentTime = 0;
        Bpm = bpm;
        LastBeat = new Beat();
    }

    public static Beat GetBeatFromTime(double timeSecs)
    {
        double beatPos = timeSecs / (60 / (double)Bpm);
        Beat result = new Beat(beatPos, LastBeat.Loop);
        //If beatPos has returned to effectively < last beat pos, a loop has happened. Idk if there's a better way to handle this
        return result.BeatPos < LastBeat.BeatPos ? result.IncDecLoop(1) : result;
    }

    public static double PosMod(double i, double mod)
    {
        return (i % mod + mod) % mod;
    }
}
