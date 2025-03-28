using System;
using System.Collections.Generic;
using FunkEngine;
using Godot;

/**
 * @class InputHandler
 * @brief InputHandler to handle input, and manage note checkers. WIP
 */
public partial class InputHandler : Node2D
{
    [Signal]
    public delegate void NotePressedEventHandler(ArrowType arrowType);

    [Signal]
    public delegate void NoteReleasedEventHandler(ArrowType arrowType);

    public ArrowData[] Arrows = new ArrowData[]
    {
        new ArrowData()
        {
            Color = Colors.Green,
            Key = "arrowUp",
            Type = ArrowType.Up,
        },
        new ArrowData()
        {
            Color = Colors.Aqua,
            Key = "arrowDown",
            Type = ArrowType.Down,
        },
        new ArrowData()
        {
            Color = Colors.HotPink,
            Key = "arrowLeft",
            Type = ArrowType.Left,
        },
        new ArrowData()
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

    public void FeedbackEffect(ArrowType arrow, string text)
    {
        // Get the particle node for this arrow
        var particles = Arrows[(int)arrow].Node.Particles;

        // Set the particle amount based on timing
        int particleAmount;
        switch (text)
        {
            case "Perfect":
                particleAmount = 10;
                break;
            case "Great":
                particleAmount = 7;
                break;
            case "Good":
                particleAmount = 4;
                break;
            default:
                return;
        }

        particles.Emit(particleAmount);
    }

    public override void _Ready()
    {
        InitializeArrowCheckers();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventJoypadButton)
        { //Force Controller if controller was pressed
            SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.InputKey, "CONTROLLER");
        }
    }

    public override void _Process(double delta)
    {
        //TODO: Add chamge control scheme signal, so we don't query each frame.
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
}
