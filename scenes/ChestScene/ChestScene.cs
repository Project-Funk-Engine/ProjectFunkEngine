using System;
using FunkEngine;
using Godot;

public partial class ChestScene : Node2D
{
    public PlayerPuppet Player;

    [Export]
    public Button ChestButton;

    public override void _Ready()
    {
        Player = GD.Load<PackedScene>("res://scenes/Puppets/PlayerPuppet.tscn")
            .Instantiate<PlayerPuppet>();
        AddChild(Player);

        ChestButton.Pressed += GetLoot;
    }

    public override void _Process(double delta)
    {
        if (!ChestButton.Disabled)
        {
            ChestButton.GrabFocus();
        }
    }

    private void GetLoot()
    {
        ChestButton.Disabled = true;
        RewardSelect.CreateSelection(this, Player.Stats, 3, "Relic").Selected += EndBattle;
    }

    private void EndBattle()
    {
        StageProducer.ChangeCurRoom(StageProducer.Config.BattleRoom);
        GetNode<StageProducer>("/root/StageProducer").TransitionStage(Stages.Map);
    }
}
