using System;
using System.Collections.Generic;
using System.Linq;
using FunkEngine;
using Godot;

public partial class ShopScene : Control
{
    public static readonly string LoadPath = "res://Scenes/ShopScene/ShopScene.tscn";

    [Export]
    private Label _moneyLabel;

    [Export]
    private Button _exitButton;

    [Export]
    private Button _removalButton;

    [Export]
    private GridContainer _noteGrid;

    [Export]
    private GridContainer _relicGrid;

    [Export]
    private CenterContainer _confirmationPopup;

    [Export]
    private Button _confirmationButton;

    [Export]
    private Button _denyButton;

    [Export]
    private Label _descriptionLabel;

    [Export]
    private Control _removalPanel;

    [Export]
    private GridContainer _possessionGrid;

    [Export]
    private Button _removalAcceptButton;

    [Export]
    private Button _cancelRemoveButton;

    [Export]
    private Label _removalCostLabel;

    private ButtonGroup _bGroup;

    private readonly int[] _priceByRarity = [100, 90, 80, 70, 60, 50, 9];
    const int NoteCost = 45;

    public override void _Ready()
    {
        _bGroup = new ButtonGroup();
        Initialize();
        _confirmationButton.Pressed += TryPurchase;
        _denyButton.Pressed += CloseConfirmationPopup;
        _removalButton.Pressed += OpenRemovalPane;
        _removalAcceptButton.Pressed += RemoveNote;
        _cancelRemoveButton.Pressed += CloseRemovalPane;
    }

    public override void _EnterTree()
    {
        BgAudioPlayer.LiveInstance.PlayLevelMusic();
    }

    private void Initialize()
    {
        UpdateMoneyLabel();
        GenerateShopItems();
        PopulatePossessedNotes();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            if (_confirmationPopup.Visible)
            {
                CloseConfirmationPopup();
                GetViewport().SetInputAsHandled();
            }
            else if (_removalPanel.Visible)
            {
                CloseRemovalPane();
                GetViewport().SetInputAsHandled();
            }
        }
    }

    private void UpdateMoneyLabel()
    {
        _moneyLabel.Text = StageProducer.PlayerStats.Money.ToString();
    }

    private const int RelicOptions = 3;
    private const int NoteOptions = 5;

    private void GenerateShopItems()
    {
        var relics = Scribe.GetRandomRelics(
            RelicOptions,
            StageProducer.CurRoom + 10,
            StageProducer.PlayerStats.RarityOdds
        );

        var notes = Scribe.GetRandomRewardNotes(NoteOptions, StageProducer.CurRoom + 10);

        foreach (var relic in relics)
        {
            int price = _priceByRarity[(int)relic.Rarity];
            AddShopItem(_relicGrid, relic, price);
        }

        foreach (var note in notes)
        {
            int price = NoteCost;
            AddShopItem(_noteGrid, note, price);
        }
    }

    private void AddShopItem(GridContainer container, IDisplayable item, int price)
    {
        if (container == null || item == null)
        {
            GD.PushError("AddShopItem called with null!");
            return;
        }
        price = Math.Max(price, 0); //Price can't go negative.
        ShopItem newItem = GD.Load<PackedScene>(ShopItem.LoadPath).Instantiate<ShopItem>();
        newItem.Display(price, item.Texture, item.Name);
        newItem.DisplayButton.Pressed += () => SetPurchasable(item, newItem);
        newItem.DisplayButton.SetButtonGroup(_bGroup);
        newItem.DisplayButton.ToggleMode = true;
        newItem.DisplayButton.FocusEntered += () => ChangeDescription(item);
        container.AddChild(newItem);
    }

    private IDisplayable _currentItem;
    private ShopItem _currentUItem;

    private void SetPurchasable(IDisplayable item, ShopItem uItem)
    {
        if (item == null || uItem == null)
            return;
        _currentItem = item;
        _currentUItem = uItem;
        _confirmationButton.Disabled = StageProducer.PlayerStats.Money < _currentUItem.Price;
        OpenConfirmationPopup();
    }

    private void TryPurchase()
    {
        if (StageProducer.PlayerStats.Money < _currentUItem.Price)
            return;

        StageProducer.PlayerStats.Money -= _currentUItem.Price;
        switch (_currentItem)
        {
            case Note note:
                StageProducer.PlayerStats.AddNote(note);
                AddNoteToPossessions(note);
                break;
            case RelicTemplate relic:
                StageProducer.PlayerStats.AddRelic(relic);
                break;
        }

        CloseConfirmationPopup();

        GetViewport().GuiGetFocusOwner().FindNextValidFocus().GrabFocus(); //slightly hacky
        _currentUItem.Visible = false;
        _currentUItem.QueueFree();
        UpdateMoneyLabel();

        _currentItem = null;
        _currentUItem = null;
    }

    private Control _lastFocused;

    private void OpenConfirmationPopup()
    {
        _confirmationPopup.Visible = true;
        _lastFocused = GetViewport().GuiGetFocusOwner();
        _denyButton.GrabFocus();
    }

    private void CloseConfirmationPopup()
    {
        _confirmationPopup.Visible = false;
        _lastFocused.GrabFocus();
        _lastFocused = null;
        _bGroup.GetPressedButton().SetPressed(false);
    }

    private void ChangeDescription(IDisplayable displayable)
    {
        if (displayable == null)
        {
            _descriptionLabel.Text = "";
            return;
        }
        string name = displayable.Name.ToUpper();
        name = name.Replace(" ", "");
        string type = displayable switch
        {
            Note => "NOTE_",
            RelicTemplate => "RELIC_",
            _ => "UNKNOWN_",
        };
        _descriptionLabel.Text = Tr(type + name + "_NAME") + ": " + Tr(type + name + "_TOOLTIP");
    }

    private const int RemovalCost = 50;
    private bool _hasRemoved;

    private void OpenRemovalPane()
    {
        if (_hasRemoved)
            return;
        _removalCostLabel.Text = RemovalCost.ToString();
        _removalButton.Disabled = true;
        _exitButton.Disabled = true;
        _removalPanel.Visible = true;
        _removalAcceptButton.Visible = false;
        _removalAcceptButton.Disabled = StageProducer.PlayerStats.Money < RemovalCost;
        _bGroup.GetPressedButton()?.SetPressed(false);
        _noteGrid.Visible = false;
        _relicGrid.Visible = false;
        _cancelRemoveButton.GrabFocus();
    }

    private void CloseRemovalPane()
    {
        _removalButton.Disabled = _hasRemoved;
        _exitButton.Disabled = false;
        _removalPanel.Visible = false;
        _removalButton.GrabFocus();
        _toRemove = null;
        _selectedRemoveButton = null;
        _noteGrid.Visible = true;
        _relicGrid.Visible = true;
        ChangeDescription(null);
    }

    private void PopulatePossessedNotes()
    {
        foreach (var note in StageProducer.PlayerStats.CurNotes)
        {
            AddNoteToPossessions(note);
        }
    }

    private void AddNoteToPossessions(Note note)
    {
        if (note == null)
            return;
        DisplayButton disButton = GD.Load<PackedScene>(DisplayButton.LoadPath)
            .Instantiate<DisplayButton>();
        disButton.Display(note.Texture, note.Name);
        disButton.ToggleMode = true;
        disButton.FocusEntered += () => ChangeDescription(note);
        disButton.Pressed += () => RemovalSelection(note, disButton);
        disButton.ButtonGroup = _bGroup;
        _possessionGrid.AddChild(disButton);
    }

    private Note _toRemove;
    private Button _selectedRemoveButton;

    private void RemovalSelection(Note note, Button button)
    {
        _toRemove = note;
        _selectedRemoveButton = button;
        _removalAcceptButton.Visible = true;
        button.SetPressed(true);
    }

    private void RemoveNote()
    {
        if (_toRemove == null || _selectedRemoveButton == null)
            return;
        StageProducer.PlayerStats.Money -= RemovalCost;
        _removalButton.Disabled = true;
        _hasRemoved = true;
        StageProducer.PlayerStats.RemoveNote(_toRemove);
        _selectedRemoveButton.QueueFree();
        CloseRemovalPane();
    }
}
