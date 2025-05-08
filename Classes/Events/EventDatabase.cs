using System;
using System.Collections.Generic;
using Godot;

/// <summary>
/// Holds all game events and their associated logic.
/// </summary>
public partial class EventDatabase : Node
{
    public static readonly EventTemplate[] EventDictionary = new[]
    {
        new EventTemplate(
            1,
            "A wild creature appears in the forest.",
            new string[] { "Fight", "Run", "Mysterious third option" },
            new EventAction[]
            {
                () =>
                {
                    GD.Print("You chose to fight");
                },
                () =>
                {
                    GD.Print("You chose to run");
                },
                () =>
                {
                    GD.Print("You chose to Mysterious third option");
                },
            },
            GD.Load<Texture2D>("res://Classes/Events/Assets/TEMP.png")
        ),
    };
}
