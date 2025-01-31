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
    [Signal]
    public delegate void NotePressedEventHandler(ArrowType arrowType);

    [Signal]
    public delegate void NoteReleasedEventHandler(ArrowType arrowType);

    //TODO: Put in a Global/Somewhere it makes sense
    public struct ArrowData
    {
        public Color Color;
        public string Key;
        public NoteChecker Node;
        public ArrowType Type;
    }

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
