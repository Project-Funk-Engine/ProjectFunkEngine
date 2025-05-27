using System;
using System.Linq;
using Godot;

public partial class ScoringScreen : CanvasLayer
{
    public static readonly string LoadPath = "res://Scenes/UI/ScoreScreen.tscn";

    public struct ScoreGuide
    {
        public int BaseMoney = 0;
        public int TotalHits = 0;
        public int TotalPerfects = 0;
        public int TotalPlaced = 0;
        public int RelicBonus = 0;
        public float StartingHealth = 0;
        public float EndingHealth = 0;

        public ScoreGuide(int baseMoney, float startingHealth)
        {
            BaseMoney = baseMoney;
            StartingHealth = startingHealth;
        }

        public void IncreaseBase(int amount)
        {
            BaseMoney += amount;
        }

        public void IncHits()
        {
            TotalHits++;
        }

        public void IncPerfects()
        {
            TotalPerfects++;
        }

        public void IncPlaced()
        {
            TotalPlaced++;
        }

        public void IncRelicBonus(int amount)
        {
            RelicBonus += amount;
        }

        public void SetEndHp(float amount)
        {
            EndingHealth = amount;
        }
    }

    [Export]
    private Label _styleLabel;

    [Export]
    private Label _styleAmount;

    [Export]
    private Label _perfectsLabel;

    [Export]
    private Label _perfectsAmount;

    [Export]
    private Label _placedLabel;

    [Export]
    private Label _placedAmount;

    [Export]
    private Label _totalLabel;

    [Export]
    private Label _totalAmount;

    [Export]
    private Label _relicLabel;

    [Export]
    private Label _relicAmount;

    [Export]
    private Button _acceptButton;

    private int _totalBaseMoney;
    private float _perfectMulti;
    private float _placedMulti;
    private int _relicBonus;
    private int FinalMoney => (int)(_totalBaseMoney * _perfectMulti * _placedMulti) + _relicBonus;

    public delegate void FinishedHandler();
    public event FinishedHandler Finished;

    public override void _Ready()
    {
        _acceptButton.Pressed += FinishScoring;
    }

    public override void _Process(double delta)
    {
        _acceptButton.GrabFocus();
    }

    public static ScoringScreen CreateScore(Node2D parent, ScoreGuide info)
    {
        ScoringScreen result = GD.Load<PackedScene>(LoadPath).Instantiate<ScoringScreen>();
        parent.AddChild(result);
        result.GenerateScore(info);
        parent.ProcessMode = ProcessModeEnum.Disabled;

        return result;
    }

    private void GenerateScore(ScoreGuide info)
    {
        //Arbitrarily deciding on money calcs
        _totalBaseMoney = CalcTotalBase(info);

        //Multis
        _perfectMulti = 1 + (float)info.TotalPerfects / (info.TotalHits - info.TotalPlaced);
        if (float.IsNaN(_perfectMulti))
            _perfectMulti = 1;
        _placedMulti = Math.Min(1 + (float)info.TotalPlaced / 20, 2);
        _relicBonus = info.RelicBonus;
        DrawScoreLabels();
    }

    private int CalcTotalBase(ScoreGuide info)
    {
        int result = info.BaseMoney;
        result += (int)((info.EndingHealth / info.StartingHealth) * 10);
        result += StageProducer.GlobalRng.RandiRange(0, 3);
        return result;
    }

    private void DrawScoreLabels()
    {
        if (_relicBonus <= 0)
        {
            _relicLabel.Visible = false;
            _relicAmount.Visible = false;
        }
        _styleAmount.Text = $"{_totalBaseMoney}";
        _perfectsAmount.Text = $"X{_perfectMulti:0.00}";
        _placedAmount.Text = $"X{_placedMulti:0.00}";
        _relicAmount.Text = $"+{_relicBonus}";
        _totalAmount.Text = $"{FinalMoney}";
    }

    private void FinishScoring()
    {
        StageProducer.PlayerStats.Money += FinalMoney;

        //Achievement check for 1k money
        if (StageProducer.PlayerStats.Money >= 1000)
        {
            SteamWhisperer.PopAchievement("money");
        }

        Finished?.Invoke();
        QueueFree();
    }
}
