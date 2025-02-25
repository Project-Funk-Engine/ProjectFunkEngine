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

    public override void _Ready()
    {
        InitializeArrowCheckers();
        LoadControlScheme();
    }

    private void LoadControlScheme()
    {
        string scheme = ProjectSettings.HasSetting("game/input_scheme")
            ? (string)ProjectSettings.GetSetting("game/input_scheme")
            : "ARROWS";
        foreach (var arrow in Arrows)
        {
            var events = InputMap.ActionGetEvents(arrow.Key);
            foreach (var inputEvent in events)
            {
                InputMap.ActionEraseEvent(arrow.Key, inputEvent);
            }
        }

        var selectedScheme = ControlSchemes.Schemes[scheme];
        foreach (var arrow in Arrows)
        {
            if (selectedScheme.ContainsKey(arrow.Key))
            {
                InputEventKey eventKey = new InputEventKey();
                eventKey.Keycode = (Key)Enum.Parse(typeof(Key), selectedScheme[arrow.Key]);
                InputMap.ActionAddEvent(arrow.Key, eventKey);
            }
        }
    }

    public override void _Process(double delta)
    {
        foreach (var arrow in Arrows)
        {
            if (Input.IsActionJustPressed(arrow.Key))
            {
                EmitSignal(nameof(NotePressed), (int)arrow.Type);
                arrow.Node.SetPressed(true);
            }
            else if (Input.IsActionJustReleased(arrow.Key))
            {
                EmitSignal(nameof(NoteReleased), (int)arrow.Type);
                arrow.Node.SetPressed(false);
            }
        }
    }
}
