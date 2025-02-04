using System;
using Godot;

public partial class NotePlacementBar : Node
{
    const int MaxValue = 5;
    int currentBarValue;
    int currentCombo;
    int comboMult;
    int notesToIncreaseCombo;

    [Export]
    TextureProgressBar notePlacementBar;

    [Export]
    TextEdit currentComboMultText;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        notePlacementBar.MaxValue = MaxValue;
        currentBarValue = 0;
        currentCombo = 0;
        comboMult = 1;
        notesToIncreaseCombo = 4;
    }

    public void ComboText(string text)
    {
        var feedbackScene = ResourceLoader.Load<PackedScene>(
            "res://scenes/BattleDirector/TextParticle.tscn"
        );
        TextParticle newText = feedbackScene.Instantiate<TextParticle>();
        AddChild(newText);
        newText.Text = text + $" {currentCombo}";
    }

    // Hitting a note increases combo, combo mult, and note placement bar
    public void HitNote()
    {
        currentCombo++;
        DetermineComboMult();
        currentBarValue += comboMult;
        UpdateNotePlacementBar(currentBarValue);
        UpdateComboMultText();
    }

    // Missing a note resets combo
    public void MissNote()
    {
        currentCombo = 0;
        DetermineComboMult();
        UpdateComboMultText();
    }

    // Placing a note resets the note placement bar
    public void PlacedNote()
    {
        currentBarValue = 0;
        UpdateNotePlacementBar(currentBarValue);
    }

    public bool CanPlaceNote()
    {
        return currentBarValue >= MaxValue;
    }

    private void DetermineComboMult()
    {
        comboMult = currentCombo / notesToIncreaseCombo + 1;
    }

    public void UpdateNotePlacementBar(int newValue)
    {
        notePlacementBar.Value = newValue;
    }

    public void UpdateComboMultText()
    {
        currentComboMultText.Text = $"x{comboMult.ToString()}";
    }
}
