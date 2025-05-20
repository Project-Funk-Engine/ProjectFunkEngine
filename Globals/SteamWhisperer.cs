using Godot;

public partial class SteamWhisperer : Node
{
    /*public override void _EnterTree()
    {
        OS.SetEnvironment("SteamAppId", AppId.ToString());
        OS.SetEnvironment("SteamGameId", AppId.ToString());
    }*/

    /*public override void _ExitTree()
    {
        if (!Steam.IsSteamRunning())
            return;
        Steam.StoreStats();
        GD.Print("SW: Steam shut down.");
    }*/

    //TODO: This might fail sometimes? IDK, need to look into it
    public static bool PopAchievement(string id)
    {
        return false;
    }

    //Should make this more generic, but we only have one stat
    public static bool IncrementNoteCount()
    {
        return false;
    }

    //For Debugging purposes. Resets all stats/ achievements
    public static void ResetAll()
    {
        return;
    }
}
