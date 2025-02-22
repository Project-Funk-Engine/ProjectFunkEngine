using System;
using Godot;

public partial class MenuModule : CanvasLayer
{
    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("Pause"))
        {
            var pauseMenu = GD.Load<PackedScene>("res://scenes/UI/Pause.tscn");
            AddChild(pauseMenu.Instantiate());
            GetTree().Paused = true;
        }
        if (@event.IsActionPressed("Inventory"))
        {
            var invenMenu = GD.Load<PackedScene>("res://scenes/UI/inventory.tscn")
                .Instantiate<Inventory>();
            AddChild(invenMenu);
            invenMenu.Display(StageProducer.PlayerStats ?? new PlayerStats()); //For now work around for testing
            GetTree().Paused = true;
        }
    }
}
