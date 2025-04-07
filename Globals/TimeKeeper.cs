using FunkEngine;
using Godot;

/**
 * <summary>TimeKeeper: Global scale current song position.</summary>
 */
public partial class TimeKeeper : Node
{
    public static double CurrentTime; //sec
    public static double SongLength; //sec
    public static float Bpm;
    public static int LoopsPerSong;
    public static double ChartWidth; //px

    public const int SecondsPerMinute = 60;

    public static double LoopLength => SongLength / LoopsPerSong; //sec
    public static double BeatsPerLoop => LoopLength / (SecondsPerMinute / Bpm);
    public static double BeatsPerSong => SongLength / (SecondsPerMinute / Bpm);

    public static Beat LastBeat { get; set; }

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
        //If beatPos has returned to effectively < last beat pos, a loop has happened. IDK if there's a better way to handle this
        return result.BeatPos < LastBeat.BeatPos ? result.IncDecLoop(1) : result;
    }

    public static double GetTimeOfBeat(Beat beat)
    {
        return beat.GetBeatInSong() / BeatsPerSong * SongLength;
    }

    public static double GetPosOfBeat(Beat beat)
    {
        return beat.GetBeatInSong() / BeatsPerSong * (ChartWidth * LoopsPerSong);
    }

    public static double PosMod(double i, double mod)
    {
        return (i % mod + mod) % mod;
    }
}
