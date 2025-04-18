using FunkEngine;
using Godot;

public partial class RewardSelect : CanvasLayer
{
    public static readonly string LoadPath = "res://Scenes/UI/RewardSelectionUI.tscn";

    [Export]
    public HBoxContainer ButtonContainer;

    [Export]
    private Label _description;

    [Export]
    private Button _acceptButton;

    [Export]
    private Button _skipButton;

    [Export]
    private Button _rerollButton;

    private ButtonGroup _rewardGroup;

    public delegate void SelectionMadeHandler();
    public event SelectionMadeHandler Selected;

    private PlayerStats _player;

    private RelicTemplate[] _rChoices;
    private Note[] _nChoices;
    private IDisplayable _selection;

    private void Initialize(PlayerStats player, int amount, Stages type)
    {
        _player = player;
        _rewardGroup = new ButtonGroup();

        _roomType = type;
        _amount = amount + player.RewardAmountModifier;
        if (player.Rerolls > 0)
        {
            _curRerolls = player.Rerolls;
            _rerollButton.Visible = true;
            _rerollButton.Text = Tr("CHEST_ROOM_REROLL") + _curRerolls;
            _rerollButton.Pressed += Reroll;
        }

        GenerateSelection();

        _acceptButton.Pressed += OnSelect;
        _acceptButton.FocusEntered += () => ChangeDescription(_selection);
        _skipButton.Pressed += OnSkip;
    }

    public override void _Process(double delta)
    {
        _acceptButton.Visible = (_selection != null);
    }

    private void AddButton(IDisplayable displayable)
    {
        var button = GD.Load<PackedScene>(DisplayButton.LoadPath).Instantiate<DisplayButton>();
        button.SetButtonGroup(_rewardGroup);
        button.ToggleMode = true;
        button.Display(displayable.Texture, displayable.Tooltip, displayable.Name);
        button.Pressed += () => SetSelection(displayable);
        button.FocusEntered += () => ChangeDescription(displayable);
        ButtonContainer.AddChild(button);
    }

    private void GenerateSelection()
    {
        if (_roomType == Stages.Battle)
        {
            GenerateNoteChoices(_amount);
        }
        else
        {
            GenerateRelicChoices(_amount);
        }
    }

    private void GenerateRelicChoices(int amount = 1)
    {
        if (amount < 1)
            GD.PushError("Error: In RewardSelect: amount < 1");
        _rChoices = Scribe.GetRandomRelics(
            amount,
            StageProducer.CurRoom + 10 * _curRerolls,
            _player.RarityOdds
        );
        int numChildren = ButtonContainer.GetChildCount();
        foreach (var relic in _rChoices)
        {
            AddButton(relic);
        }
        ButtonContainer.GetChild<Button>(numChildren).GrabFocus();
    }

    private void GenerateNoteChoices(int amount = 1)
    {
        if (amount < 1)
            GD.PushError("Error: In RewardSelect: amount < 1");
        _nChoices = Scribe.GetRandomRewardNotes(amount, StageProducer.CurRoom + 10 * _curRerolls);
        int numChildren = ButtonContainer.GetChildCount();
        foreach (var note in _nChoices)
        {
            AddButton(note);
        }
        ButtonContainer.GetChild<Button>(numChildren).GrabFocus();
    }

    public static RewardSelect CreateSelection(
        Node2D parent,
        PlayerStats playerStats,
        int amount,
        Stages type
    )
    {
        var rewardUI = GD.Load<PackedScene>(LoadPath).Instantiate<RewardSelect>();
        parent.AddChild(rewardUI);
        rewardUI.Initialize(playerStats, amount, type);
        parent.ProcessMode = ProcessModeEnum.Disabled;

        return rewardUI;
    }

    private void ChangeDescription(IDisplayable displayable)
    {
        if (displayable == null)
        {
            _description.Text = "";
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
        _description.Text = Tr(type + name + "_NAME") + ": " + Tr(type + name + "_TOOLTIP");
    }

    private int _curRerolls;
    private Stages _roomType;
    private int _amount; //The UI can accomodate an effectively infinite amount, but preferably this should be <=11

    private void Reroll()
    {
        _curRerolls--;
        _selection = null;
        foreach (Node child in ButtonContainer.GetChildren())
        {
            child.QueueFree();
        }
        GenerateSelection();
        if (_curRerolls < 1)
        {
            _rerollButton.Visible = false;
            return;
        }
        _rerollButton.Text = Tr("CHEST_ROOM_REROLL") + _curRerolls;
    }

    private void SetSelection(IDisplayable reward)
    {
        _selection = reward;
        ChangeDescription(reward);
    }

    private void OnSelect()
    {
        switch (_selection)
        {
            case Note note:
                _player.AddNote(note);
                break;
            case RelicTemplate relic:
                _player.AddRelic(relic);
                break;
            default:
                return;
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
