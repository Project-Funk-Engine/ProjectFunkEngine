using System;
using System.Collections.Generic;
using Godot;
using ArrowType = NoteArrow.ArrowType;

/**
 * @class NoteManager
 * @brief Note Manager to handle input, timing and lower level data processing for notes. Should signal timing info to Battle Director for HP management. WIP
 */
public partial class NoteManager : Node2D
{
    //TODO: Put in a Global/Somewhere it makes sense
    [Signal]
    public delegate void NotePressedEventHandler(ArrowType arrowType);

    [Signal]
    public delegate void NoteReleasedEventHandler(ArrowType arrowType);

    public struct ArrowData
    {
        public Color Color;
        public string Key;
        public NoteChecker Node;
    }

    public Dictionary<ArrowType, ArrowData> Arrows;

    private void InitializeArrowCheckers()
    {
        Arrows = new()
        {
            {
                ArrowType.Up,
                new ArrowData
                {
                    Color = Colors.Green,
                    Key = "arrowUp",
                    Node = GetNode<NoteChecker>("noteCheckers/arrowUp"),
                }
            },
            {
                ArrowType.Down,
                new ArrowData
                {
                    Color = Colors.Aqua,
                    Key = "arrowDown",
                    Node = GetNode<NoteChecker>("noteCheckers/arrowDown"),
                }
            },
            {
                ArrowType.Left,
                new ArrowData
                {
                    Color = Colors.HotPink,
                    Key = "arrowLeft",
                    Node = GetNode<NoteChecker>("noteCheckers/arrowLeft"),
                }
            },
            {
                ArrowType.Right,
                new ArrowData
                {
                    Color = Colors.Red,
                    Key = "arrowRight",
                    Node = GetNode<NoteChecker>("noteCheckers/arrowRight"),
                }
            },
        };

        //Set the color of the arrows
        foreach (var arrow in Arrows)
        {
            arrow.Value.Node.SetColor(arrow.Value.Color);
        }
    }

    public override void _Ready()
    {
        InitializeArrowCheckers();
    }

    public override void _Process(double delta)
    {
        foreach (var arrow in Arrows)
        {
            if (Input.IsActionJustPressed(arrow.Value.Key))
            {
                arrow.Value.Node.SetPressed(true);
                GD.Print(arrow.Value.Key);
                EmitSignal(nameof(NotePressed), arrow.Value.Key);
            }
            else if (Input.IsActionJustReleased(arrow.Value.Key))
            {
                arrow.Value.Node.SetPressed(false);
                EmitSignal(nameof(NoteReleased), arrow.Value.Key);
            }
        }
    }
}
