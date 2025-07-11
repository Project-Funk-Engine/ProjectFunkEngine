using System;
using System.Collections.Generic;
using FunkEngine;
using Godot;

/**<summary>Current charge bar amount and note queue.</summary>
 */
public partial class NotePlacementBar : Node
{ //TODO: More effectively integrate with player
    private double MaxValue
    {
        get => _notePlacementBar.MaxValue;
        set
        {
            _notePlacementBar.MaxValue = value;
            _particles.Emitting = CurrentBarValue >= MaxValue;
        }
    }
    private double CurrentBarValue
    {
        get => _notePlacementBar.Value;
        set
        {
            _notePlacementBar.Value = value;
            _particles.Emitting = CurrentBarValue >= MaxValue; //This is so goated
            _waveMaterial.SetShaderParameter("fillLevel", _notePlacementBar.Value / MaxValue);
        }
    }

    [Export]
    private ShaderMaterial _waveMaterial; //Sort of breaks the pixel art style, but its cool

    [Export]
    private TextureProgressBar _notePlacementBar;
    private Gradient _gradTex;

    [Export]
    private GpuParticles2D _particles;

    [Export]
    private CpuParticles2D _fullBarParticles;

    private Note[] _noteDeck;

    #region Combo&Mult
    [Export]
    private TextEdit _currentComboMultText;

    private int _currentCombo;
    private int MaxComboMult;
    public int ComboMult =>
        Math.Min(_currentCombo / _notesToIncreaseCombo + 1 + _bonusMult, MaxComboMult);
    private int _bonusMult;
    private int _notesToIncreaseCombo;

    private void UpdateComboMultText()
    {
        _currentComboMultText.Text = $" x{ComboMult.ToString()}";
    }
    #endregion

    #region Initialization
    public override void _Ready()
    {
        MaxValue = 60;
        _notesToIncreaseCombo = 4;

        _barInitPosition = _notePlacementBar.Position;

        if (_notePlacementBar.TextureProgress is GradientTexture2D gradientTexture)
            _gradTex = gradientTexture.Gradient;

        _waveMaterial.SetShaderParameter("fillLevel", 0.0);
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
        MaxValue = playerStats.MaxComboBar;
        MaxComboMult = playerStats.MaxComboMult;
        _notesToIncreaseCombo = playerStats.NotesToIncreaseCombo;
    }
    #endregion

    #region NoteQueue
    [Export]
    private Sprite2D _currentNote; //Sprite for current note
    private Note _currentNoteInstance; //First note in queue, store outside of queue to maintain first and second

    [Export]
    private Sprite2D _nextNote; //Secondary note, stored as the first in teh queue

    private Queue<Note> _noteQueue = new Queue<Note>();

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

    //Progress the queue by one, and handle shuffling and setting current instances
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
    #endregion

    #region Helpers
    private Color[] _fillColors = [Colors.Green, Colors.Aqua, Colors.Pink, Colors.Red];

    private void FillWithColor(ArrowType type, Color overrideCol = default)
    {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (CurrentBarValue == MaxValue)
            return;
        Color color = overrideCol == default ? _fillColors[(int)type] : overrideCol;

        if (_gradTex.GetPointCount() == 1)
            _gradTex.SetColor(0, color);

        float offset = (float)((CurrentBarValue) / MaxValue);
        _gradTex.AddPoint(offset, color);
    }

    private void ClearColors()
    {
        for (int i = _gradTex.GetPointCount() - 1; i > 0; i--)
            _gradTex.RemovePoint(i);
    }
    #endregion

    #region Public Functions
    public int GetCurrentCombo()
    {
        return _currentCombo;
    }

    public double GetCurrentBarValue()
    {
        return CurrentBarValue;
    }

    public void ResetCurrentCombo()
    {
        _currentCombo = 0;
        _bonusMult = 0;
    }

    //For external events to directly change mult
    public void IncreaseBonusMult(int amount = 1)
    {
        _bonusMult += amount;
        UpdateComboMultText();
    }

    public void IncreaseCharge(int amount = 1)
    {
        FillWithColor(default, Colors.DarkViolet);
        CurrentBarValue += amount;
    }

    public bool CanPlaceNote()
    {
        return CurrentBarValue >= MaxValue;
    }

    // Placing a note resets the note placement bar, should only be called if CanPlaceNote was already checked true
    public Note NotePlaced()
    {
        if (!CanPlaceNote())
            GD.PushWarning("Note is attempting placement without a full bar!");
        string inputType = Configkeeper
            .GetConfigValue(Configkeeper.ConfigSettings.InputType)
            .ToString();
        Note placedNote = GetNote(Input.IsActionPressed(inputType + "_secondaryPlacement"));
        CurrentBarValue -= placedNote.CostModifier * MaxValue;
        ClearColors();
        return placedNote;
    }

    public void HandleTiming(Timing timed, ArrowType type)
    {
        if (BattleDirector.PlayerDisabled)
            return;
        if (timed == Timing.Miss)
        {
            MissNote();
        }
        else
        {
            HitNote(type);
        }
    }

    // Hitting a note increases combo, combo mult, and note placement bar
    private void HitNote(ArrowType type)
    {
        _currentCombo++;
        FillWithColor(type);
        CurrentBarValue += ComboMult;
        UpdateComboMultText();
    }

    public bool IgnoreMiss; //a one time safe miss

    // Missing a note resets combo
    private void MissNote()
    {
        if (IgnoreMiss)
        {
            IgnoreMiss = false;
            return;
        }
        ResetCurrentCombo();
        UpdateComboMultText();
    }

    public void SetIgnoreMiss(bool ignore)
    {
        IgnoreMiss = ignore;
    }
    #endregion

    #region Shake
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
        if (CurrentBarValue >= MaxValue)
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
    #endregion
}
