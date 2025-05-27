using System;
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
            GD.Load<Texture2D>("res://Classes/Notes/Assets/Note_PlayerBasic.png"),
            4,
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
            GD.Load<Texture2D>("res://Classes/Notes/Assets/Note_PlayerDouble.png"),
            8,
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
            GD.Load<Texture2D>("res://Classes/Notes/Assets/Note_PlayerHeal.png"),
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
            GD.Load<Texture2D>("res://Classes/Notes/Assets/Note_PlayerVampire.png"),
            3,
            (director, note, timing) =>
            {
                if (timing == Timing.Miss)
                    return;
                int dmg = (int)timing;
                director.Player.Heal(dmg);
                director.DealDamage(note, dmg * note.GetBaseVal(), director.Player);
            }
        ),
        new Note(
            5,
            "PlayerQuarter",
            GD.Load<Texture2D>("res://Classes/Notes/Assets/Note_PlayerQuarter.png"),
            3,
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
            GD.Load<Texture2D>("res://Classes/Notes/Assets/Note_PlayerBlock.png"),
            1,
            (director, note, timing) =>
            {
                if (timing == Timing.Miss)
                    return;
                director.AddStatus(Targetting.Player, StatusEffect.Block.CreateInstance()); //todo: should scale with timing????
            }
        ),
        new Note(
            7,
            "PlayerExplosive",
            GD.Load<Texture2D>("res://Classes/Notes/Assets/Note_PlayerExplosive.png"),
            4,
            (director, note, timing) =>
            {
                if (timing == Timing.Miss)
                    return;
                director.DealDamage(note, (int)timing * note.GetBaseVal(), director.Player);
            },
            1f,
            Targetting.All
        ),
        new Note(
            8,
            "PlayerEcho",
            GD.Load<Texture2D>("res://Classes/Notes/Assets/Note_PlayerEcho.png"),
            4,
            (director, note, timing) =>
            {
                if (timing == Timing.Miss)
                    return;
                director.DealDamage(note, (int)timing * note.GetBaseVal(), director.Player);
                note.SetBaseVal(note.GetBaseVal() + 2);
            }
        ),
        new Note(
            9,
            "PlayerPoison",
            GD.Load<Texture2D>("res://Classes/Notes/Assets/Note_PlayerPoison.png"),
            1,
            (director, note, timing) =>
            {
                if (timing == Timing.Miss)
                    return;
                director.AddStatus(Targetting.First, StatusEffect.Poison, (int)timing);
            }
        ),
        new Note(
            10,
            "GWS",
            GD.Load<Texture2D>("res://Classes/Notes/Assets/Note_GWS.png"),
            1,
            (director, note, timing) =>
            {
                int dmg = 2 * (3 - (int)timing) * note.GetBaseVal() + TimeKeeper.LastBeat.Loop; //Double an enemy base plus the loop num, unless perfect
                if (timing == Timing.Perfect)
                    dmg = 0;
                director.DealDamage(Targetting.Player, dmg, note.Owner);
            }
        ),
        new Note(
            11,
            "PlayerMoney",
            GD.Load<Texture2D>("res://Classes/Notes/Assets/Note_PlayerMoney.png"),
            1,
            (director, note, timing) =>
            {
                if (timing == Timing.Miss)
                    return;
                StageProducer.PlayerStats.Money += note.GetBaseVal() * (int)timing;
            }
        ),
        new Note(
            12,
            "PlayerCombo",
            GD.Load<Texture2D>("res://Classes/Notes/Assets/Note_PlayerCombo.png"),
            1,
            (director, note, timing) =>
            {
                if (timing == Timing.Miss)
                    return;
                director.NPB.HandleTiming(
                    timing,
                    (ArrowType)StageProducer.GlobalRng.RandiRange(0, 3)
                );
            }
        ),
        new Note(
            13,
            "Parasifly",
            GD.Load<Texture2D>("res://Classes/Notes/Assets/Note_Parasifly.png"),
            1,
            (director, note, timing) =>
            {
                int amt = Math.Max((3 - (int)timing) * note.GetBaseVal(), 0);
                director.AddStatus(Targetting.All, StatusEffect.Block, amt);
            }
        ),
        new Note(
            14,
            "BossBlood",
            GD.Load<Texture2D>("res://Classes/Notes/Assets/Note_BossBlood.png"),
            2,
            (director, note, timing) =>
            {
                int dmg = (3 - (int)timing) * note.GetBaseVal();
                director.DealDamage(note, dmg, note.Owner);
                if (dmg > 0)
                    note.Owner.Heal((3 - (int)timing));
            },
            default,
            Targetting.Player
        ),
        new Note(
            15,
            "Spider",
            GD.Load<Texture2D>("res://Classes/Notes/Assets/Note_Spider.png"),
            1,
            (director, note, timing) =>
            {
                if (timing == Timing.Perfect)
                    return;
                int amt = Math.Max((3 - (int)timing) * note.GetBaseVal(), 1);
                director.AddStatus(Targetting.Player, StatusEffect.Poison, amt);
            }
        ),
        new Note(
            16,
            "LWS",
            GD.Load<Texture2D>("res://Classes/Notes/Assets/Note_LWS.png"),
            1,
            (director, note, timing) =>
            {
                int dmg = (3 - (int)timing) * note.GetBaseVal() + (TimeKeeper.LastBeat.Loop / 2);
                if (timing == Timing.Perfect)
                    dmg = 0;
                director.DealDamage(Targetting.Player, dmg, note.Owner);
            }
        ),
        new Note(
            17,
            "Mushroom",
            GD.Load<Texture2D>("res://Classes/Notes/Assets/Note_Mushroom.png"),
            2,
            (director, note, timing) =>
            {
                if (timing == Timing.Perfect)
                    return;
                int amt = Math.Max((3 - (int)timing) * note.GetBaseVal(), 1);
                director.AddStatus(Targetting.Player, StatusEffect.Poison, amt);
            }
        ),
    };

    public static readonly RelicTemplate[] RelicDictionary = new[]
    {
        new RelicTemplate(
            0,
            "Breakfast", //Reference ha ha, Item to give when relic pool is empty.
            Rarity.Breakfast,
            GD.Load<Texture2D>("res://Classes/Relics/Assets/Relic_Breakfast.png"),
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.OnPickup,
                    15,
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
            Rarity.Common,
            GD.Load<Texture2D>("res://Classes/Relics/Assets/Relic_PaperCut.png"),
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.OnLoop,
                    15,
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
            Rarity.Common,
            GD.Load<Texture2D>("res://Classes/Relics/Assets/Relic_Bandage.png"),
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.OnBattleEnd,
                    20,
                    (e, self, val) =>
                    {
                        e.BD.Player.Heal(val);
                    }
                ),
            }
        ),
        new RelicTemplate(
            8,
            "Medkit",
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
            Rarity.Epic,
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
        new RelicTemplate(
            10,
            "Loose Change",
            Rarity.Common,
            GD.Load<Texture2D>("res://Classes/Relics/Assets/Relic_LooseChange.png"),
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.OnBattleEnd,
                    10,
                    (e, self, val) =>
                    {
                        e.BD.BattleScore.IncRelicBonus(val);
                    }
                ),
            }
        ),
        new RelicTemplate(
            11,
            "Spiked Shield",
            Rarity.Rare,
            GD.Load<Texture2D>("res://Classes/Relics/Assets/Relic_SpikedShield.png"),
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.OnDamageInstance,
                    5,
                    (e, self, val) =>
                    {
                        if (
                            e is BattleDirector.Harbinger.OnDamageInstanceArgs dmgArgs
                            && dmgArgs.Dmg.Target == e.BD.Player
                            && dmgArgs.Dmg.Damage > 0
                            && e.BD.Player.HasStatus(StatusEffect.Block.CreateInstance())
                        )
                        {
                            e.BD.DealDamage(Targetting.First, val, null);
                        }
                    }
                ),
            }
        ),
        new RelicTemplate(
            12,
            "Lucky Dice",
            Rarity.Uncommon,
            GD.Load<Texture2D>("res://Classes/Relics/Assets/Relic_LuckyDice.png"),
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.OnPickup,
                    1,
                    (e, self, val) =>
                    {
                        StageProducer.PlayerStats.Rerolls = 1;
                    }
                ),
            }
        ),
        new RelicTemplate(
            13,
            "Shortcut",
            Rarity.Uncommon,
            GD.Load<Texture2D>("res://Classes/Relics/Assets/Relic_Shortcut.png"),
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.OnPickup,
                    1,
                    (e, self, val) =>
                    {
                        StageProducer.PlayerStats.Shortcuts += 1;
                    }
                ),
            }
        ),
        new RelicTemplate(
            14,
            "Second Pick",
            Rarity.Uncommon,
            GD.Load<Texture2D>("res://Classes/Relics/Assets/Relic_SecondPick.png"),
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.NotePlaced,
                    20,
                    (e, self, val) =>
                    {
                        if (StageProducer.GlobalRng.RandiRange(1, 100) <= val)
                            e.BD.NPB.IncreaseCharge(StageProducer.PlayerStats.MaxComboBar);
                    }
                ),
            }
        ),
        new RelicTemplate(
            15,
            "Broken Drumstick",
            Rarity.Uncommon,
            GD.Load<Texture2D>("res://Classes/Relics/Assets/Relic_BrokenDrumstick.png"),
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.OnBattleStart,
                    10,
                    (e, self, val) =>
                    {
                        //TODO: make damage scale with current act
                        e.BD.DealDamage(Targetting.All, val, e.BD.Player);
                    }
                ),
            }
        ),
        new RelicTemplate(
            16,
            "Blood Money",
            Rarity.Epic,
            GD.Load<Texture2D>("res://Classes/Relics/Assets/Relic_BloodMoney.png"),
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.OnDamageInstance,
                    5,
                    (e, self, val) =>
                    {
                        if (
                            e is BattleDirector.Harbinger.OnDamageInstanceArgs dmgArgs
                            && dmgArgs.Dmg.Target == e.BD.Player
                            && e.BD.Player.GetCurrentHealth()
                                < StageProducer.PlayerStats.MaxHealth / 2
                        )
                            StageProducer.PlayerStats.Money += val;
                    }
                ),
            }
        ),
        new RelicTemplate(
            17,
            "Coupon",
            Rarity.Common,
            GD.Load<Texture2D>("res://Classes/Relics/Assets/Relic_Coupon.png"),
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.OnPickup,
                    10,
                    (e, self, val) =>
                    {
                        StageProducer.PlayerStats.DiscountPercent += val;
                    }
                ),
            }
        ),
        new RelicTemplate(
            18,
            "War Horn",
            Rarity.Epic,
            GD.Load<Texture2D>("res://Classes/Relics/Assets/Relic_WarHorn.png"),
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.OnPickup,
                    1,
                    (e, self, val) =>
                    {
                        MapGrid.ForceEliteBattles = true;
                    }
                ),
            }
        ),
        new RelicTemplate(
            19,
            "Looter's Lens",
            Rarity.Uncommon,
            GD.Load<Texture2D>("res://Classes/Relics/Assets/Relic_LootersLens.png"),
            new RelicEffect[]
            {
                new RelicEffect(
                    BattleEffectTrigger.OnPickup,
                    1,
                    (e, self, val) =>
                    {
                        StageProducer.PlayerStats.RewardAmountModifier += val;
                    }
                ),
            }
        ),
    };

    private static string DefaultNoteChartPath = "Audio/songMaps/";

    public static readonly SongTemplate[] SongDictionary = new[] //Generalize and make pools for areas/room types
    {
        new SongTemplate(
            "Song1",
            [P_BossBlood.LoadPath],
            ResourceLoader.Load<NoteChart>(DefaultNoteChartPath + "BossBlood.tres")
        ),
        new SongTemplate(
            "Song2",
            [P_Parasifly.LoadPath],
            ResourceLoader.Load<NoteChart>(DefaultNoteChartPath + "ParasiflySingle.tres")
        ),
        new SongTemplate(
            "Song2",
            [P_Parasifly.LoadPath, P_Parasifly.LoadPath],
            ResourceLoader.Load<NoteChart>(DefaultNoteChartPath + "ParasiflyDouble.tres")
        ),
        new SongTemplate(
            "Song3",
            [P_TheGWS.LoadPath],
            ResourceLoader.Load<NoteChart>(DefaultNoteChartPath + "GWS.tres")
        ),
        new SongTemplate(
            "TutorialSong",
            [P_Strawman.LoadPath],
            ResourceLoader.Load<NoteChart>(DefaultNoteChartPath + "TutorialSong.tres")
        ),
        new SongTemplate(
            "YouWillDie:)",
            [P_Effigy.LoadPath],
            ResourceLoader.Load<NoteChart>(DefaultNoteChartPath + "TutorialBoss176_7.tres")
        ),
        new SongTemplate(
            "EcholaneSong",
            [P_Turtle.LoadPath],
            ResourceLoader.Load<NoteChart>(DefaultNoteChartPath + "EcholaneSong.tres")
        ),
        new SongTemplate(
            "CyberFoxSong",
            [P_CyberFox.LoadPath],
            ResourceLoader.Load<NoteChart>(DefaultNoteChartPath + "CyberFoxSong.tres")
        ),
        new SongTemplate(
            "GobblerSong",
            [P_Gobbler.LoadPath],
            ResourceLoader.Load<NoteChart>(DefaultNoteChartPath + "Gobbler.tres")
        ),
        new SongTemplate( //9
            "Holograeme",
            [P_Holograeme.LoadPath],
            ResourceLoader.Load<NoteChart>(DefaultNoteChartPath + "HoloRepeat.tres")
        ),
        new SongTemplate( //10
            "Shapes",
            [P_Shapes.LoadPath],
            ResourceLoader.Load<NoteChart>(DefaultNoteChartPath + "Shapes.tres")
        ),
        new SongTemplate( //11
            "Spideer",
            [P_Spider.LoadPath, P_Spider.LoadPath],
            ResourceLoader.Load<NoteChart>(DefaultNoteChartPath + "Spider.tres")
        ),
        new SongTemplate( //12
            "Squirkel",
            [P_Squirkel.LoadPath],
            ResourceLoader.Load<NoteChart>(DefaultNoteChartPath + "SquirkelSong.tres")
        ),
        new SongTemplate( //13
            "Mushroom",
            [P_Mushroom.LoadPath],
            ResourceLoader.Load<NoteChart>(DefaultNoteChartPath + "Mushroom.tres")
        ),
        new SongTemplate(
            "Keythulu",
            [P_Keythulu.LoadPath],
            ResourceLoader.Load<NoteChart>(DefaultNoteChartPath + "KeythuluSong.tres")
        ),
        new SongTemplate( // 15
            name: "LWS",
            enemyScenePath: [P_LWS.LoadPath],
            ResourceLoader.Load<NoteChart>(DefaultNoteChartPath + "FrostWaltz.tres")
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
