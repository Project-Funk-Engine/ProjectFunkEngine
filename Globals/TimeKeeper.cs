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

    public static double PosMod(double i, double mod)
    {
        return (i % mod + mod) % mod;
    }
}
