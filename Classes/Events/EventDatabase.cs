using System;
using System.Threading.Tasks;
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
                async (self, node) =>
                {
                    int randIndex = StageProducer.GlobalRng.RandiRange(
                        0,
                        StageProducer.PlayerStats.CurNotes.Length
                    );
                    StageProducer.PlayerStats.RemoveNote(randIndex);
                },
                async (self, node) =>
                {
                    int randIndex = StageProducer.GlobalRng.RandiRange(
                        0,
                        StageProducer.PlayerStats.CurRelics.Length
                    );
                    StageProducer.PlayerStats.RemoveRelic(randIndex);
                },
                async (self, node) =>
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
                async (self, node) =>
                {
                    var spinner = node.GetNodeOrNull<Sprite2D>("EventSprite");
                    int spinOutcome = (int)StageProducer.GlobalRng.RandWeighted([1, 1, 1, 1, 1, 1]);

                    int outcomeCount = 6;
                    float sectorAngle = 360f / outcomeCount;
                    float targetAngle = spinOutcome * sectorAngle;
                    float fullSpins = 6 * 360f;
                    float finalRotation = spinner.RotationDegrees % 360f + fullSpins + targetAngle;

                    var tween = node.GetTree().CreateTween();
                    tween
                        .TweenProperty(spinner, "rotation_degrees", finalRotation, 2.5f)
                        .SetTrans(Tween.TransitionType.Cubic)
                        .SetEase(Tween.EaseType.Out);

                    var tcs = new TaskCompletionSource<bool>();
                    tween.TweenCallback(Callable.From(() => tcs.SetResult(true)));
                    await tcs.Task;

                    switch (spinOutcome)
                    {
                        case 0:
                            GD.Print("lost half money");
                            StageProducer.PlayerStats.Money /= 2;
                            self.OutcomeDescriptions[0] = "EVENT_EVENT2_OUTCOME2";
                            break;
                        case 1:
                            GD.Print("took damage");
                            self.OutcomeDescriptions[0] = "EVENT_EVENT2_OUTCOME3";
                            StageProducer.PlayerStats.CurrentHealth = Math.Max(
                                1,
                                StageProducer.PlayerStats.CurrentHealth - 10
                            );
                            break;
                        case 2:
                            GD.Print("gain money");
                            self.OutcomeDescriptions[0] = "EVENT_EVENT2_OUTCOME4";
                            StageProducer.PlayerStats.Money += 50;
                            break;
                        case 3:
                            GD.Print("random note");
                            self.OutcomeDescriptions[0] = "EVENT_EVENT2_OUTCOME5";
                            StageProducer.PlayerStats.AddNote(
                                Scribe.GetRandomRewardNotes(1, StageProducer.CurRoom + 10)[0]
                            );
                            break;
                        case 4:
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

                    GD.Print($"new description {self.OutcomeDescriptions[0]}");
                },
                async (self, node) => {
                    // does nothing
                },
            ],
            GD.Load<Texture2D>("res://Classes/Events/Assets/Event2.png"),
            [() => true, () => true]
        ),
        new EventTemplate(
            2,
            "EVENT_EVENT3_DESC",
            ["EVENT_EVENT3_OPTION1", "EVENT_EVENT3_OPTION2", "EVENT_EVENT3_OPTION3"],
            ["EVENT_EVENT3_OUTCOME1", "EVENT_EVENT3_OUTCOME2", "EVENT_EVENT3_OUTCOME3"],
            [
                async (self, node) =>
                {
                    StageProducer.PlayerStats.CurrentHealth = Math.Min(
                        StageProducer.PlayerStats.CurrentHealth + 10,
                        StageProducer.PlayerStats.MaxHealth
                    );
                },
                async (self, node) =>
                {
                    StageProducer.PlayerStats.MaxComboBar -= 5;
                },
                async (self, node) =>
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
