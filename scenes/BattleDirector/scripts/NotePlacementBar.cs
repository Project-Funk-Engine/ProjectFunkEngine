using System;
using System.Collections.Generic;
using System.Linq;
using FunkEngine;
using Godot;

public partial class NotePlacementBar : Node
{
    const int MaxValue = 80;
    private int _currentBarValue;
    private int _currentCombo;
    int comboMult;
    int bonusMult;
    int notesToIncreaseCombo;

    [Export]
    TextureProgressBar notePlacementBar;

    [Export]
    TextEdit currentComboMultText;

    [Export]
    private GpuParticles2D _particles;

    [Export]
    private Sprite2D _currentNote;
    private Note _currentNoteInstance;

    [Export]
    private Sprite2D _nextNote;

    [Export]
    private CpuParticles2D fullBarParticles;

    private Note[] _noteDeck;
    private Queue<Note> _noteQueue = new Queue<Note>();

    //Juice - https://www.youtube.com/watch?v=LGt-jjVf-ZU
    private int _limiter;
    private Vector2 _barInitPosition;
    private float _randomStrength = 1f;
    private float _shakeFade = 10f;
    private RandomNumberGenerator _rng = new();
    private float _shakeStrength;

    private void ProcessShake(double delta)
    {
        _limiter = (_limiter + 1) % 3;
        if (_limiter != 1)
            return;
        if (_currentBarValue >= MaxValue)
        {
            _shakeStrength = _randomStrength;
        }
        if (_shakeStrength > 0)
        {
            _shakeStrength = (float)Mathf.Lerp(_shakeStrength, 0, _shakeFade * delta);
        }
        notePlacementBar.Position =
            _barInitPosition
            + new Vector2(
                _rng.RandfRange(-_shakeStrength, _shakeStrength),
                _rng.RandfRange(-_shakeStrength, _shakeStrength)
            );
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        notePlacementBar.MaxValue = MaxValue;
        _currentBarValue = 0;
        _currentCombo = 0;
        comboMult = 1;
        notesToIncreaseCombo = 4;

        _barInitPosition = notePlacementBar.Position;
    }

    public override void _Process(double delta)
    {
        ProcessShake(delta);
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
        bonusMult = 0;
        DetermineComboMult();
        UpdateComboMultText();
    }

    public void IncreaseBonusMult(int amount = 1)
    {
        bonusMult += amount;
        DetermineComboMult();
        UpdateComboMultText();
    }

    public void IncreaseCharge(int amount = 1)
    {
        _currentBarValue = Math.Min(_currentBarValue + amount, MaxValue);
        UpdateNotePlacementBar(_currentBarValue);
        UpdateComboMultText();
    }

    // Placing a note resets the note placement bar
    public Note PlacedNote(BattleDirector BD)
    {
        _currentBarValue -= (int)(_currentNoteInstance.CostModifier * MaxValue);

        UpdateNotePlacementBar(_currentBarValue);
        //fullBarParticles.Emitting = false;

        Note placedNote = GetNote(Input.IsActionPressed("Secondary"));
        placedNote?.OnHit(BD, Timing.Okay); //Hardcode for now, eventually the note itself could have its default
        return placedNote;
    }

    public bool CanPlaceNote()
    {
        return _currentBarValue >= MaxValue;
    }

    private void DetermineComboMult()
    {
        comboMult = _currentCombo / notesToIncreaseCombo + 1 + bonusMult;
    }

    public int GetCurrentCombo()
    {
        return _currentCombo;
    }

    private void UpdateNotePlacementBar(int newValue)
    {
        notePlacementBar.Value = newValue;
        _particles.Emitting = _currentBarValue >= MaxValue;
    }

    private void UpdateComboMultText()
    {
        currentComboMultText.Text = $"    x{comboMult.ToString()}";
    }
}
