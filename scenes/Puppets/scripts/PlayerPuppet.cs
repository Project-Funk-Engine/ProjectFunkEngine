using System;
using Godot;

public partial class PlayerPuppet : PuppetTemplate
{
    public PlayerStats Stats = new PlayerStats();

    public override void _Ready()
    {
        base._Ready();
        Init(GD.Load<Texture2D>("res://scenes/BattleDirector/assets/Character1.png"), "Player");
        SetPosition(new Vector2(80, 0));
        Sprite.Position += Vector2.Down * 30; //TEMP
    }
}
