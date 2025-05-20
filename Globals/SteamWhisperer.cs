using Godot;

public partial class SteamWhisperer : Node
{ //I don't want to hunt down all the instances.
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
