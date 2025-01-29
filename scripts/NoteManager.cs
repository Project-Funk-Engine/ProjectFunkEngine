using System.Collections.Generic;
using Godot;

public partial class NoteManager : Node2D
{
    public enum ArrowType
    {
        Up,
        Down,
        Left,
        Right,
    }

    public struct ArrowData
    {
        public Color color;
        public string key;
        public NoteChecker node;
    }

    private Dictionary<ArrowType, ArrowData> _arrows;

    private void InitializeArrowCheckers()
    {
        _arrows = new()
        {
            {
                ArrowType.Up,
                new ArrowData
                {
                    color = Colors.Green,
                    key = "arrowUp",
                    node = GetNode<NoteChecker>("arrowUp"),
                }
            },
            {
                ArrowType.Down,
                new ArrowData
                {
                    color = Colors.Aqua,
                    key = "arrowDown",
                    node = GetNode<NoteChecker>("arrowDown"),
                }
            },
            {
                ArrowType.Left,
                new ArrowData
                {
                    color = Colors.HotPink,
                    key = "arrowLeft",
                    node = GetNode<NoteChecker>("arrowLeft"),
                }
            },
            {
                ArrowType.Right,
                new ArrowData
                {
                    color = Colors.Red,
                    key = "arrowRight",
                    node = GetNode<NoteChecker>("arrowRight"),
                }
            },
        };

        //Set the color of the arrows
        foreach (var arrow in _arrows)
        {
            arrow.Value.node.SetColor(arrow.Value.color);
        }
    }

    public override void _Ready()
    {
        InitializeArrowCheckers();

        CreateNote(ArrowType.Up);
        CreateNote(ArrowType.Down);
        CreateNote(ArrowType.Left);
        CreateNote(ArrowType.Right);
    }

    public override void _Process(double delta)
    {
        HandleInput();

        //have a 20% chance of creating a note in a random lane
        if (GD.Randf() >= 0.02f)
            return;
        var randomLane = (ArrowType)
            GD.RandRange(0, ArrowType.GetValues(typeof(ArrowType)).Length - 1);
        CreateNote(randomLane);
    }

    private void HandleInput()
    {
        foreach (var arrow in _arrows)
        {
            if (Input.IsActionJustPressed(arrow.Value.key))
            {
                arrow.Value.node.SetPressed(true);
            }
            else if (Input.IsActionJustReleased(arrow.Value.key))
            {
                arrow.Value.node.SetPressed(false);
            }
        }
    }

    private void CreateNote(ArrowType arrow)
    {
        CreateNote(_arrows[arrow]);
    }

    private void CreateNote(ArrowData arrowData)
    {
        var noteScene = ResourceLoader.Load<PackedScene>("res://scripts/noteSystem/note.tscn");
        var note = noteScene.Instantiate<Note>();

        note.Init(arrowData, 1.0f, -1);

        AddChild(note);
    }
}
