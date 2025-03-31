using System;
using System.Collections.Generic;
using FunkEngine;
using Godot;

/**<summary>Current charge bar amount and note queue.</summary>
 */
public partial class NotePlacementBar : Node
{ //TODO: More effectively integrate with player
    const int MaxValue = 80;
    private int _currentBarValue;
    private int _currentCombo;
    private int _comboMult;
    private int _bonusMult;
    private int _notesToIncreaseCombo;

    [Export]
    private TextureProgressBar _notePlacementBar;

    [Export]
    private TextEdit _currentComboMultText;

    [Export]
    private GpuParticles2D _particles;

    [Export]
    private Sprite2D _currentNote;
    private Note _currentNoteInstance;

    [Export]
    private Sprite2D _nextNote;

    [Export]
    private CpuParticles2D _fullBarParticles;

    private Note[] _noteDeck;
    private Queue<Note> _noteQueue = new Queue<Note>();

    //Juice - https://www.youtube.com/watch?v=LGt-jjVf-ZU
    private int _limiter;
    private Vector2 _barInitPosition;
    private float _baseShake = 1f;
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
            _shakeStrength = _baseShake;
        }
        if (_shakeStrength > 0)
        {
            _shakeStrength = (float)Mathf.Lerp(_shakeStrength, 0, _shakeFade * delta);
        }
        _notePlacementBar.Position =
            _barInitPosition
            + new Vector2(
                _rng.RandfRange(-_shakeStrength, _shakeStrength),
                _rng.RandfRange(-_shakeStrength, _shakeStrength)
            );
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _notePlacementBar.MaxValue = MaxValue;
        _currentBarValue = 0;
        _currentCombo = 0;
        _comboMult = 1;
        _notesToIncreaseCombo = 4;

        _barInitPosition = _notePlacementBar.Position;
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
        Sprite2D selectedNote;
        if (!getNextNote)
        {
            selectedNote = _currentNote;
            result = _currentNoteInstance;
            _currentNoteInstance = null;
        }
        else
        {
            selectedNote = _nextNote;
            result = _noteQueue.Dequeue();
        }
        NoteQueueParticlesFactory.NoteParticles(selectedNote, selectedNote.Texture);
        ProgressQueue();
        return result;
    }

    public void HandleTiming(Timing timed)
    {
        if (timed == Timing.Miss)
        {
            MissNote();
        }
        else
        {
            HitNote();
        }
    }

    // Hitting a note increases combo, combo mult, and note placement bar
    private void HitNote()
    {
        _currentCombo++;
        DetermineComboMult();
        _currentBarValue = Math.Min(_currentBarValue + _comboMult, MaxValue);
        UpdateNotePlacementBar(_currentBarValue);
        UpdateComboMultText();
    }

    // Missing a note resets combo
    private void MissNote()
    {
        _currentCombo = 0;
        _bonusMult = 0;
        DetermineComboMult();
        UpdateComboMultText();
    }

    public void IncreaseBonusMult(int amount = 1)
    {
        _bonusMult += amount;
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
        Note placedNote = GetNote(Input.IsActionPressed("Secondary"));

        _currentBarValue -= (int)(placedNote.CostModifier * MaxValue);
        UpdateNotePlacementBar(_currentBarValue);

        placedNote.OnHit(BD, Timing.Okay); //Hardcode for now, eventually the note itself could have its default
        return placedNote;
    }

    public bool CanPlaceNote()
    {
        return _currentBarValue >= MaxValue;
    }

    private void DetermineComboMult()
    {
        _comboMult = _currentCombo / _notesToIncreaseCombo + 1 + _bonusMult;
    }

    public int GetCurrentCombo()
    {
        return _currentCombo;
    }

    private void UpdateNotePlacementBar(int newValue)
    {
        _notePlacementBar.Value = newValue;
        _particles.Emitting = _currentBarValue >= MaxValue;
    }

    private void UpdateComboMultText()
    {
        _currentComboMultText.Text = $"    x{_comboMult.ToString()}";
    }
}
