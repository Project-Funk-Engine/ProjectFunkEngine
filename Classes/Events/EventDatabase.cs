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
            ["EVENT_EVENT1_OPTION1", "EVENT_EVENT1_OPTION2", "EVENT_EVENT1_OPTION3"],
            ["EVENT_EVENT1_OUTCOME1", "EVENT_EVENT1_OUTCOME2", "EVENT_EVENT1_OUTCOME3"],
            [
                (self) =>
                {
                    int randIndex = StageProducer.GlobalRng.RandiRange(
                        0,
                        StageProducer.PlayerStats.CurNotes.Length
                    );
                    StageProducer.PlayerStats.RemoveNote(randIndex);
                },
                (self) =>
                {
                    int randIndex = StageProducer.GlobalRng.RandiRange(
                        0,
                        StageProducer.PlayerStats.CurRelics.Length
                    );
                    StageProducer.PlayerStats.RemoveRelic(randIndex);
                },
                (self) =>
                {
                    StageProducer.PlayerStats.Money /= 2;
                },
            ],
            GD.Load<Texture2D>("res://Classes/Events/Assets/TEMP.png"),
            [
                () => StageProducer.PlayerStats.CurNotes.Length > 0,
                () => StageProducer.PlayerStats.CurRelics.Length > 0,
                () => StageProducer.PlayerStats.Money > 0,
            ]
        ),
        new EventTemplate(
            1,
            "EVENT_EVENT2_DESC",
            ["EVENT_EVENT2_OPTION1", "EVENT_EVENT2_OPTION2"],
            ["", "EVENT_EVENT2_OUTCOME1"],
            [
                (self) =>
                {
                    StageProducer.PlayerStats.Money -= 20;
                    // [do nothing, get money back, win money, get note, get relic, heal]
                    int spinOutcome = (int)
                        StageProducer.GlobalRng.RandWeighted([13, 8, 5, 5, 3, 3]); //TODO: adjust odds
                    switch (spinOutcome)
                    {
                        case 0: //do nothing AKA lose
                            GD.Print("owned lol");
                            self.OutcomeDescriptions[0] = "EVENT_EVENT2_OUTCOME2";
                            break;
                        case 1: // get money back
                            GD.Print("refund");
                            self.OutcomeDescriptions[0] = "EVENT_EVENT2_OUTCOME3";
                            StageProducer.PlayerStats.Money += 20;
                            break;
                        case 2: // get triple money
                            GD.Print("triple money");
                            self.OutcomeDescriptions[0] = "EVENT_EVENT2_OUTCOME4";
                            StageProducer.PlayerStats.Money += 60;
                            break;
                        case 3: // get random note
                            GD.Print("random note");
                            self.OutcomeDescriptions[0] = "EVENT_EVENT2_OUTCOME5";
                            StageProducer.PlayerStats.AddNote(
                                Scribe.GetRandomRewardNotes(1, StageProducer.CurRoom + 10)[0]
                            );
                            break;
                        case 4: // get random relic
                            GD.Print("random relic");
                            self.OutcomeDescriptions[0] = "EVENT_EVENT2_OUTCOME6";
                            StageProducer.PlayerStats.AddRelic(
                                Scribe.GetRandomRelics(
                                    1,
                                    StageProducer.CurRoom + 10,
                                    StageProducer.PlayerStats.RarityOdds
                                )[0]
                            );
                            break;
                        case 5:
                            GD.Print("heal");
                            self.OutcomeDescriptions[0] = "EVENT_EVENT2_OUTCOME7";
                            StageProducer.PlayerStats.CurrentHealth = Math.Min(
                                StageProducer.PlayerStats.CurrentHealth + 20,
                                StageProducer.PlayerStats.MaxHealth
                            );
                            break;
                    }
                },
                (self) => {
                    //does nothing
                },
            ],
            GD.Load<Texture2D>("res://Classes/Events/Assets/TEMP.png"),
            [() => StageProducer.PlayerStats.Money >= 20, () => true]
        ),
        new EventTemplate(
            2,
            "EVENT_EVENT3_DESC",
            ["EVENT_EVENT3_OPTION1", "EVENT_EVENT3_OPTION2", "EVENT_EVENT3_OPTION3"],
            ["EVENT_EVENT3_OUTCOME1", "EVENT_EVENT3_OUTCOME2", "EVENT_EVENT3_OUTCOME3"],
            [
                (self) =>
                {
                    StageProducer.PlayerStats.CurrentHealth = Math.Min(
                        StageProducer.PlayerStats.CurrentHealth + 10,
                        StageProducer.PlayerStats.MaxHealth
                    );
                },
                (self) =>
                {
                    StageProducer.PlayerStats.MaxComboBar -= 5;
                },
                (self) =>
                {
                    StageProducer.PlayerStats.Money -= 30;
                    StageProducer.PlayerStats.AddNote(Scribe.NoteDictionary[3]);
                    StageProducer.PlayerStats.AddNote(Scribe.NoteDictionary[3]);
                },
            ],
            GD.Load<Texture2D>("res://Classes/Events/Assets/TEMP.png"),
            [() => true, () => true, () => StageProducer.PlayerStats.Money >= 30]
        ),
    };
}
