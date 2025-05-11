using System;
using System.Collections.Generic;
using System.Linq;
using FunkEngine;
using Godot;

public partial class ShopUI : CanvasLayer
{
    public static readonly string LoadPath = "res://Scenes/ShopScene/ShopScene.tscn";

    [Export]
    public Control ShopHubMenu;

    [Export]
    public Control RelicMenu;

    [Export]
    public Control NoteMenu;

    [Export]
    public Control PossessionsMenu;

    [Export]
    public Button RelicTabButton;

    [Export]
    public Button NoteTabButton;

    [Export]
    public Button PossessionsTabButton;

    [Export]
    public Button RelicBackButton;

    [Export]
    public Button NoteBackButton;

    [Export]
    public Button PossessionsBackButton;

    [Export]
    public VBoxContainer RelicContainer;

    [Export]
    public VBoxContainer NoteContainer;

    [Export]
    public VBoxContainer PossessionsContainer;

    [Export]
    public Label MoneyLabel;

    [Export]
    public Button ExitButton;

    private PlayerPuppet _player;

    private Dictionary<IDisplayable, int> _itemPrices = new();
    private Dictionary<IDisplayable, HBoxContainer> _shopItemUI = new();

    public override void _Ready()
    {
        _player = GD.Load<PackedScene>(PlayerPuppet.LoadPath).Instantiate<PlayerPuppet>();
        AddChild(_player);

        RelicTabButton.Pressed += () => ShowMenu(RelicMenu);
        NoteTabButton.Pressed += () => ShowMenu(NoteMenu);
        PossessionsTabButton.Pressed += () => ShowMenu(PossessionsMenu);

        RelicBackButton.Pressed += () => ShowMenu(ShopHubMenu);
        NoteBackButton.Pressed += () => ShowMenu(ShopHubMenu);
        PossessionsBackButton.Pressed += () => ShowMenu(ShopHubMenu);

        ShowMenu(ShopHubMenu);

        Initialize();
    }

    public void Initialize()
    {
        _player.Stats.Money = 5000;
        UpdateMoneyLabel();
        GenerateShopItems();
        PopulatePossessedNotes();
        ExitButton.Pressed += ReturnToMap;
    }

    private void UpdateMoneyLabel()
    {
        MoneyLabel.Text = $"Money: {_player.Stats.Money}";
    }

    private void GenerateShopItems()
    {
        var ownedRelicNames = _player.Stats.CurRelics.Select(r => r.Name).ToHashSet();
        var relics = Scribe
            .GetRandomRelics(2, StageProducer.CurRoom + 10, _player.Stats.RarityOdds)
            .Where(r => !ownedRelicNames.Contains(r.Name))
            .Take(2)
            .ToList();

        var ownedNoteNames = _player.Stats.CurNotes.Select(n => n.Name).ToHashSet();
        var notes = Scribe
            .GetRandomRewardNotes(3, StageProducer.CurRoom + 10)
            .Where(n => !ownedNoteNames.Contains(n.Name))
            .Take(3)
            .ToList();

        foreach (var relic in relics)
        {
            int price = GD.RandRange(80, 150);
            _itemPrices[relic] = price;
            AddShopItem(RelicContainer, relic, price);
        }

        foreach (var note in notes)
        {
            int price = GD.RandRange(40, 100);
            _itemPrices[note] = price;
            AddShopItem(NoteContainer, note, price);
        }
    }

    private void AddShopItem(VBoxContainer container, IDisplayable item, int price)
    {
        var hbox = new HBoxContainer();

        var icon = new TextureRect
        {
            Texture = item.Texture,
            CustomMinimumSize = new Vector2(32, 32),
        };
        var label = new Label { Text = $"{item.Name} \n ${price}" };
        var buyButton = new Button { Text = "Buy" };

        buyButton.Pressed += () => TryPurchase(item);

        hbox.AddChild(icon);
        hbox.AddChild(label);
        hbox.AddChild(buyButton);
        container.AddChild(hbox);

        _itemPrices[item] = price;
        _shopItemUI[item] = hbox;
    }

    private void TryPurchase(IDisplayable item)
    {
        if (!_itemPrices.ContainsKey(item))
            return;
        int cost = _itemPrices[item];

        if (_player.Stats.Money >= cost)
        {
            _player.Stats.Money -= cost;
            switch (item)
            {
                case Note note:
                    _player.Stats.AddNote(note);
                    AddNoteToPossessions(note);
                    break;
                case RelicTemplate relic:
                    _player.Stats.AddRelic(relic);
                    break;
            }

            UpdateMoneyLabel();
            if (_shopItemUI.TryGetValue(item, out var hbox))
            {
                foreach (var child in hbox.GetChildren())
                {
                    if (child is Label label)
                    {
                        label.Modulate = new Color(0.5f, 0.5f, 0.5f);
                    }
                    else if (child is Button button)
                    {
                        button.Disabled = true;
                    }
                }
            }
        }
        else
        {
            GD.Print("Not enough money!");
        }
    }

    private void PopulatePossessedNotes()
    {
        foreach (var note in _player.Stats.CurNotes)
        {
            var hbox = new HBoxContainer();

            var icon = new TextureRect
            {
                Texture = note.Texture,
                CustomMinimumSize = new Vector2(32, 32),
            };
            var label = new Label { Text = note.Name };
            var removeButton = new Button { Text = "Remove" };
            var capturedNote = note;

            removeButton.Pressed += () => RemoveNote(capturedNote, hbox);

            hbox.AddChild(icon);
            hbox.AddChild(label);
            hbox.AddChild(removeButton);
            PossessionsContainer.AddChild(hbox);
        }
    }

    private void AddNoteToPossessions(Note note)
    {
        var hbox = new HBoxContainer();

        var icon = new TextureRect
        {
            Texture = note.Texture,
            CustomMinimumSize = new Vector2(32, 32),
        };
        var label = new Label { Text = note.Name };
        var removeButton = new Button { Text = "Remove" };
        var capturedNote = note;

        removeButton.Pressed += () => RemoveNote(capturedNote, hbox);

        hbox.AddChild(icon);
        hbox.AddChild(label);
        hbox.AddChild(removeButton);
        PossessionsContainer.AddChild(hbox);
    }

    private void RemoveNote(Note note, HBoxContainer hbox)
    {
        _player.Stats.CurNotes = _player.Stats.CurNotes.Where(n => n != note).ToArray();
        hbox.QueueFree();
    }

    private void ShowMenu(Control menuToShow)
    {
        ShopHubMenu.Visible = false;
        RelicMenu.Visible = false;
        NoteMenu.Visible = false;
        PossessionsMenu.Visible = false;

        menuToShow.Visible = true;
    }

    private void ReturnToMap()
    {
        StageProducer.LiveInstance.TransitionStage(Stages.Map);
    }
}
