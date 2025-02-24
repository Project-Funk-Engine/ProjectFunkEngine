using System.Collections.Generic;
using Godot;

public static class ControlSchemes
{
    public static Dictionary<string, Dictionary<string, string>> Schemes = new Dictionary<
        string,
        Dictionary<string, string>
    >()
    {
        {
            "WASD",
            new Dictionary<string, string>()
            {
                { "arrowUp", "W" },
                { "arrowDown", "S" },
                { "arrowLeft", "A" },
                { "arrowRight", "D" },
            }
        },
        {
            "ARROWS",
            new Dictionary<string, string>()
            {
                { "arrowUp", "Up" },
                { "arrowDown", "Down" },
                { "arrowLeft", "Left" },
                { "arrowRight", "Right" },
            }
        },
        {
            "QWERT",
            new Dictionary<string, string>()
            {
                { "arrowUp", "E" },
                { "arrowDown", "W" },
                { "arrowLeft", "Q" },
                { "arrowRight", "R" },
            }
        },
    };
}
