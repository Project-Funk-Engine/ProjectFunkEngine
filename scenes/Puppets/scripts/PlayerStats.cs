using System;
using Godot;

public partial class PlayerStats : Resource
{
    public int MaxHealth = 300;
    public Note[] CurNotes = new Note[] { new Note(null) };

    public RelicTemplate[] CurRelics = new[] { RelicDict.RelicPool[0] };
    public int Attack = 1;
}
