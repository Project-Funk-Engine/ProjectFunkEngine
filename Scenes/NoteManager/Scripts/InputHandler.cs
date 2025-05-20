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

    public static bool UseArrows = false;

    private void InitializeArrowCheckers()
    {
        //Set the color of the arrows
        for (int i = 0; i < Arrows.Length; i++)
        {
            Arrows[i].Node = GetNode<NoteChecker>("noteCheckers/" + Arrows[i].Key);
            Arrows[i].Node.SetColor(Arrows[i].Color);

            int arrowIndex = i;
            Arrows[i].Node.InputButton.ButtonDown += () =>
            {
                EmitSignal(nameof(NotePressed), (int)Arrows[arrowIndex].Type);
                Arrows[arrowIndex].Node.SetPressed(true);
            };
            Arrows[i].Node.InputButton.ButtonUp += () =>
            {
                EmitSignal(nameof(NoteReleased), (int)Arrows[arrowIndex].Type);
                Arrows[arrowIndex].Node.SetPressed(false);
            };
        }
    }

    public override void _Ready()
    {
        InitializeArrowCheckers();
        UpdateArrowSprites();
        if (!BattleDirector.VerticalScroll)
            return;
        foreach (CheckerData data in Arrows)
            data.Node.RotationDegrees += 90f;
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

    private static readonly string ArrowFolderPath = "res://Scenes/NoteManager/Assets/";

    private void UpdateArrowSprites()
    {
        if (!UseArrows)
            return;
        Arrows[(int)ArrowType.Left].Node.Texture = GD.Load<Texture2D>(
            ArrowFolderPath + "New_Arrow.png"
        );
        Arrows[(int)ArrowType.Left].Node.Outline.Texture = GD.Load<Texture2D>(
            ArrowFolderPath + "Arrow_Outline.png"
        );
        Arrows[(int)ArrowType.Left].Node.RotationDegrees = 180f;

        Arrows[(int)ArrowType.Up].Node.Texture = GD.Load<Texture2D>(
            ArrowFolderPath + "New_Arrow.png"
        );
        Arrows[(int)ArrowType.Up].Node.Outline.Texture = GD.Load<Texture2D>(
            ArrowFolderPath + "Arrow_Outline.png"
        );
        Arrows[(int)ArrowType.Up].Node.RotationDegrees = 270f;

        Arrows[(int)ArrowType.Down].Node.Texture = GD.Load<Texture2D>(
            ArrowFolderPath + "New_Arrow.png"
        );
        Arrows[(int)ArrowType.Down].Node.Outline.Texture = GD.Load<Texture2D>(
            ArrowFolderPath + "Arrow_Outline.png"
        );
        Arrows[(int)ArrowType.Down].Node.RotationDegrees = 90f;

        Arrows[(int)ArrowType.Right].Node.Texture = GD.Load<Texture2D>(
            ArrowFolderPath + "New_Arrow.png"
        );
        Arrows[(int)ArrowType.Right].Node.Outline.Texture = GD.Load<Texture2D>(
            ArrowFolderPath + "Arrow_Outline.png"
        );
    }
}
