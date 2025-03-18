using System;
using FunkEngine;
using Godot;

public partial class Inventory : Control, IFocusableMenu
{
    [Export]
    private GridContainer Relics;

    [Export]
    private GridContainer Notes;

    [Export]
    private Label Description;

    [Export]
    private TabContainer Tabs;

    public IFocusableMenu Prev { get; set; }
    private static readonly string[] TabNames = new[] { "NOTE", "RELIC" };

    private void Display(PlayerStats playerStats)
    {
        foreach (RelicTemplate relic in playerStats.CurRelics)
        {
            var newButton = GD.Load<PackedScene>("res://Scenes/UI/DisplayButton.tscn")
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
            var newButton = GD.Load<PackedScene>("res://Scenes/UI/DisplayButton.tscn")
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

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel") || @event.IsActionPressed("Inventory"))
        {
            ReturnToPrev();
            GetViewport().SetInputAsHandled();
        }
    }

    public void ResumeFocus()
    {
        ProcessMode = ProcessModeEnum.Pausable;
        Tabs.GetTabBar().GrabFocus();
    }

    public void PauseFocus()
    {
        ProcessMode = ProcessModeEnum.Disabled;
    }

    public void OpenMenu(IFocusableMenu prev)
    {
        Display(StageProducer.PlayerStats ?? new PlayerStats());
        Prev = prev;
        Prev.PauseFocus();
        Tabs.GetTabBar().GrabFocus();
    }

    public void ReturnToPrev()
    {
        Prev.ResumeFocus();
        QueueFree();
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
