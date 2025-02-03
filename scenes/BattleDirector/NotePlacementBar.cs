using System;
using Godot;

public partial class NotePlacementBar : Node
{
    const int MaxValue = 100;
    int currentBarValue;
    int currentCombo;
    int comboMult;
    int notesToIncreaseCombo;

    [Export]
    ProgressBar notePlacementBar;

    [Export]
    TextEdit currentComboMultText;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        currentBarValue = 0;
        currentCombo = 0;
        comboMult = 1;
        notesToIncreaseCombo = 4;
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
        if (currentBarValue >= MaxValue)
            return true;
        return false;
    }

    private void DetermineComboMult()
    {
        comboMult = currentCombo / notesToIncreaseCombo;
        if (comboMult == 0)
            comboMult = 1;
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
