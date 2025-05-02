using System;
using FunkEngine;
using Godot;

public partial class AreaBasedBackground : TextureRect
{
    public override void _Ready()
    {
        Texture = GD.Load<Texture2D>(StageProducer.CurLevel.BackgroundPath);
    }
}
