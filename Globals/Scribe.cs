using System;
using System.Linq;
using FunkEngine;
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
            GD.Load<Texture2D>("res://scenes/BattleDirector/assets/Character1.png"),
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
    };

    public static readonly RelicTemplate[] RelicDictionary = new[]
    {
        new RelicTemplate(
            "Breakfast", //Reference ha ha, Item to give when relic pool is empty.
            "Breakfast, currently does nothing :).", //TODO: Description can include the relics values?
            GD.Load<Texture2D>("res://scenes/BattleDirector/assets/Character1.png"),
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.NotePlaced,
                    0,
                    (director, val) =>
                    {
                        director.Player.Heal(val);
                    }
                ),
            }
        ),
        new RelicTemplate(
            "Good Vibes",
            "Good vibes, heals the player whenever they place a note.", //TODO: Description can include the relics values?
            GD.Load<Texture2D>("res://scenes/BattleDirector/assets/Character1.png"),
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
}
