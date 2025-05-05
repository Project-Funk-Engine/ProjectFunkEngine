using Godot;
using GodotSteam;

public partial class SteamWhisperer : Node
{
    private const uint AppId = 3647600;

    public static bool IsOverlayActive = false;

    public override void _EnterTree()
    {
        OS.SetEnvironment("SteamAppId", AppId.ToString());
        OS.SetEnvironment("SteamGameId", AppId.ToString());
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
        //Uncomment this to reset your achievements/stats. There's no confirmation so...
        //ResetAll();
    }

    //TODO: This might fail sometimes? IDK, need to look into it
    public static bool PopAchievement(string id)
    {
        if (!Steam.IsSteamRunning())
        {
            return false;
        }

        if (Steam.SetAchievement(id) && Steam.StoreStats())
        {
            GD.Print($"SW: Unlocked {id}.");
            return true;
        }

        GD.PrintErr($"SW: Failed to set achievement {id}.");
        return false;
    }

    //For Debugging purposes. Resets all stats/ achievements
    private static void ResetAll()
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
