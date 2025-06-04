using System;
using FunkEngine;
using Godot;

public partial class Toriel : CanvasLayer
{
    public static string LoadPath { get; private set; } =
        "res://Scenes/BattleDirector/Tutorial/Toriel.tscn";
    private BattleDirector _currentDirector;

    [Export]
    private Label _dialogueLabel;

    [Export]
    private Control _dialogueBox;

    [Export]
    private Button _nextButton;

    [Export]
    private Sprite2D[] _inputSprites = new Sprite2D[4]; //In enum order

    [Export]
    private Marker2D _noteQueueMarker;

    [Export]
    private Marker2D _comboMarker;

    [Export]
    private Marker2D _barMarker;

    [Export]
    private Marker2D _noteMarker;

    [Export]
    private Marker2D _loopMarker;

    [Export]
    private NinePatchRect _selector;

    public static Toriel AttachNewToriel(BattleDirector director)
    {
        Toriel result = GD.Load<PackedScene>(LoadPath).Instantiate<Toriel>();
        result._currentDirector = director;
        director.AddChild(result);
        return result;
    }

    public override void _EnterTree()
    {
        UpdateInputSprites();
    }

    public override void _Process(double delta)
    {
        if (GetViewport().GuiGetFocusOwner() == null)
        {
            _nextButton?.GrabFocus();
        }
        UpdateInputSprites();
        string scheme = SaveSystem.GetConfigValue(SaveSystem.ConfigSettings.InputType).As<string>();
        if (_waitingForPlace && Input.IsActionPressed(scheme + "_arrowRight"))
        {
            GetViewport().SetInputAsHandled();
            FirstNotePlaced();
        }
    }

    private void UpdateInputSprites()
    {
        string prefix = SaveSystem.GetConfigValue(SaveSystem.ConfigSettings.InputType).ToString();
        _inputSprites[0].Texture = GD.Load<Texture2D>(
            ControlSettings.GetTextureForInput(prefix + "_arrowUp")
        );
        _inputSprites[1].Texture = GD.Load<Texture2D>(
            ControlSettings.GetTextureForInput(prefix + "_arrowDown")
        );
        _inputSprites[2].Texture = GD.Load<Texture2D>(
            ControlSettings.GetTextureForInput(prefix + "_arrowLeft")
        );
        _inputSprites[3].Texture = GD.Load<Texture2D>(
            ControlSettings.GetTextureForInput(prefix + "_arrowRight")
        );
    }

    private SceneTreeTimer _timer;

    public void NoDying()
    {
        _dialogueBox.Visible = true;
        _nextButton.Visible = false;
        _dialogueLabel.Text = Tr("TUTORIAL_NODYING");
        _timer = GetTree().CreateTimer(3);
        _timer.Timeout += () =>
        {
            _dialogueBox.Visible = false;
            _nextButton.Visible = true;
        };
    }

    #region IntroDialogues
    public void IntroDialogue()
    {
        _dialogueBox.Visible = true;
        GetTree().SetPause(true);
        _currentDirector.FocusedButton.Visible = false;
        _dialogueLabel.Text = Tr("TUTORIAL_DIALOGUE_1");
        _nextButton.GrabFocus();
        _nextButton.Pressed += Dialogue2;
    }

    public void Dialogue2()
    {
        _nextButton.Pressed -= Dialogue2;
        _dialogueLabel.Text = Tr("TUTORIAL_DIALOGUE_2");
        _nextButton.Pressed += Dialogue3;
    }

    public void Dialogue3()
    {
        _nextButton.Pressed -= Dialogue3;
        _dialogueLabel.Text = Tr("TUTORIAL_DIALOGUE_3");
        _selector.Visible = true;
        _selector.Position = _inputSprites[(int)ArrowType.Down].Position - _selector.Size / 2;
        _nextButton.Pressed += Dialogue4;
    }

    public void Dialogue4()
    {
        _nextButton.Pressed -= Dialogue4;
        _dialogueLabel.Text = Tr("TUTORIAL_DIALOGUE_4");
        _selector.Visible = false;
        _inputSprites[0].Visible = true;
        _inputSprites[1].Visible = true;
        _inputSprites[2].Visible = true;
        _inputSprites[3].Visible = true;
        _nextButton.Pressed += Dialogue5;
    }

    public void Dialogue5()
    {
        _nextButton.Pressed -= Dialogue5;
        _dialogueBox.Visible = false;
        GetTree().SetPause(false);
        _currentDirector.FocusedButton.Visible = true;
        _currentDirector.FocusedButton.GrabFocus();
    }
    #endregion

    #region Loop Dialogue
    public void LoopDialogue()
    {
        _timer?.SetTimeLeft(0);
        _dialogueBox.Visible = true;
        _nextButton.Visible = true;

        _inputSprites[0].Visible = false;
        _inputSprites[1].Visible = false;
        _inputSprites[2].Visible = false;
        _inputSprites[3].Visible = false;

        _selector.Visible = true;
        _selector.Position = _loopMarker.Position - _selector.Size / 2;

        _dialogueLabel.Text = Tr("TUTORIAL_LOOP_1");
        GetTree().SetPause(true);
        _nextButton.GrabFocus();
        _nextButton.Pressed += LoopDialogue2;
    }

    public void LoopDialogue2()
    {
        _nextButton.Pressed -= LoopDialogue2;
        _inputSprites[0].Visible = true;
        _inputSprites[1].Visible = true;
        _inputSprites[2].Visible = true;
        _inputSprites[3].Visible = true;

        _dialogueLabel.Text = Tr("TUTORIAL_LOOP_2");
        _selector.Position = _noteQueueMarker.Position - _selector.Size / 2;
        _nextButton.Pressed += LoopDialogue3;
    }

    public void LoopDialogue3()
    {
        _nextButton.Pressed -= LoopDialogue3;
        _dialogueLabel.Text = Tr("TUTORIAL_LOOP_3");
        _selector.Position = _comboMarker.Position - _selector.Size / 2;
        _nextButton.Pressed += LoopDialogue4;
    }

    public void LoopDialogue4()
    {
        _nextButton.Pressed -= LoopDialogue4;
        _dialogueLabel.Text = Tr("TUTORIAL_LOOP_4");
        _selector.Position = _barMarker.Position - _selector.Size / 2;
        _nextButton.Pressed += LoopDialogue5;
    }

    public void LoopDialogue5()
    {
        _nextButton.Pressed -= LoopDialogue5;
        _dialogueLabel.Text = Tr("TUTORIAL_LOOP_5");
        _selector.Visible = false;
        _nextButton.Pressed += LoopDialogue6;
    }

    public void LoopDialogue6()
    {
        _nextButton.Pressed -= LoopDialogue6;
        _dialogueBox.Visible = false;
        GetTree().SetPause(false);
    }
    #endregion

    #region Placed Dialogue
    public void PlaceDialogue1()
    {
        _dialogueBox.Visible = true;
        GetTree().SetPause(true);
        _dialogueLabel.Text = Tr("TUTORIAL_PLACE_1");
        _nextButton.GrabFocus();
        _selector.Visible = true;
        _selector.Position = _inputSprites[(int)ArrowType.Right].Position - _selector.Size / 2;
        _nextButton.Pressed += PlaceDialogue2;
    }

    public void PlaceDialogue2()
    {
        _nextButton.Pressed -= PlaceDialogue2;
        _dialogueLabel.Text = Tr("TUTORIAL_PLACE_2");
        _selector.Position = _noteQueueMarker.Position - _selector.Size / 2;
        _nextButton.Pressed += PlaceDialogue3;
    }

    public void PlaceDialogue3()
    {
        _nextButton.Pressed -= PlaceDialogue3;
        _dialogueLabel.Text = Tr("TUTORIAL_PLACE_3");
        _selector.Position = _inputSprites[(int)ArrowType.Right].Position - _selector.Size / 2;
        _nextButton.Pressed += PlaceDialogue4;
    }

    public void PlaceDialogue4()
    {
        _nextButton.Pressed -= PlaceDialogue4;
        _dialogueLabel.Text = Tr("TUTORIAL_PLACE_4");
        _selector.Position = _noteQueueMarker.Position - _selector.Size / 2;
        _nextButton.Pressed += PlaceDialogue5;
    }

    public void PlaceDialogue5()
    {
        _nextButton.Pressed -= PlaceDialogue5;
        _dialogueLabel.Text = Tr("TUTORIAL_PLACE_5");
        _nextButton.Pressed += PlaceDialogue6;
    }

    public void PlaceDialogue6()
    {
        _nextButton.Pressed -= PlaceDialogue6;
        _dialogueLabel.Text = Tr("TUTORIAL_PLACE_6");
        _selector.Position = _inputSprites[(int)ArrowType.Right].Position - _selector.Size / 2;
        _waitingForPlace = true;
        _nextButton.Visible = false;
    }

    private bool _waitingForPlace = false;

    public void FirstNotePlaced()
    {
        _waitingForPlace = false;
        _dialogueBox.Visible = false;
        _selector.Visible = false;
        GetTree().SetPause(false);
        _currentDirector.PlayerAddNote(ArrowType.Right, TimeKeeper.LastBeat.RoundBeat());
        _finalDialogue = true;
    }
    #endregion

    #region OnPlace Dialogue
    bool _finalDialogue = false;

    public void OnPlaceDialogue1()
    {
        if (!_finalDialogue)
            return;
        _finalDialogue = false;

        _dialogueBox.Visible = true;
        _nextButton.Visible = true;

        _dialogueLabel.Text = Tr("TUTORIAL_FINAL_1");
        GetTree().SetPause(true);
        _nextButton.GrabFocus();
        _nextButton.Pressed += OnPlaceDialogue2;
    }

    public void OnPlaceDialogue2()
    {
        _nextButton.Pressed -= OnPlaceDialogue2;
        _dialogueLabel.Text = Tr("TUTORIAL_FINAL_2");
        _selector.Visible = true;
        _selector.Position = _barMarker.Position - _selector.Size / 2;
        _nextButton.Pressed += OnPlaceDialogue3;
    }

    public bool FromBoss;

    public void OnPlaceDialogue3()
    {
        if (FromBoss)
        {
            _dialogueBox.Visible = true;
            _nextButton.Visible = true;
            GetTree().SetPause(true);
            _nextButton.GrabFocus();
        }
        else
        {
            _nextButton.Pressed -= OnPlaceDialogue3;
            _selector.Visible = false;
        }
        _dialogueLabel.Text = Tr("TUTORIAL_FINAL_3");
        _nextButton.Pressed += OnPlaceDialogue4;
    }

    public void OnPlaceDialogue4()
    {
        _nextButton.Pressed -= OnPlaceDialogue4;
        _dialogueBox.Visible = false;
        GetTree().SetPause(false);
    }
    #endregion

    #region Boss Dialogue
    public void BossDialogue()
    {
        _dialogueBox.Visible = true;
        GetTree().SetPause(true);
        _currentDirector.FocusedButton.Visible = false;
        _dialogueLabel.Text = Tr("TUTORIAL_BOSS");
        _nextButton.GrabFocus();
        _nextButton.Pressed += BossDialogueReady;
    }

    public void BossDialogueReady()
    {
        _nextButton.Pressed -= BossDialogueReady;
        _dialogueBox.Visible = false;
        GetTree().SetPause(false);
        _currentDirector.FocusedButton.Visible = true;
        _currentDirector.FocusedButton.GrabFocus();
    }
    #endregion
}
