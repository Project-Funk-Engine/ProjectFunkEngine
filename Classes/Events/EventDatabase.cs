using System;
using Godot;

/// <summary>
/// Holds all game events and their associated logic.
/// </summary>

public class EventDatabase
{
    public const int EventDatabaseSize = 3;

    public static readonly EventTemplate[] EventDictionary = new[]
    {
        new EventTemplate(
            0,
            "EVENT_EVENT1_DESC",
            ["EVENT_EVENT1_OPTION1", "EVENT_EVENT1_OPTION2", "EVENT_EVENT1_OPTION3"],
            ["EVENT_EVENT1_OUTCOME1", "EVENT_EVENT1_OUTCOME2", "EVENT_EVENT1_OUTCOME3"],
            [
                (self, node) =>
                {
                    int randIndex = StageProducer.GlobalRng.RandiRange(
                        0,
                        StageProducer.PlayerStats.CurNotes.Length - 1
                    );
                    var note = StageProducer.PlayerStats.CurNotes[randIndex];
                    string name = note.Name.ToUpper().Replace(" ", "");
                    string localizedName = TranslationServer.Translate("NOTE_" + name + "_NAME");
                    StageProducer.PlayerStats.RemoveNote(randIndex);

                    self.OutcomeDescriptions[0] = string.Format(
                        TranslationServer.Translate("EVENT_EVENT1_OUTCOME1"),
                        localizedName
                    );
                },
                (self, node) =>
                {
                    int randIndex = StageProducer.GlobalRng.RandiRange(
                        0,
                        StageProducer.PlayerStats.CurRelics.Length - 1
                    );
                    var relic = StageProducer.PlayerStats.CurRelics[randIndex];
                    string name = relic.Name.ToUpper().Replace(" ", "");
                    string localizedName = TranslationServer.Translate("RELIC_" + name + "_NAME");
                    StageProducer.PlayerStats.RemoveRelic(randIndex);

                    self.OutcomeDescriptions[1] = string.Format(
                        TranslationServer.Translate("EVENT_EVENT1_OUTCOME2"),
                        localizedName
                    );
                },
                (self, node) =>
                {
                    string stolenMoney = (StageProducer.PlayerStats.Money / 2).ToString();
                    StageProducer.PlayerStats.Money /= 2;

                    self.OutcomeDescriptions[2] = self.OutcomeDescriptions[0] = string.Format(
                        TranslationServer.Translate("EVENT_EVENT1_OUTCOME3"),
                        stolenMoney
                    );
                },
            ],
            GD.Load<Texture2D>("res://Classes/Events/Assets/Bandit_Event.png"),
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
                (self, node) =>
                {
                    var spinner = node.EventSprite;
                    int spinOutcome = StageProducer.GlobalRng.RandiRange(0, 5);

                    int outcomeCount = 6;
                    float sectorAngle = 360f / outcomeCount;
                    float targetAngle = spinOutcome * sectorAngle;
                    float fullSpins = 6 * 360f;
                    float finalRotation = spinner.RotationDegrees % 360f + fullSpins + targetAngle;

                    var tween = node.CreateTween();
                    tween
                        .TweenProperty(spinner, "rotation_degrees", finalRotation, 2.5f)
                        .SetTrans(Tween.TransitionType.Cubic)
                        .SetEase(Tween.EaseType.Out);

                    // Defer execution of the outcome until the tween finishes
                    string eventEffect = "";
                    tween.TweenCallback(
                        Callable.From(() =>
                        {
                            switch (spinOutcome)
                            {
                                case 0:
                                    eventEffect = (StageProducer.PlayerStats.Money / 2).ToString();
                                    StageProducer.PlayerStats.Money /= 2;
                                    self.OutcomeDescriptions[0] = string.Format(
                                        TranslationServer.Translate("EVENT_EVENT2_OUTCOME2"),
                                        eventEffect
                                    );
                                    break;
                                case 1:
                                    self.OutcomeDescriptions[0] = "EVENT_EVENT2_OUTCOME3";
                                    StageProducer.PlayerStats.CurrentHealth = Math.Max(
                                        1,
                                        StageProducer.PlayerStats.CurrentHealth - 10
                                    );
                                    break;
                                case 2:
                                    self.OutcomeDescriptions[0] = "EVENT_EVENT2_OUTCOME4";
                                    StageProducer.PlayerStats.Money += 50;
                                    break;
                                case 3:
                                    StageProducer.PlayerStats.AddNote(
                                        Scribe.GetRandomRewardNotes(1, StageProducer.CurRoom + 10)[
                                            0
                                        ]
                                    );
                                    var note = StageProducer.PlayerStats.CurNotes[^1];
                                    string name = note.Name.ToUpper().Replace(" ", "");
                                    eventEffect = TranslationServer.Translate(
                                        "NOTE_" + name + "_NAME"
                                    );

                                    self.OutcomeDescriptions[0] = string.Format(
                                        TranslationServer.Translate("EVENT_EVENT2_OUTCOME5"),
                                        eventEffect
                                    );
                                    break;
                                case 4:
                                    StageProducer.PlayerStats.AddRelic(
                                        Scribe.GetRandomRelics(
                                            1,
                                            StageProducer.CurRoom + 10,
                                            StageProducer.PlayerStats.RarityOdds
                                        )[0]
                                    );

                                    var relic = StageProducer.PlayerStats.CurRelics[^1];
                                    string name1 = relic.Name.ToUpper().Replace(" ", "");
                                    eventEffect = TranslationServer.Translate(
                                        "NOTE_" + name1 + "_NAME"
                                    );
                                    self.OutcomeDescriptions[0] = string.Format(
                                        TranslationServer.Translate("EVENT_EVENT2_OUTCOME6"),
                                        eventEffect
                                    );
                                    break;
                                case 5:
                                    self.OutcomeDescriptions[0] = "EVENT_EVENT2_OUTCOME7";
                                    StageProducer.PlayerStats.CurrentHealth = Math.Min(
                                        StageProducer.PlayerStats.CurrentHealth + 20,
                                        StageProducer.PlayerStats.MaxHealth
                                    );
                                    break;
                            }
                            node.AnyButtonPressed(0);
                            self.OutcomeDescriptions[0] = ""; //Will need to fix later, currently changes the primary reference
                        })
                    );
                },
                null,
            ],
            GD.Load<Texture2D>("res://Classes/Events/Assets/Event2.png"),
            [null, null]
        ),
        new EventTemplate(
            2,
            "EVENT_EVENT3_DESC",
            ["EVENT_EVENT3_OPTION1", "EVENT_EVENT3_OPTION2", "EVENT_EVENT3_OPTION3"],
            ["EVENT_EVENT3_OUTCOME1", "EVENT_EVENT3_OUTCOME2", "EVENT_EVENT3_OUTCOME3"],
            [
                (self, node) =>
                {
                    StageProducer.PlayerStats.CurrentHealth = Math.Min(
                        StageProducer.PlayerStats.CurrentHealth + 10,
                        StageProducer.PlayerStats.MaxHealth
                    );
                },
                (self, node) =>
                {
                    StageProducer.PlayerStats.MaxComboBar -= 5;
                },
                (self, node) =>
                {
                    StageProducer.PlayerStats.Money -= 30;
                    StageProducer.PlayerStats.AddNote(Scribe.NoteDictionary[3]);
                    StageProducer.PlayerStats.AddNote(Scribe.NoteDictionary[3]);
                },
            ],
            GD.Load<Texture2D>("res://Classes/Events/Assets/Medic_Event.png"),
            [null, null, () => StageProducer.PlayerStats.Money >= 30]
        ),
    };
}
