using System;
using System.Linq;
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
    }; //TODO: Get a better method than .Clone

    public RelicTemplate[] CurRelics = Array.Empty<RelicTemplate>();
    public int Attack = 1;

    public void AddRelic(RelicTemplate relic)
    {
        if (CurRelics.Any(r => r.Name == relic.Name))
        {
            GD.PrintErr("Relic already in inventory: " + relic.Name);
            return;
        }
        CurRelics = CurRelics.Append(relic).ToArray();
        GD.Print("Adding relic: " + relic.Name);
    }
}
