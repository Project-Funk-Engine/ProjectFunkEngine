using System;
using FunkEngine;
using Godot;

public partial class Inventory : Control, IFocusableMenu
{
    public static readonly string LoadPath = "res://Scenes/UI/Inventory.tscn";

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
        AddDisplayButtons(playerStats.CurRelics, Relics);
        AddDisplayButtons(playerStats.CurNotes, Notes);

        Tabs.TabChanged += ClearDescription;
    }

    private void AddDisplayButtons(IDisplayable[] displayables, Node parentNode)
    {
        foreach (IDisplayable item in displayables)
        {
            var newButton = GD.Load<PackedScene>(DisplayButton.LoadPath)
                .Instantiate<DisplayButton>();
            newButton.Display(item.Texture, item.Tooltip, item.Name);
            newButton.Pressed += () =>
            {
                DoDescription(newButton);
            };
            parentNode.AddChild(newButton);
        }
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
