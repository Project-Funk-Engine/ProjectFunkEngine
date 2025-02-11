using System;
using System.Linq;
using Godot;

public partial class RewardSelect : CanvasLayer
{
    [Export]
    public VBoxContainer ButtonContainer;

    private PlayerStats _player;
    private RelicTemplate[] _choices;

    public void Initialize(PlayerStats player)
    {
        _player = player;
        GenerateRelicChoices();
    }

    private void GenerateRelicChoices()
    {
        //should probably change this so that the amount of relics offered can be changed when BD calls it
        //i.e less options when killing trash mobs/basic/weak enemies
        _choices = Reward.GetMultipleRelics(_player.CurRelics, 3);

        foreach (var relic in _choices)
        {
            var button = new Button();
            button.Text = relic.Name;
            button.Pressed += () => OnRelicSelected(relic);
            ButtonContainer.AddChild(button);
        }
    }

    private void OnRelicSelected(RelicTemplate choiceRelic)
    {
        Reward.AddRelic(_player, choiceRelic);
        GD.Print("Relic selected: " + choiceRelic.Name);

        QueueFree();
    }
}
