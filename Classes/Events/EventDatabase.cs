using System;
using System.Collections.Generic;
using Godot;

/// <summary>
/// Holds all game events and their associated logic.
/// </summary>
public partial class EventDatabase
{
    public static readonly EventTemplate[] EventDictionary = new[]
    {
        new EventTemplate(
            1,
            "EVENT_EVENT1_DESC",
            new string[] { "EVENT_EVENT1_OPTION1", "EVENT_EVENT1_OPTION2", "EVENT_EVENT1_OPTION3" },
            new string[]
            {
                "EVENT_EVENT1_OUTCOME1",
                "EVENT_EVENT1_OUTCOME2",
                "EVENT_EVENT1_OUTCOME3",
            },
            new EventAction[]
            {
                () =>
                {
                    int randIndex = StageProducer.GlobalRng.RandiRange(
                        0,
                        StageProducer.PlayerStats.CurNotes.Length
                    );
                    StageProducer.PlayerStats.RemoveNote(randIndex);
                },
                () =>
                {
                    int randIndex = StageProducer.GlobalRng.RandiRange(
                        0,
                        StageProducer.PlayerStats.CurRelics.Length
                    );
                    StageProducer.PlayerStats.RemoveRelic(randIndex);
                },
                () =>
                {
                    StageProducer.PlayerStats.Money = (int)StageProducer.PlayerStats.Money / 2;
                },
            },
            GD.Load<Texture2D>("res://Classes/Events/Assets/TEMP.png")
        ),
    };
}
