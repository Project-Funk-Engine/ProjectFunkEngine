using System;
using Godot;

public partial class Inventory : Control
{
    [Export]
    private GridContainer Relics;

    [Export]
    private GridContainer Notes;

    [Export]
    private Label Description;

    [Export]
    private TabContainer Tabs;

    private static readonly string[] TabNames = new[] { "NOTE", "RELIC" };

    public void Display(PlayerStats playerStats)
    {
        foreach (RelicTemplate relic in playerStats.CurRelics)
        {
            var newButton = GD.Load<PackedScene>("res://scenes/UI/display_button.tscn")
                .Instantiate<DisplayButton>();
            newButton.Display(relic.Texture, relic.Tooltip, relic.Name);
            newButton.Pressed += () =>
            {
                DoDescription(newButton);
            };
            Relics.AddChild(newButton);
        }
        foreach (Note note in playerStats.CurNotes)
        {
            var newButton = GD.Load<PackedScene>("res://scenes/UI/display_button.tscn")
                .Instantiate<DisplayButton>();
            newButton.Display(note.Texture, note.Tooltip, note.Name);
            newButton.Pressed += () =>
            {
                DoDescription(newButton);
            };
            Notes.AddChild(newButton);
        }

        Tabs.TabChanged += ClearDescription;
    }

    public override void _Ready()
    {
        Tabs.GetTabBar().GrabFocus();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("Pause") || @event.IsActionPressed("Inventory"))
        {
            Resume();
            GetViewport().SetInputAsHandled();
        }
    }

    private void Resume()
    {
        GetTree().Paused = false;
        QueueFree(); //Hacky and shortsighted (probably?)
    }

    private void DoDescription(DisplayButton dispButton)
    {
        string itemName = dispButton.DisplayName.ToUpper();
        itemName = itemName.Replace(" ", "");
        Description.Text =
            Tr(TabNames[Tabs.CurrentTab] + "_" + itemName + "_NAME")
            + ": "
            + Tr(TabNames[Tabs.CurrentTab] + "_" + itemName + "_TOOLTIP");
    }

    private void ClearDescription(long newTab)
    {
        Description.Text = "";
    }
}
