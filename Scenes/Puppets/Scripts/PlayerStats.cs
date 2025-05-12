using System;
using System.Linq;
using FunkEngine;
using Godot;

public partial class PlayerStats : Resource
{
    public int Money = 0;

    public int MaxHealth = 100;
    public int CurrentHealth = 100;
    public int MaxComboBar = 60;
    public int MaxComboMult = 25;
    public int NotesToIncreaseCombo = 4;
    public int RewardAmountModifier = 0;
    public int Rerolls = 0;
    public int Shortcuts = 0;

    //Array in order of descending rarities, Legendary -> ... Common. Int odds out of 100.
    public int[] RarityOdds = [1, 5, 10, 20, 100];
    public Note[] CurNotes = new Note[]
    {
        Scribe.NoteDictionary[1].Clone(),
        Scribe.NoteDictionary[1].Clone(),
        Scribe.NoteDictionary[2].Clone(),
        Scribe.NoteDictionary[3].Clone(),
    };

    public RelicTemplate[] CurRelics = Array.Empty<RelicTemplate>();
    public int Attack = 1;

    public void AddRelic(RelicTemplate relic)
    {
        foreach (RelicEffect effect in relic.Effects)
        {
            if (effect.GetTrigger() == BattleEffectTrigger.OnPickup)
            {
                effect.OnTrigger(null);
            }
        }
        CurRelics = CurRelics.Append(relic).ToArray();
        Scribe.RemoveRelicFromPool(relic);
    }

    public void AddNote(Note nSelection)
    {
        //If the note is vampire, check to see if we already have 2 of them
        if (
            nSelection.Name == "PlayerVampire"
            && CurNotes.Count(note => note.Name == "PlayerVampire") >= 2
        )
        {
            SteamWhisperer.PopAchievement("vampire");
        }

        CurNotes = CurNotes.Append(nSelection).ToArray();
    }

    public void RemoveNote(Note nSelection)
    {
        CurNotes = CurNotes.Where(n => n != nSelection).ToArray();
    }
}
