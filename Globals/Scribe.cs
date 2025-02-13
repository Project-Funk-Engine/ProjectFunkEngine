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
            null,
            1,
            (director, note, timing) =>
            {
                director.Player.TakeDamage(4 - (int)timing);
            }
        ),
        new Note(
            "PlayerBase",
            null,
            1,
            (director, note, timing) =>
            {
                director.Enemy.TakeDamage((int)timing);
            }
        ),
    };

    public static readonly RelicTemplate[] RelicDictionary = new[]
    {
        new RelicTemplate(
            "Breakfast", //Reference ha ha, Item to give when relic pool is empty.
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
        new RelicTemplate(
            "Good Vibes",
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.NotePlaced,
                    5,
                    (director, val) =>
                    {
                        director.Player.Heal(val);
                    }
                ),
            }
        ),
        new RelicTemplate(
            "Dummy Item",
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.NotePlaced,
                    100,
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
