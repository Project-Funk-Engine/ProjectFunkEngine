using System.Linq;
using FunkEngine;
using FunkEngine.Classes.MidiMaestro;
using Godot;

/**
 * Catch all for storing defined data. Catch all as single source of truth for items and battles.
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
            GD.Load<Texture2D>("res://Classes/Notes/Assets/Note_PlayerBasic.png"),
            null,
            1,
            (director, note, timing) =>
            {
                if (timing == Timing.Miss)
                    return;
                director.Enemy.TakeDamage((int)timing * note.GetBaseVal());
            }
        ),
        new Note(
            2,
            "PlayerDouble",
            "Basic player note, deals double damage to enemy.",
            GD.Load<Texture2D>("res://Classes/Notes/Assets/Note_PlayerDouble.png"),
            null,
            2,
            (director, note, timing) =>
            {
                if (timing == Timing.Miss)
                    return;
                director.Enemy.TakeDamage(note.GetBaseVal() * (int)timing);
            }
        ),
        new Note(
            3,
            "PlayerHeal",
            "Basic player note, heals player.",
            GD.Load<Texture2D>("res://Classes/Notes/Assets/Note_PlayerHeal.png"),
            null,
            1,
            (director, note, timing) =>
            {
                if (timing == Timing.Miss)
                    return;
                director.Player.Heal((int)timing * note.GetBaseVal());
            }
        ),
        new Note(
            4,
            "PlayerVampire",
            "Steals health from enemy.",
            GD.Load<Texture2D>("res://Classes/Notes/Assets/Note_PlayerVampire.png"),
            null,
            1,
            (director, note, timing) =>
            {
                if (timing == Timing.Miss)
                    return;
                director.Player.Heal((int)timing * note.GetBaseVal());
                director.Enemy.TakeDamage((int)timing * note.GetBaseVal());
            }
        ),
        new Note(
            5,
            "PlayerQuarter",
            "Basic note at a quarter of the cost.",
            GD.Load<Texture2D>("res://Classes/Notes/Assets/Note_PlayerQuarter.png"),
            null,
            1,
            (director, note, timing) =>
            {
                if (timing == Timing.Miss)
                    return;
                director.Enemy.TakeDamage((int)timing + note.GetBaseVal());
            },
            0.25f
        ),
        new Note(
            6,
            "PlayerBlock",
            "Gives player one charge of block.",
            GD.Load<Texture2D>("res://Classes/Notes/Assets/Note_PlayerBlock.png"),
            null,
            1,
            (director, note, timing) =>
            {
                if (timing == Timing.Miss)
                    return;
                director.Player.GainShield(note.GetBaseVal()); //todo: should scale with timing????
            }
        ),
    };

    public static readonly RelicTemplate[] RelicDictionary = new[]
    {
        new RelicTemplate(
            0,
            "Breakfast", //Reference ha ha, Item to give when relic pool is empty.
            "Increases max hp.", //TODO: Description can include the relics values?
            GD.Load<Texture2D>("res://Classes/Relics/Assets/Relic_Breakfast.png"),
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
            GD.Load<Texture2D>("res://Classes/Relics/Assets/Relic_GoodVibes.png"),
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
            GD.Load<Texture2D>("res://Classes/Relics/Assets/Relic_Auroboros.png"),
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.OnLoop,
                    1,
                    (director, self, val) =>
                    {
                        director.NPB.IncreaseBonusMult(val);
                        self.Value++;
                    }
                ),
            }
        ),
        new RelicTemplate(
            3,
            "Colorboros",
            "Taste the rainbow. Charges the freestyle bar every riff.",
            GD.Load<Texture2D>("res://Classes/Relics/Assets/Relic_Colorboros.png"),
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.OnLoop,
                    10,
                    (director, self, val) =>
                    {
                        director.NPB.IncreaseCharge(val);
                        self.Value += 5;
                    }
                ),
            }
        ),
        new RelicTemplate(
            4,
            "Chips",
            "Hitting an enemy note deals a bit of damage.",
            GD.Load<Texture2D>("res://Classes/Relics/Assets/Relic_Chips.png"),
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.EnemyNoteHit,
                    1,
                    (director, self, val) =>
                    {
                        director.Enemy.TakeDamage(val);
                    }
                ),
            }
        ),
        new RelicTemplate(
            5,
            "Paper Cut",
            "Deals damage each loop.",
            GD.Load<Texture2D>("res://Classes/Relics/Assets/Relic_PaperCut.png"),
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.OnLoop,
                    5,
                    (director, self, val) =>
                    {
                        director.Enemy.TakeDamage(val);
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
            "Audio/Midi/Song1.mid"
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
            "Audio/Midi/Song2.mid",
            P_Parasifly.LoadPath
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
            "Audio/Midi/Song3.mid",
            P_TheGWS.LoadPath
        ),
    };

    //TODO: Item pool(s)

    public static RelicTemplate[] GetRandomRelics(RelicTemplate[] excludedRelics, int count)
    {
        var availableRelics = Scribe
            .RelicDictionary.Where(r => excludedRelics.All(o => o.Name != r.Name))
            .ToArray();

        RandomNumberGenerator lootRng = new RandomNumberGenerator();
        lootRng.SetSeed(StageProducer.GlobalRng.Seed + (ulong)StageProducer.CurRoom);

        availableRelics = availableRelics
            .OrderBy(_ => lootRng.Randi())
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

        RandomNumberGenerator lootRng = new RandomNumberGenerator();
        lootRng.SetSeed(StageProducer.GlobalRng.Seed + (ulong)StageProducer.CurRoom);

        availableNotes = availableNotes
            .OrderBy(_ => lootRng.Randi())
            .Take(count)
            .Select(r => r.Clone())
            .ToArray();

        return availableNotes;
    }
}
