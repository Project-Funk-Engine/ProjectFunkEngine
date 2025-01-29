using Godot;
using System;

public partial class ChartBg : TextureRect
{
	[Export] public float Bounds = 700f;
	[Export] public float Speed = 5;


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Position.X <= -Bounds)
		{
			Position = new Vector2(Bounds, Position.Y);
		}
		Position += Speed * Vector2.Left;

	}
}
