using System;
using Godot;
using static NoteManager;

public partial class Main : Node2D
{
    private NoteManager _noteManager;

    public override void _Ready()
    {
        _noteManager = GetNode<NoteManager>("noteManager");
        _noteManager.CreateNote(ArrowType.Down);
    }

    public override void _Process(double delta)
    {
        //have a tiny chance of spanning a note in a row every frame. For testing purposes.
        if (GD.Randf() >= 0.02f)
            return;
        var randomLane = (ArrowType)
            GD.RandRange(0, ArrowType.GetValues(typeof(ArrowType)).Length - 1);
        _noteManager.CreateNote(randomLane);
    }
}
