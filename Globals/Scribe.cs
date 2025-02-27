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
            "EnemyBase",
            "Basic enemy note, deals damage to player.",
            null,
            null,
            1,
            (director, note, timing) =>
            {
                director.Player.TakeDamage(4 - (int)timing);
            }
        ),
        new Note(
            "PlayerBase",
            "Basic player note, deals damage to enemy",
            GD.Load<Texture2D>("res://Classes/Notes/assets/single_note.png"),
            null,
            1,
            (director, note, timing) =>
            {
                director.Enemy.TakeDamage((int)timing);
            }
        ),
        new Note(
            "PlayerDouble",
            "Basic player note, deals double damage to enemy",
            GD.Load<Texture2D>("res://Classes/Notes/assets/double_note.png"),
            null,
            1,
            (director, note, timing) =>
            {
                // can change later, but I want it like this instead of changing base
                // in case we have some relic that messes with timing
                director.Enemy.TakeDamage(2 * (int)timing);
            }
        ),
        new Note(
            "PlayerHeal",
            "Basic player note, heals player",
            GD.Load<Texture2D>("res://Classes/Notes/assets/heal_note.png"),
            null,
            1,
            (director, note, timing) =>
            {
                director.Player.Heal((int)timing);
            }
        ),
        new Note(
            "PlayerVampire",
            "Steals health from enemy",
            GD.Load<Texture2D>("res://Classes/Notes/assets/vampire_note.png"),
            null,
            1,
            (director, note, timing) =>
            {
                director.Player.Heal((int)timing);
                director.Enemy.TakeDamage((int)timing);
            }
        ),
        new Note(
            "PlayerQuarter",
            "Basic note at a quarter of the cost",
            GD.Load<Texture2D>("res://Classes/Notes/assets/quarter_note.png"),
            null,
            1,
            (director, note, timing) =>
            {
                director.Enemy.TakeDamage((int)timing);
            },
            0.25f
        ),
    };

    public static readonly RelicTemplate[] RelicDictionary = new[]
    {
        new RelicTemplate(
            "Breakfast", //Reference ha ha, Item to give when relic pool is empty.
            "Increases max hp.", //TODO: Description can include the relics values?
            GD.Load<Texture2D>("res://Classes/Relics/assets/relic_Breakfast.png"),
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.OnPickup,
                    10,
                    (director, val) =>
                    {
                        StageProducer.PlayerStats.MaxHealth += val;
                        StageProducer.PlayerStats.CurrentHealth += val;
                    }
                ),
            }
        ),
        new RelicTemplate(
            "Good Vibes",
            "Good vibes, heals the player whenever they place a note.", //TODO: Description can include the relics values?
            GD.Load<Texture2D>("res://Classes/Relics/assets/relic_GoodVibes.png"),
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.NotePlaced,
                    1,
                    (director, val) =>
                    {
                        director.Player.Heal(val);
                    }
                ),
            }
        ),
    };

    //TODO: Item pool(s)

    public static RelicTemplate[] GetRandomRelics(RelicTemplate[] ownedRelics, int count)
    {
        var availableRelics = Scribe
            .RelicDictionary.Where(r => !ownedRelics.Any(o => o.Name == r.Name))
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
            "Audio/335571__magntron__gamemusic_120bpm.mp3",
            "Audio/midi/midiTest.mid"
        ),
        new SongTemplate(
            new SongData
            {
                Bpm = 60,
                SongLength = -1,
                NumLoops = 1,
            },
            "Song2",
            "Audio/620230__josefpres__dark-loops-220-octave-piano-with-efect-short-loop-60-bpm.wav",
            "Audio/midi/midiTest2.mid",
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
            "Audio/789998__josefpres__piano-loops-181-octave-short-loop-120-bpm.wav",
            "Audio/midi/midiTest3.mid",
            "res://scenes/Puppets/Enemies/TheGWS/GWS.tscn"
        ),
    };
}
