using System;
using System.Linq;
using FunkEngine;
using FunkEngine.Classes.MidiMaestro;
using Godot;

/**
 * Global for storing defined data, e.g. Notes and Relic Dictionaries.
 */
public partial class Scribe : Node
{
    public static readonly Note[] NoteDictionary = new[]
    {
        new Note(
            0,
            "EnemyBase",
            "Basic enemy note, deals damage to player.",
            null,
            null,
            1,
            (director, note, timing) =>
            {
                director.Player.TakeDamage((3 - (int)timing) * note.GetBaseVal());
            }
        ),
        new Note(
            1,
            "PlayerBase",
            "Basic player note, deals damage to enemy.",
            GD.Load<Texture2D>("res://Classes/Notes/assets/single_note.png"),
            null,
            1,
            (director, note, timing) =>
            {
                director.Enemy.TakeDamage((int)timing * note.GetBaseVal());
            }
        ),
        new Note(
            2,
            "PlayerDouble",
            "Basic player note, deals double damage to enemy.",
            GD.Load<Texture2D>("res://Classes/Notes/assets/double_note.png"),
            null,
            1,
            (director, note, timing) =>
            {
                // can change later, but I want it like this instead of changing base
                // in case we have some relic that messes with timing
                director.Enemy.TakeDamage((2 * (int)timing) * note.GetBaseVal());
            }
        ),
        new Note(
            3,
            "PlayerHeal",
            "Basic player note, heals player.",
            GD.Load<Texture2D>("res://Classes/Notes/assets/heal_note.png"),
            null,
            1,
            (director, note, timing) =>
            {
                director.Player.Heal((int)timing * note.GetBaseVal());
            }
        ),
        new Note(
            4,
            "PlayerVampire",
            "Steals health from enemy.",
            GD.Load<Texture2D>("res://Classes/Notes/assets/vampire_note.png"),
            null,
            1,
            (director, note, timing) =>
            {
                director.Player.Heal((int)timing);
                director.Enemy.TakeDamage((int)timing * note.GetBaseVal());
            }
        ),
        new Note(
            5,
            "PlayerQuarter",
            "Basic note at a quarter of the cost.",
            GD.Load<Texture2D>("res://Classes/Notes/assets/quarter_note.png"),
            null,
            1,
            (director, note, timing) =>
            {
                director.Enemy.TakeDamage((int)timing * note.GetBaseVal());
            },
            0.25f
        ),
    };

    public static readonly RelicTemplate[] RelicDictionary = new[]
    {
        new RelicTemplate(
            0,
            "Breakfast", //Reference ha ha, Item to give when relic pool is empty.
            "Increases max hp.", //TODO: Description can include the relics values?
            GD.Load<Texture2D>("res://Classes/Relics/assets/relic_Breakfast.png"),
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.OnPickup,
                    10,
                    (director, self, val) =>
                    {
                        StageProducer.PlayerStats.MaxHealth += val;
                        StageProducer.PlayerStats.CurrentHealth += val;
                    }
                ),
            }
        ),
        new RelicTemplate(
            1,
            "Good Vibes",
            "Heals the player whenever they place a note.",
            GD.Load<Texture2D>("res://Classes/Relics/assets/relic_GoodVibes.png"),
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.NotePlaced,
                    2,
                    (director, self, val) =>
                    {
                        director.Player.Heal(val);
                    }
                ),
            }
        ),
        new RelicTemplate(
            2,
            "Auroboros",
            "Bigger number, better person. Increases combo multiplier every riff.",
            GD.Load<Texture2D>("res://Classes/Relics/assets/Auroboros.png"),
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.OnLoop,
                    1,
                    (director, self, val) =>
                    {
                        director.NotePlacementBar.IncreaseBonusMult(val);
                        self.Value++;
                    }
                ),
            }
        ),
        new RelicTemplate(
            3,
            "Colorboros",
            "Taste the rainbow. Charges the freestyle bar every riff.",
            GD.Load<Texture2D>("res://Classes/Relics/assets/Colorboros.png"),
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.OnLoop,
                    20,
                    (director, self, val) =>
                    {
                        director.NotePlacementBar.IncreaseCharge(val);
                        self.Value++;
                    }
                ),
            }
        ),
    };

    public static readonly SongTemplate[] SongDictionary = new[]
    {
        new SongTemplate(
            new SongData
            {
                Bpm = 120,
                SongLength = -1,
                NumLoops = 5,
            },
            "Song1",
            "Audio/Song1.ogg",
            "Audio/midi/Song1.mid"
        ),
        new SongTemplate(
            new SongData
            {
                Bpm = 60,
                SongLength = -1,
                NumLoops = 1,
            },
            "Song2",
            "Audio/Song2.ogg",
            "Audio/midi/Song2.mid",
            "res://scenes/Puppets/Enemies/Parasifly/Parasifly.tscn"
        ),
        new SongTemplate(
            new SongData
            {
                Bpm = 120,
                SongLength = -1,
                NumLoops = 1,
            },
            "Song3",
            "Audio/Song3.ogg",
            "Audio/midi/Song3.mid",
            "res://scenes/Puppets/Enemies/TheGWS/GWS.tscn"
        ),
    };

    //TODO: Item pool(s)

    public static RelicTemplate[] GetRandomRelics(RelicTemplate[] ownedRelics, int count)
    {
        var availableRelics = Scribe
            .RelicDictionary.Where(r => ownedRelics.All(o => o.Name != r.Name))
            .ToArray();

        availableRelics = availableRelics
            .OrderBy(_ => StageProducer.GlobalRng.Randi())
            .Take(count)
            .Select(r => r.Clone())
            .ToArray();

        for (int i = availableRelics.Length; i < count; i++)
        {
            availableRelics = availableRelics.Append(RelicDictionary[0].Clone()).ToArray();
        }
        return availableRelics;
    }

    public static Note[] GetRandomRewardNotes(int count)
    {
        var availableNotes = Scribe
            .NoteDictionary.Where(r => r.Name.Contains("Player")) //TODO: Classifications/pools
            .ToArray();

        availableNotes = availableNotes
            .OrderBy(_ => StageProducer.GlobalRng.Randi())
            .Take(count)
            .Select(r => r.Clone())
            .ToArray();

        return availableNotes;
    }
}
