using System;
using FunkEngine;
using Godot;

public partial class P_Mushroom : EnemyPuppet
{
    public static new readonly string LoadPath =
        "res://Scenes/Puppets/Enemies/Mushroom/Mushroom.tscn";

    public override void _Ready()
    {
        MaxHealth = 200;
        CurrentHealth = MaxHealth;
        BaseMoney = 10;
        InitialNote = (14, 3);
        base._Ready();
    }
}
