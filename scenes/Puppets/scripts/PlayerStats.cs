using System;
using Godot;

public partial class PlayerStats : Resource
{
    public int MaxHealth = 300;
    public Note[] CurNotes = new Note[] { Scribe.NoteDictionary[1].Clone() };

    public RelicTemplate[] CurRelics = new[] { Scribe.RelicDictionary[0].Clone() };
    public int Attack = 1;
}
