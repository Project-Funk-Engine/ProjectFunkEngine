using System;
using FunkEngine;
using Godot;

public partial class AreaBasedBackground : TextureRect
{
    public override void _Ready()
    {
        Texture = StageProducer.CurArea switch
        {
            Area.Forest => GD.Load<Texture2D>("res://SharedAssets/BackGround_Full.png"),
            Area.City => GD.Load<Texture2D>("res://icon.svg"),
            _ => null,
        };
    }
}
