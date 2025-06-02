using FunkEngine;
using Godot;

/**
 * <summary>InputHandler to handle note checkers, and bubble input events upwards.</summary>
 */
public partial class InputHandler : Node2D
{
    [Signal]
    public delegate void NotePressedEventHandler(ArrowType arrowType);

    [Signal]
    public delegate void NoteReleasedEventHandler(ArrowType arrowType);

    [Export]
    public Sprite2D leftArrowSprite;

    [Export]
    public Sprite2D leftArrowOutlineSprite;

    [Export]
    public Sprite2D upArrowSprite;

    [Export]
    public Sprite2D upArrowOutlineSprite;

    [Export]
    public Sprite2D downArrowSprite;

    [Export]
    public Sprite2D downArrowOutlineSprite;

    [Export]
    public Sprite2D rightArrowSprite;

    [Export]
    public Sprite2D rightArrowOutlineSprite;

    private static readonly string _arrowFolderPath = "res://Scenes/NoteManager/Assets/";

    public readonly CheckerData[] Arrows = new CheckerData[]
    {
        new CheckerData()
        {
            Color = Colors.Green,
            Key = "arrowUp",
            Type = ArrowType.Up,
        },
        new CheckerData()
        {
            Color = Colors.Aqua,
            Key = "arrowDown",
            Type = ArrowType.Down,
        },
        new CheckerData()
        {
            Color = Colors.HotPink,
            Key = "arrowLeft",
            Type = ArrowType.Left,
        },
        new CheckerData()
        {
            Color = Colors.Red,
            Key = "arrowRight",
            Type = ArrowType.Right,
        },
    };

    private void InitializeArrowCheckers()
    {
        //Set the color of the arrows
        for (int i = 0; i < Arrows.Length; i++)
        {
            Arrows[i].Node = GetNode<NoteChecker>("noteCheckers/" + Arrows[i].Key);
            Arrows[i].Node.SetColor(Arrows[i].Color);
        }
    }

    public override void _Ready()
    {
        InitializeArrowCheckers();
        UpdateArrowSprites();
    }

    public override void _Process(double delta)
    {
        //TODO: Add change control scheme signal, so we don't query each frame.
        string scheme = SaveSystem.GetConfigValue(SaveSystem.ConfigSettings.InputType).As<string>();
        if (Input.GetConnectedJoypads().Count <= 0 && scheme == "CONTROLLER")
        {
            SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.InputType, "WASD");
        }

        if (BattleDirector.PlayerDisabled)
            return;
        foreach (var arrow in Arrows)
        {
            if (Input.IsActionJustPressed(scheme + "_" + arrow.Key))
            {
                EmitSignal(nameof(NotePressed), (int)arrow.Type);
                arrow.Node.SetPressed(true);
            }
            else if (Input.IsActionJustReleased(scheme + "_" + arrow.Key))
            {
                EmitSignal(nameof(NoteReleased), (int)arrow.Type);
                arrow.Node.SetPressed(false);
            }
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventJoypadButton)
        { //Force Controller if controller was pressed
            SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.InputType, "CONTROLLER");
        }
    }

    public void FeedbackEffect(ArrowType arrow, Timing timed)
    {
        // Get the particle node for this arrow
        var particles = Arrows[(int)arrow].Node.Particles;

        // Set the particle amount based on timing
        int particleAmount;
        switch (timed)
        {
            case Timing.Perfect:
                particleAmount = 30;
                break;
            case Timing.Good:
                particleAmount = 7;
                break;
            case Timing.Okay:
                particleAmount = 4;
                break;
            default:
                return;
        }

        particles.Emit(particleAmount);
    }

    private void UpdateArrowSprites()
    {
        if (SaveSystem.GetConfigValue(SaveSystem.ConfigSettings.TypeIsArrow).As<bool>())
        {
            leftArrowSprite.Texture = GD.Load<Texture2D>(_arrowFolderPath + "New_Arrow.png");
            leftArrowOutlineSprite.Texture = GD.Load<Texture2D>(
                _arrowFolderPath + "Arrow_Outline.png"
            );
            leftArrowSprite.RotationDegrees = 180f;
            upArrowSprite.Texture = GD.Load<Texture2D>(_arrowFolderPath + "New_Arrow.png");
            upArrowOutlineSprite.Texture = GD.Load<Texture2D>(
                _arrowFolderPath + "Arrow_Outline.png"
            );
            upArrowSprite.RotationDegrees = 270f;
            downArrowSprite.Texture = GD.Load<Texture2D>(_arrowFolderPath + "New_Arrow.png");
            downArrowOutlineSprite.Texture = GD.Load<Texture2D>(
                _arrowFolderPath + "Arrow_Outline.png"
            );
            downArrowSprite.RotationDegrees = 90f;
            rightArrowSprite.Texture = GD.Load<Texture2D>(_arrowFolderPath + "New_Arrow.png");
            rightArrowOutlineSprite.Texture = GD.Load<Texture2D>(
                _arrowFolderPath + "Arrow_Outline.png"
            );
        }
        else
        {
            leftArrowSprite.Texture = GD.Load<Texture2D>(_arrowFolderPath + "Note.png");
            leftArrowOutlineSprite.Texture = GD.Load<Texture2D>(
                _arrowFolderPath + "LeftNoteOutline.png"
            );
            leftArrowSprite.RotationDegrees = 0f;
            upArrowSprite.Texture = GD.Load<Texture2D>(_arrowFolderPath + "Note.png");
            upArrowOutlineSprite.Texture = GD.Load<Texture2D>(
                _arrowFolderPath + "TopNoteOutline.png"
            );
            upArrowSprite.RotationDegrees = 0f;
            downArrowSprite.Texture = GD.Load<Texture2D>(_arrowFolderPath + "Note.png");
            downArrowOutlineSprite.Texture = GD.Load<Texture2D>(
                _arrowFolderPath + "BottomNoteOutline.png"
            );
            downArrowSprite.RotationDegrees = 0f;
            rightArrowSprite.Texture = GD.Load<Texture2D>(_arrowFolderPath + "Note.png");
            rightArrowOutlineSprite.Texture = GD.Load<Texture2D>(
                _arrowFolderPath + "RightNoteOutline.png"
            );
        }
    }
}
