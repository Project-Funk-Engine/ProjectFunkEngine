using System;
using Godot;

public partial class NotePlacementBar : Node
{
    const int MaxValue = 80;
    private int _currentBarValue;
    private int _currentCombo;
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
        _currentBarValue = 0;
        _currentCombo = 0;
        comboMult = 1;
        notesToIncreaseCombo = 4;
    }

    public void ComboText(string text)
    {
        TextParticle newText = new TextParticle();
        AddChild(newText);
        newText.Text = text + $" {_currentCombo}";
    }

    // Hitting a note increases combo, combo mult, and note placement bar
    public void HitNote()
    {
        _currentCombo++;
        DetermineComboMult();
        _currentBarValue += comboMult;
        UpdateNotePlacementBar(_currentBarValue);
        UpdateComboMultText();
    }

    // Missing a note resets combo
    public void MissNote()
    {
        _currentCombo = 0;
        DetermineComboMult();
        UpdateComboMultText();
    }

    // Placing a note resets the note placement bar
    public void PlacedNote()
    {
        _currentBarValue = 0;
        UpdateNotePlacementBar(_currentBarValue);
    }

    public bool CanPlaceNote()
    {
        return _currentBarValue >= MaxValue;
    }

    private void DetermineComboMult()
    {
        comboMult = _currentCombo / notesToIncreaseCombo + 1;
    }

    private void UpdateNotePlacementBar(int newValue)
    {
        notePlacementBar.Value = newValue;
    }

    private void UpdateComboMultText()
    {
        currentComboMultText.Text = $"x{comboMult.ToString()}";
    }
}
