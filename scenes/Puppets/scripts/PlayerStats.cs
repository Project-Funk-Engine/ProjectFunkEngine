using System;
using Godot;

public partial class PlayerStats : Resource
{
    public int MaxHealth = 300;
    public Note[] CurNotes = new Note[5];

    public RelicTemplate[] CurRelics = new[] { RelicDict.RelicPool[0] };
    public int Attack = 1;
}
