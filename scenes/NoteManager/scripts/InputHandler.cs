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
    }

    public override void _Process(double delta)
    {
        //TODO: Add change control scheme signal, so we don't query each frame.
        string scheme = SaveSystem.GetConfigValue(SaveSystem.ConfigSettings.InputKey).As<string>();
        if (Input.GetConnectedJoypads().Count <= 0 && scheme == "CONTROLLER")
        {
            SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.InputKey, "ARROWS");
        }

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
            SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.InputKey, "CONTROLLER");
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
                particleAmount = 10;
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
}
