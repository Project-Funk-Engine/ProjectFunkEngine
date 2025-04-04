using System;
using System.Linq;
using FunkEngine;
using Godot;

public partial class PlayerStats : Resource
{
    public int MaxHealth = 100;
    public int CurrentHealth = 100;
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
    }

    public void AddNote(Note nSelection)
    {
        CurNotes = CurNotes.Append(nSelection).ToArray();
    }
}
