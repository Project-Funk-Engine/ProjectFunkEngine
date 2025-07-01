using System;
using System.Globalization;
using FunkEngine;
using Godot;

public partial class CustomScore : CanvasLayer
{
    public const string LoadPath = "res://Scenes/CustomSong/CustomScoreScreen.tscn";

    private enum ScoringVals
    {
        PlayerHP = 0,
        EnemyHP = 1,
        NotesPlaced = 2,
        Perfects = 3,
        Misses = 4,
        Loops = 5,
    }

    private float[] score = new float[6];

    [Export]
    private Label[] _amtLabels = new Label[6];

    [Export]
    private Button _acceptButton;

    public delegate void FinishedHandler();
    public event FinishedHandler Finished;

    private BattleDirector.Harbinger.NoteHitHandler _noteHitListener;
    private BattleDirector.Harbinger.NotePlacedHandler _notePlacedListener;

    public void ListenToDirector()
    {
        _noteHitListener = e =>
        {
            if (e is not BattleDirector.Harbinger.NoteHitArgs nArgs)
                return;
            switch (nArgs.Timing)
            {
                case Timing.Perfect:
                    score[(int)ScoringVals.Perfects] += 1;
                    break;
                case Timing.Miss:
                    score[(int)ScoringVals.Misses] += 1;
                    break;
            }
        };
        _notePlacedListener = _ =>
        {
            score[(int)ScoringVals.NotesPlaced] += 1;
        };

        BattleDirector.Harbinger.Instance.NotePlaced += _notePlacedListener;
        BattleDirector.Harbinger.Instance.NoteHit += _noteHitListener;
    }

    public override void _ExitTree()
    {
        BattleDirector.Harbinger.Instance.NotePlaced -= _notePlacedListener;
        BattleDirector.Harbinger.Instance.NoteHit -= _noteHitListener;
    }

    public CustomScore ShowResults(BattleDirector battleDirector, float enemyPercent)
    {
        score[(int)ScoringVals.Loops] = TimeKeeper.LastBeat.Loop;
        score[(int)ScoringVals.PlayerHP] =
            (float)battleDirector.Player.GetCurrentHealth()
            / StageProducer.PlayerStats.MaxHealth
            * 100;
        score[(int)ScoringVals.EnemyHP] = enemyPercent * 100;

        for (int i = 0; i < 6; i++)
        {
            _amtLabels[i].Text = score[i].ToString("0");
            if (i == (int)ScoringVals.PlayerHP || i == (int)ScoringVals.EnemyHP)
                _amtLabels[i].Text += "%";
        }
        battleDirector.AddChild(this);
        battleDirector.ProcessMode = ProcessModeEnum.Disabled;

        return this;
    }

    public override void _Ready()
    {
        _acceptButton.Pressed += FinishScoring;
    }

    public override void _Process(double delta)
    {
        _acceptButton.GrabFocus();
    }

    private void FinishScoring()
    {
        Finished?.Invoke();
        QueueFree();
    }
}
