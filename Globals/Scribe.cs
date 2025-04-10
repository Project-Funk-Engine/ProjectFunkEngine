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
                director.GetFirstEnemy()?.TakeDamage((int)timing * note.GetBaseVal());
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
                director.GetFirstEnemy()?.TakeDamage(note.GetBaseVal() * (int)timing);
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
                director.GetFirstEnemy()?.TakeDamage((int)timing * note.GetBaseVal());
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
                director.GetFirstEnemy()?.TakeDamage((int)timing + note.GetBaseVal());
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
            Rarity.Breakfast,
            GD.Load<Texture2D>("res://Classes/Relics/Assets/Relic_Breakfast.png"),
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.OnPickup,
                    10,
                    (e, self, val) =>
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
            Rarity.Common,
            GD.Load<Texture2D>("res://Classes/Relics/Assets/Relic_GoodVibes.png"),
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.NotePlaced,
                    2,
                    (e, self, val) =>
                    {
                        e.BD.Player.Heal(val);
                    }
                ),
            }
        ),
        new RelicTemplate(
            2,
            "Auroboros",
            "Bigger number, better person. Increases combo multiplier every riff.",
            Rarity.Common,
            GD.Load<Texture2D>("res://Classes/Relics/Assets/Relic_Auroboros.png"),
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.OnLoop,
                    1,
                    (e, self, val) =>
                    {
                        e.BD.NPB.IncreaseBonusMult(val);
                        self.Value++;
                    }
                ),
            }
        ),
        new RelicTemplate(
            3,
            "Colorboros",
            "Taste the rainbow. Charges the freestyle bar every riff.",
            Rarity.Common,
            GD.Load<Texture2D>("res://Classes/Relics/Assets/Relic_Colorboros.png"),
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.OnLoop,
                    10,
                    (e, self, val) =>
                    {
                        e.BD.NPB.IncreaseCharge(val);
                        self.Value += 5;
                    }
                ),
            }
        ),
        new RelicTemplate(
            4,
            "Chips",
            "Hitting a note deals a bit of damage.",
            Rarity.Common,
            GD.Load<Texture2D>("res://Classes/Relics/Assets/Relic_Chips.png"),
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.NoteHit,
                    1,
                    (e, self, val) =>
                    {
                        e.BD.GetFirstEnemy().TakeDamage(val);
                    }
                ),
            }
        ),
        new RelicTemplate(
            5,
            "Paper Cut",
            "Deals damage each loop.",
            Rarity.Common,
            GD.Load<Texture2D>("res://Classes/Relics/Assets/Relic_PaperCut.png"),
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.OnLoop,
                    5,
                    (e, self, val) =>
                    {
                        e.BD.GetFirstEnemy().TakeDamage(val);
                    }
                ),
            }
        ),
        new RelicTemplate(
            6,
            "Energy Drink",
            "Take a chance to cool down and sip an energy drink to increase your max energy bar.",
            Rarity.Common,
            GD.Load<Texture2D>("res://Classes/Relics/Assets/Relic_EnergyDrink.png"),
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.OnPickup,
                    10,
                    (e, self, val) =>
                    {
                        StageProducer.PlayerStats.MaxComboBar -= val;
                    }
                ),
            }
        ),
        new RelicTemplate(
            7,
            "Bandage",
            "A clean strip of cloth. Use it after a fight to patch up and feel better.",
            Rarity.Common,
            GD.Load<Texture2D>("res://Classes/Relics/Assets/Relic_Bandage.png"),
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.OnBattleEnd,
                    10,
                    (e, self, val) =>
                    {
                        StageProducer.PlayerStats.CurrentHealth += val;
                    }
                ),
            }
        ),
        new RelicTemplate(
            8,
            "Medkit",
            "A small kit with medical supplies. Heals you a bit after each loop.",
            Rarity.Common,
            GD.Load<Texture2D>("res://Classes/Relics/Assets/Relic_Medkit.png"),
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.OnLoop,
                    5,
                    (e, self, val) =>
                    {
                        e.BD.Player.Heal(val);
                    }
                ),
            }
        ),
        new RelicTemplate(
            9,
            "Vinyl Record",
            "Right round, right round. All loop effects trigger twice.",
            Rarity.Legendary,
            GD.Load<Texture2D>("res://Classes/Relics/Assets/Relic_VinylRecord.png"),
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.OnLoop,
                    0,
                    (e, self, val) =>
                    {
                        if (
                            (e is BattleDirector.Harbinger.LoopEventArgs eLoop)
                            && !eLoop.ArtificialLoop
                        )
                            BattleDirector.Harbinger.Instance.InvokeChartLoop(eLoop.Loop);
                    }
                ),
            }
        ),
    };

    public static readonly SongTemplate[] SongDictionary = new[] //Generalize and make pools for areas/room types
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
            "Audio/Midi/Song1.mid",
            [P_BossBlood.LoadPath]
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
            [P_Parasifly.LoadPath]
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
            [P_Parasifly.LoadPath, P_Parasifly.LoadPath]
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
            [P_TheGWS.LoadPath]
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
