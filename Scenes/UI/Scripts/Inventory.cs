using FunkEngine;
using Godot;

public partial class Inventory : Control, IFocusableMenu
{
    public static readonly string LoadPath = "res://Scenes/UI/Inventory.tscn";

    [Export]
    private GridContainer _relics;

    [Export]
    private GridContainer _notes;

    [Export]
    private Label _description;

    [Export]
    private TabContainer _tabs;

    [Export]
    private Label _moneyLabel;

    public IFocusableMenu Prev { get; set; }
    private static readonly string[] TabNames = new[] { "NOTE", "RELIC" };

    private void Display(PlayerStats playerStats)
    {
        AddDisplayButtons(playerStats.CurRelics, _relics);
        AddDisplayButtons(playerStats.CurNotes, _notes);

        _moneyLabel.Text = playerStats.Money.ToString();

        _tabs.TabChanged += ClearDescription;
    }

    private void AddDisplayButtons(IDisplayable[] displayables, Node parentNode)
    {
        foreach (IDisplayable item in displayables)
        {
            var newButton = GD.Load<PackedScene>(DisplayButton.LoadPath)
                .Instantiate<DisplayButton>();
            newButton.Display(item.Texture, item.Name, true);
            newButton.ThemeTypeVariation = "nopress";
            newButton.Pressed += () =>
            {
                DoDescription(newButton);
            };
            parentNode.AddChild(newButton);
        }
    }

    public override void _Process(double delta)
    {
        NoNullFocus();
    }

    public override void _Input(InputEvent @event)
    {
        if (ControlSettings.IsOutOfFocus(this))
        {
            GetViewport().SetInputAsHandled();
            return;
        }
        if (_tabs.CurrentTab == 0) //Godot 4.4 changed neighbor behaviour
        {
            if (_notes.GetChildCount() > 0)
                _tabs.GetTabBar().FocusNeighborBottom = _notes.GetChild(0).GetPath();
            else
                _tabs.GetTabBar().FocusNeighborBottom = null;
        }
        else if (_tabs.CurrentTab == 1)
        {
            if (_relics.GetChildCount() > 0)
                _tabs.GetTabBar().FocusNeighborBottom = _relics.GetChild(0).GetPath();
            else
                _tabs.GetTabBar().FocusNeighborBottom = null;
        }

        if (
            @event.IsActionPressed("ui_cancel")
            || @event.IsActionPressed("WASD_inventory")
            || @event.IsActionPressed("CONTROLLER_inventory")
        )
        {
            ReturnToPrev();
            GetViewport().SetInputAsHandled();
        }
    }

    public void ResumeFocus()
    {
        ProcessMode = ProcessModeEnum.Pausable;
        _tabs.GetTabBar().GrabFocus();
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
        _tabs.GetTabBar().GrabFocus();
    }

    public void ReturnToPrev()
    {
        Prev.ResumeFocus();
        QueueFree();
    }

    private void NoNullFocus()
    {
        var focusedNode = GetViewport().GuiGetFocusOwner();
        if (focusedNode != null)
            return;
        _tabs.GetTabBar().GrabFocus();
        ClearDescription(-1);
    }

    private void DoDescription(DisplayButton dispButton)
    {
        string itemName = dispButton.DisplayName.ToUpper();
        itemName = itemName.Replace(" ", "");
        _description.Text =
            Tr(TabNames[_tabs.CurrentTab] + "_" + itemName + "_NAME")
            + ": "
            + Tr(TabNames[_tabs.CurrentTab] + "_" + itemName + "_TOOLTIP");
    }

    private void ClearDescription(long newTab)
    {
        _description.Text = "";
    }
}
