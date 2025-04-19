using System.Collections.Generic;
using System.Linq;
using FunkEngine;
using FunkEngine.Classes.MidiMaestro;
using Godot;

// ReSharper disable UnusedParameter.Local

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
                int dmg = (3 - (int)timing) * note.GetBaseVal();
                director.Player.TakeDamage(new DamageInstance(dmg, null, director.Player));
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
                director.DealDamage(note, (int)timing * note.GetBaseVal(), director.Player);
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
                director.DealDamage(note, (int)timing * note.GetBaseVal(), director.Player);
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
                int dmg = (int)timing * note.GetBaseVal();
                director.Player.Heal(dmg);
                director.DealDamage(note, dmg, director.Player);
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
                director.DealDamage(note, (int)timing * note.GetBaseVal(), director.Player);
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
                director.AddStatus(Targetting.First, StatusEffect.Block.GetInstance(), true); //todo: should scale with timing????
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
            Rarity.Rare, //This thing is really good imo.
            GD.Load<Texture2D>("res://Classes/Relics/Assets/Relic_Chips.png"),
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.NoteHit,
                    1,
                    (e, self, val) =>
                    {
                        if (e is not BattleDirector.Harbinger.NoteHitArgs noteHitArgs)
                            return;
                        if (noteHitArgs.Timing != Timing.Miss)
                            e.BD.DealDamage(Targetting.First, val, null);
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
                        e.BD.DealDamage(Targetting.First, val, null);
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

    //Needs to be strictly maintained based on what the player has obtained.
    private static List<int>[] _relicRarityPools = null;

    public static void InitRelicPools()
    {
        _relicRarityPools = new List<int>[(int)Rarity.Breakfast + 1];
        for (int i = 0; i <= (int)Rarity.Breakfast; i++)
        {
            _relicRarityPools[i] = new List<int>();
        }

        foreach (RelicTemplate relic in RelicDictionary)
        {
            _relicRarityPools[(int)relic.Rarity].Add(relic.Id);
        }
    }

    //TODO: Keep sorted by Id for faster binary search.
    private static void AddRelicToPool(RelicTemplate relic)
    {
        if (relic.Rarity == Rarity.Breakfast)
            return;
        int indexRelic = _relicRarityPools[(int)relic.Rarity].IndexOf(relic.Id);
        if (indexRelic == -1)
        {
            _relicRarityPools[(int)relic.Rarity].Add(relic.Id);
        }
    }

    public static void RemoveRelicFromPool(RelicTemplate relic)
    {
        if (relic.Rarity == Rarity.Breakfast)
            return;
        int indexRelic = _relicRarityPools[(int)relic.Rarity].IndexOf(relic.Id);
        if (indexRelic == -1)
        {
            GD.PushWarning(
                "Attempting to remove relic " + relic.Id + " from the Relic Pool, not found!"
            );
            return;
        }
        _relicRarityPools[(int)relic.Rarity].RemoveAt(indexRelic);
    }

    /// <summary>
    /// Return an array of relics for reward selection.
    /// Intended usage of rarity. Player Stats has the rarity distribution. Do rolls in descending order of rarity.
    /// Get a relic for the rolled rarity, continue for count.
    /// If the relic pool is out of the rolled rarity, be nice to player and give them a relic of higher rarity.
    /// Continue through ascending rarities until no new relics are acquirable, then give Breakfast.
    /// </summary>
    /// <param name="count">Number of relics to generate.</param>
    /// <param name="lootOffset">An offset for the loot rng seed.</param>
    /// <param name="odds">An array of the int odds out of 100 for each typical rarity (Common through Legendary).</param>
    /// <returns></returns>
    public static RelicTemplate[] GetRandomRelics(int count, int lootOffset, int[] odds)
    {
        RelicTemplate[] result = new RelicTemplate[count];

        RandomNumberGenerator lootRng = new RandomNumberGenerator();
        lootRng.SetSeed(StageProducer.GlobalRng.Seed + (ulong)lootOffset);

        for (int i = 0; i < count; i++)
        {
            Rarity rarity = RollRarities(odds, lootRng);
            RelicTemplate relic = H_GetRandomRelic(rarity, lootRng);
            result[i] = relic;
        }
        //Re-add relics back to pools.
        foreach (RelicTemplate relic in result)
        {
            _relicRarityPools[(int)relic.Rarity].Add(relic.Id);
        }

        return result;
    }

    private static Rarity RollRarities(int[] rarityOdds, RandomNumberGenerator rng)
    {
        int rarityRoll = rng.RandiRange(1, 100);
        for (int i = 0; i < rarityOdds.Length; i++)
        {
            if (rarityRoll < rarityOdds[i])
            {
                return (Rarity)i;
            }
        }

        return Rarity.Common;
    }

    private static RelicTemplate H_GetRandomRelic(Rarity startingRarity, RandomNumberGenerator rng)
    {
        int countOfRarity = 0;
        Rarity currentRarity = startingRarity;

        while (countOfRarity <= 0) //While there are no options of current rarity selected
        {
            countOfRarity = _relicRarityPools[(int)currentRarity].Count;

            if (countOfRarity > 0) //There are relics of a rarity
            { //Select a random relic of rarity.
                int relicIndex = rng.RandiRange(0, countOfRarity - 1);
                int selectedRelicId = _relicRarityPools[(int)currentRarity][relicIndex];
                RelicTemplate result = RelicDictionary[selectedRelicId].Clone();
                RemoveRelicFromPool(result); //Prevent same relic being selected in same selection process.
                return result;
            }

            //Rotate through, in increasing rarity. Technically right now it will go Legendary -> Common before Uncommon, this is ok for now, but should be noted.
            currentRarity = (Rarity)Mathf.PosMod((int)(currentRarity - 1), (int)Rarity.Breakfast);
            if (currentRarity == startingRarity)
                countOfRarity = 1; //Gone through all rarities, found no valid relic, exit loop to throw Breakfast.
        }
        return RelicDictionary[0].Clone();
    }

    public static Note[] GetRandomRewardNotes(int count, int lootOffset)
    {
        var availableNotes = Scribe
            .NoteDictionary.Where(r => r.Name.Contains("Player")) //TODO: Classifications/pools
            .ToArray();

        RandomNumberGenerator lootRng = new RandomNumberGenerator();
        lootRng.SetSeed(StageProducer.GlobalRng.Seed + (ulong)lootOffset);

        availableNotes = availableNotes
            .OrderBy(_ => lootRng.Randi())
            .Take(count)
            .Select(r => r.Clone())
            .ToArray();

        return availableNotes;
    }
}
