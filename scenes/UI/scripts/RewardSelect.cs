using System;
using System.Linq;
using Godot;

public partial class RewardSelect : CanvasLayer
{
    [Export]
    public HBoxContainer ButtonContainer;

    [Export]
    private Label _description;

    [Export]
    private Button _acceptButton;

    [Export]
    private Button _skipButton;

    public delegate void SelectionMadeHandler();
    public event SelectionMadeHandler Selected;

    private PlayerStats _player;

    private RelicTemplate[] _rChoices; //TODO: look into typed functions
    private RelicTemplate _rSelection;
    private Note[] _nChoices;
    private Note _nSelection;

    public void Initialize(PlayerStats player, int amount, string type)
    {
        _player = player;
        if (type == "Relic")
        {
            GenerateRelicChoices(amount);
        }
        else
        {
            GenerateNoteChoices(amount);
        }

        _acceptButton.Pressed += OnSelect;
        _skipButton.Pressed += OnSkip;
    }

    public override void _Process(double delta)
    {
        _acceptButton.Visible = (_nSelection != null) || (_rSelection != null);
    }

    private void GenerateRelicChoices(int amount = 1)
    {
        if (amount < 1)
            GD.PushError("Error: In RewardSelect: amount < 1");
        _rChoices = Scribe.GetRandomRelics(_player.CurRelics, amount);

        foreach (var relic in _rChoices)
        {
            var button = new DisplayButton();
            button.Display(relic.Texture, relic.Tooltip, relic.Name);
            button.Pressed += () => OnRelicSelected(relic);
            ButtonContainer.AddChild(button);
        }
        ButtonContainer.GetChild<Button>(0).GrabFocus();
    }

    private void GenerateNoteChoices(int amount = 1)
    {
        if (amount < 1)
            GD.PushError("Error: In RewardSelect: amount < 1");
        _nChoices = Scribe.GetRandomRewardNotes(amount);

        foreach (var note in _nChoices)
        {
            var button = new DisplayButton();
            button.Display(note.Texture, note.Tooltip, note.Name);
            button.Pressed += () => OnNoteSelected(note);
            ButtonContainer.AddChild(button);
        }
        ButtonContainer.GetChild<Button>(0).GrabFocus();
    }

    public static RewardSelect CreateSelection(
        Node2D parent,
        PlayerStats playerStats,
        int amount,
        string type
    )
    {
        var rewardUI = GD.Load<PackedScene>("res://Scenes/UI/RewardSelectionUI.tscn")
            .Instantiate<RewardSelect>();
        parent.AddChild(rewardUI);
        rewardUI.Initialize(playerStats, amount, type);
        parent.GetTree().Paused = true;

        return rewardUI;
    }

    private void OnNoteSelected(Note choiceNote)
    {
        _nSelection = choiceNote;
        string notename = choiceNote.Name.ToUpper();
        _description.Text =
            Tr("NOTE_" + notename + "_NAME") + ": " + Tr("NOTE_" + notename + "_TOOLTIP");
    }

    private void OnRelicSelected(RelicTemplate choiceRelic)
    {
        _rSelection = choiceRelic;
        string relicname = choiceRelic.Name.ToUpper();
        relicname = relicname.Replace(" ", "");
        _description.Text =
            Tr("RELIC_" + relicname + "_NAME") + ": " + Tr("RELIC_" + relicname + "_TOOLTIP");
    }

    private void OnSelect()
    {
        if (_nSelection == null && _rSelection == null)
            return;
        if (_nSelection != null)
        {
            _player.AddNote(_nSelection);
        }
        else if (_rSelection != null)
        {
            _player.AddRelic(_rSelection);
        }
        GetTree().Paused = false;
        Selected?.Invoke();
        QueueFree();
    }

    private void OnSkip()
    {
        GetTree().Paused = false;
        Selected?.Invoke();
        QueueFree();
    }
}
