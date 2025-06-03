using System;
using System.Linq;
using Godot;

public partial class NewEndScreen : Control
{
    public static readonly string LoadPath = "res://Scenes/EndScreen/EndScreen.tscn";

    [Export]
    private Label _resultLabel;

    [Export]
    private Label _battlesWonLabel;

    [Export]
    private Label _notesPlacedLabel;

    [Export]
    private Label _hitRateLabel;

    [Export]
    private Label _relicsLabel;

    [Export]
    private Label _notesLabel;

    [Export]
    private Button _continueButton;

    public void Setup(bool won)
    {
        var stats = StageProducer.PlayerStats;
        _resultLabel.Text = won ? "Victory!" : "Defeat";
        _battlesWonLabel.Text = $"Battles Won: {stats.BattlesWon}";
        _notesPlacedLabel.Text = $"Notes Placed: {stats.TotalNotesPlaced}";
        _hitRateLabel.Text = $"Hit Rate: {stats.HitRate:P2}";
        _relicsLabel.Text = $"Relics: {string.Join(", ", stats.CurRelics.Select(r => r.Name))}";
        _notesLabel.Text = $"Notes: {string.Join(", ", stats.CurNotes.Select(n => n.Name))}";

        // _continueButton.Pressed += OnContinuePressed;
    }
}
