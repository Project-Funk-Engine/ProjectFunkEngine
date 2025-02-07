using System;
using Godot;

public partial class PlayerPuppet : PuppetTemplate
{
    public PlayerStats Stats = new PlayerStats();

    public override void _Ready()
    {
        base._Ready();
    }
}
