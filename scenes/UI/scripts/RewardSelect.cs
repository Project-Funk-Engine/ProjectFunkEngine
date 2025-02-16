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

    private PlayerStats _player;
    private RelicTemplate[] _choices;
    private RelicTemplate _selection;

    public void Initialize(PlayerStats player, int amount)
    {
        _player = player;
        GenerateRelicChoices(amount);

        _acceptButton.Pressed += OnSelect;
        _skipButton.Pressed += OnSkip;
    }

    public override void _Process(double delta)
    {
        _acceptButton.Visible = _selection != null;
    }

    private void GenerateRelicChoices(int amount = 1)
    {
        //should probably change this so that the amount of relics offered can be changed when BD calls it
        //i.e less options when killing trash mobs/basic/weak enemies
        _choices = Scribe.GetRandomRelics(_player.CurRelics, amount);

        foreach (var relic in _choices)
        {
            var button = new DisplayButton();
            button.Display(relic.Texture, relic.Tooltip, relic.Name);
            button.Pressed += () => OnRelicSelected(relic);
            ButtonContainer.AddChild(button);
            button.GrabFocus();
        }
    }

    private void OnRelicSelected(RelicTemplate choiceRelic)
    {
        _selection = choiceRelic;
        _description.Text = $"{choiceRelic.Name}: {choiceRelic.Tooltip}";
    }

    private void OnSelect()
    {
        if (_selection == null)
            return;
        GD.Print("Relic selected: " + _selection.Name);
        _player.AddRelic(_selection);
        GetTree().Paused = false;
        QueueFree();
    }

    private void OnSkip()
    {
        GD.Print("Relic skipped.");
        GetTree().Paused = false;
        QueueFree();
    }
}
