using System;
using System.Collections.Generic;
using System.Linq;
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

    [Export]
    private Sprite2D _currentNote;
    private Note _currentNoteInstance;

    [Export]
    private Sprite2D _nextNote;

    [Export]
    private CpuParticles2D fullBarParticles;

    private Note[] _noteDeck;
    private Queue<Note> _noteQueue = new Queue<Note>();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        notePlacementBar.MaxValue = MaxValue;
        _currentBarValue = 0;
        _currentCombo = 0;
        comboMult = 1;
        notesToIncreaseCombo = 4;
    }

    public void Setup(PlayerStats playerStats)
    {
        //Create during battle copies of player notes
        _noteDeck = (Note[])playerStats.CurNotes.Clone(); //Do NOT ever change these notes directly :)
        if (_noteDeck.Length <= 0)
        {
            GD.PushError("Imminent crash: Note Deck is empty!");
            GetTree().Paused = true;
        }
        ShuffleNoteQueue();
        ProgressQueue();
    }

    // Fisher-Yates shuffle from: https://stackoverflow.com/a/1262619
    private void ShuffleNoteQueue()
    {
        List<Note> tempList = new List<Note>(_noteDeck);

        int n = tempList.Count;
        while (n > 1)
        {
            n--;
            int k = StageProducer.GlobalRng.RandiRange(0, n);
            (tempList[k], tempList[n]) = (tempList[n], tempList[k]);
        }

        _noteQueue = new Queue<Note>(tempList);
    }

    private void ProgressQueue()
    {
        if (_noteQueue.Count == 0)
        {
            ShuffleNoteQueue();
        }
        if (_currentNoteInstance == null)
        {
            _currentNoteInstance = _noteQueue.Dequeue();
            if (_noteQueue.Count == 0)
            {
                ShuffleNoteQueue();
            }
        }
        UpdateNoteQueue();
    }

    private void UpdateNoteQueue()
    {
        _currentNote.Texture = _currentNoteInstance.Texture;
        _nextNote.Texture = _noteQueue.Count > 0 ? _noteQueue.Peek().Texture : null;
    }

    private Note GetNote(bool getNextNote = false)
    {
        Note result;
        if (!getNextNote)
        {
            result = _currentNoteInstance;
            _currentNoteInstance = null;
        }
        else
        {
            result = _noteQueue.Dequeue();
        }
        ProgressQueue();
        return result;
    }

    // Hitting a note increases combo, combo mult, and note placement bar
    public void HitNote()
    {
        _currentCombo++;
        DetermineComboMult();
        _currentBarValue = Math.Min(_currentBarValue + comboMult, MaxValue);
        UpdateNotePlacementBar(_currentBarValue);
        UpdateComboMultText();
        //fullBarParticles.Emitting = CanPlaceNote();
    }

    // Missing a note resets combo
    public void MissNote()
    {
        _currentCombo = 0;
        DetermineComboMult();
        UpdateComboMultText();
    }

    // Placing a note resets the note placement bar
    public Note PlacedNote()
    {
        _currentBarValue -= (int)(_currentNoteInstance.CostModifier * MaxValue);

        UpdateNotePlacementBar(_currentBarValue);
        //fullBarParticles.Emitting = false;
        return GetNote();
    }

    public bool CanPlaceNote()
    {
        return _currentBarValue >= MaxValue;
    }

    private void DetermineComboMult()
    {
        comboMult = _currentCombo / notesToIncreaseCombo + 1;
    }

    public int GetCurrentCombo()
    {
        return _currentCombo;
    }

    private void UpdateNotePlacementBar(int newValue)
    {
        notePlacementBar.Value = newValue;
    }

    private void UpdateComboMultText()
    {
        currentComboMultText.Text = $"    x{comboMult.ToString()}";
    }
}
