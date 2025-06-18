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
    private Button _healButton;

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

    [Export]
    private PlayerPuppet _player;

    private ButtonGroup _bGroup;

    private readonly int[] _priceByRarity = [200, 180, 160, 140, 120, 100, 18];
    const int NoteCost = 90;

    private List<ShopItem> _shopItems = new List<ShopItem>();

    public override void _EnterTree()
    {
        BgAudioPlayer.LiveInstance.ResumeLevelMusic();
        _bGroup = new ButtonGroup();
        Initialize();
        _confirmationButton.Pressed += TryPurchase;
        _denyButton.Pressed += CloseConfirmationPopup;
        _removalButton.Pressed += OpenRemovalPane;
        _removalAcceptButton.Pressed += RemoveNote;
        _cancelRemoveButton.Pressed += CloseRemovalPane;
        _healButton.Pressed += TryHeal;
    }

    private void Initialize()
    {
        UpdateMoneyLabel();
        GenerateShopItems();
        PopulatePossessedNotes();
        UpdateHealButton();
    }

    public override void _Input(InputEvent @event)
    {
        if (GetViewport().GuiGetFocusOwner() == null)
        {
            _exitButton.GrabFocus();
        }
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
        if (@event is InputEventKey eventKey && eventKey.Pressed && !eventKey.Echo)
        {
            if (eventKey.Keycode == Key.Key0)
            {
                StageProducer.PlayerStats.Money += 999;
                UpdateMoneyLabel();
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
        List<RelicTemplate> _shopRelics = Scribe
            .GetRandomRelics(
                RelicOptions,
                StageProducer.CurRoom + 10,
                StageProducer.PlayerStats.RarityOdds
            )
            .ToList();

        List<Note> _shopNotes = Scribe
            .GetRandomRewardNotes(NoteOptions, StageProducer.CurRoom + 10)
            .ToList();

        foreach (var relic in _shopRelics)
        {
            int price = _priceByRarity[(int)relic.Rarity];
            AddShopItem(_relicGrid, relic, price);
        }

        foreach (var note in _shopNotes)
        {
            int price = NoteCost;
            AddShopItem(_noteGrid, note, price);
        }
    }

    private void RefreshShopPrices()
    {
        foreach (ShopItem sItem in _shopItems)
        {
            sItem.UpdateCost(GetPrice(sItem.BaseCost));
        }
    }

    private void AddShopItem(GridContainer container, IDisplayable item, int basePrice)
    {
        if (container == null || item == null)
        {
            GD.PushError("AddShopItem called with null!");
            return;
        }
        int price = GetPrice(basePrice);
        ShopItem newItem = GD.Load<PackedScene>(ShopItem.LoadPath).Instantiate<ShopItem>();
        newItem.Display(basePrice, price, item.Texture, item.Name);
        newItem.DisplayButton.Pressed += () => SetPurchasable(item, newItem);
        newItem.DisplayButton.SetButtonGroup(_bGroup);
        newItem.DisplayButton.ToggleMode = true;
        newItem.DisplayButton.FocusEntered += () => ChangeDescription(item);
        _shopItems.Add(newItem);
        container.AddChild(newItem);
    }

    private int GetPrice(int basePrice)
    {
        return (int)
            Math.Max(basePrice * (1 - (float)StageProducer.PlayerStats.DiscountPercent / 100), 0); //Price can't go negative
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

        _shopItems.Remove(_currentUItem);
        CloseConfirmationPopup();

        GetViewport().GuiGetFocusOwner().FindNextValidFocus().GrabFocus(); //slightly hacky
        _currentUItem.Visible = false;
        _currentUItem.QueueFree();
        UpdateMoneyLabel();

        _currentItem = null;
        _currentUItem = null;

        RefreshShopPrices();
        UpdateHealButton();
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

    private const int RemovalCost = 75;
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
        StageProducer.PlayerStats.RemoveNote(
            Array.IndexOf(StageProducer.PlayerStats.CurNotes, _toRemove)
        );
        _selectedRemoveButton.QueueFree();
        CloseRemovalPane();
        UpdateMoneyLabel();
        UpdateHealButton();
    }

    private bool _hasHealed;
    private const int HealCost = 50;
    private int _healAmount = (StageProducer.PlayerStats.MaxHealth / 4);

    private void UpdateHealButton()
    {
        _healButton.Disabled =
            StageProducer.PlayerStats.Money < HealCost
            || StageProducer.PlayerStats.CurrentHealth == StageProducer.PlayerStats.MaxHealth
            || _hasHealed;
    }

    private void TryHeal()
    {
        if (
            StageProducer.PlayerStats.Money < HealCost
            || StageProducer.PlayerStats.CurrentHealth == StageProducer.PlayerStats.MaxHealth
            || _hasHealed
        )
        {
            return;
        }

        StageProducer.PlayerStats.Money -= HealCost;
        _hasHealed = true;
        _player.Heal(_healAmount);
        UpdateHealButton();
        UpdateMoneyLabel();
    }
}
