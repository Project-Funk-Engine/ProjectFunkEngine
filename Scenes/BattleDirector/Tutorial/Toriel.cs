using System;
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

    public void IntroDialogue()
    {
        _dialogueBox.Visible = true;
        GetTree().SetPause(true);
        _currentDirector.FocusedButton.Visible = false;
        _dialogueLabel.Text = Tr("TUTORIAL_DIALOGUE_1");
        _nextButton.GrabFocus();
    }
}
