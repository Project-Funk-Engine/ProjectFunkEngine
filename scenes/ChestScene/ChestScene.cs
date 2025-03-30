using System;
using FunkEngine;
using Godot;

/**
 * <summary>Scene for handling rooms with random loot.</summary>
 */
public partial class ChestScene : Node2D
{
    public static readonly string LoadPath = "res://Scenes/ChestScene/ChestScene.tscn";
    public PlayerPuppet Player;

    [Export]
    public Button ChestButton;

    public override void _Ready()
    {
        Player = GD.Load<PackedScene>(PlayerPuppet.LoadPath).Instantiate<PlayerPuppet>();
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
        RewardSelect.CreateSelection(this, Player.Stats, 3, Stages.Chest).Selected += EndBattle;
    }

    private void EndBattle()
    {
        StageProducer.ChangeCurRoom(StageProducer.Config.BattleRoom.Idx);
        StageProducer.LiveInstance.TransitionStage(Stages.Map);
    }
}
