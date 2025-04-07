using FunkEngine;
using Godot;

/**
 * <summary>Scene for handling rooms with random loot.</summary>
 */
public partial class ChestScene : Node2D
{
    public static readonly string LoadPath = "res://Scenes/ChestScene/ChestScene.tscn";
    private PlayerPuppet _player;

    [Export]
    public Button ChestButton;

    [Export]
    public Marker2D PlayerMarker;

    public override void _Ready()
    {
        _player = GD.Load<PackedScene>(PlayerPuppet.LoadPath).Instantiate<PlayerPuppet>();
        PlayerMarker.AddChild(_player);

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
        RewardSelect.CreateSelection(this, _player.Stats, 3, Stages.Chest).Selected += EndBattle;
    }

    private void EndBattle()
    {
        StageProducer.LiveInstance.TransitionStage(Stages.Map);
    }
}
