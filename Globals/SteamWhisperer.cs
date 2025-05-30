using Godot;
using GodotSteam;

public partial class SteamWhisperer : Node
{
    private const uint AppId = 3647600;

    public static bool IsOverlayActive = false;

    private static int placedNotes = 0;

    public override void _EnterTree()
    {
        OS.SetEnvironment("SteamAppId", AppId.ToString());
        OS.SetEnvironment("SteamGameId", AppId.ToString());
    }

    public override void _ExitTree()
    {
        if (!Steam.IsSteamRunning())
            return;
        Steam.StoreStats();
        GD.Print("SW: Steam shut down.");
    }

    public override void _Ready()
    {
        if (!Steam.SteamInit(AppId, true))
        {
            GD.PrintErr(
                "SW: here was an error initializing Steam. No Steam features will be available."
            );
            return;
        }

        GD.Print("SW: Steam initialized successfully.");
        Steam.OverlayToggled += (active, _, _) =>
        {
            IsOverlayActive = active;
        };

        //Pull in stats
        placedNotes = Steam.GetStatInt("NotesPlaced");
        GD.Print($"SW: Placed notes: {placedNotes}");

        //Uncomment this to reset your achievements/stats. There's no confirmation so...
        //ResetAll();
    }

    //TODO: This might fail sometimes? IDK, need to look into it
    public static bool PopAchievement(string id)
    {
        return false;
    }

    //Should make this more generic, but we only have one stat
    public static bool IncrementNoteCount()
    {
        if (!Steam.IsSteamRunning())
        {
            return false;
        }

        placedNotes++;

        if (Steam.SetStatInt("NotesPlaced", placedNotes) && Steam.StoreStats())
        {
            GD.Print($"SW: Incremented placed notes to {placedNotes}.");
            return true;
        }
        GD.PrintErr($"SW: Failed to increment placed notes to {placedNotes}.");
        return false;
    }

    //For Debugging purposes. Resets all stats/ achievements
    public static void ResetAll()
    {
        if (!Steam.IsSteamRunning())
        {
            return;
        }

        Steam.ResetAllStats(true);
        Steam.StoreStats();
        GD.Print("SW: All stats reset.");
    }
}
