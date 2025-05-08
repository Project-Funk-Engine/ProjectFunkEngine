using System;
using FunkEngine;
using Godot;

public partial class AreaBasedBackground : TextureRect
{
    public override void _Ready()
    {
        if (StageProducer.CurLevel == null)
            Texture = GD.Load<Texture2D>("res://SharedAssets/BackGround_Full.png");
        else
            Texture = GD.Load<Texture2D>(StageProducer.CurLevel.BackgroundPath);
    }
}
