using System;
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
        new Note(
            "PlayerDouble",
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
    };
}
