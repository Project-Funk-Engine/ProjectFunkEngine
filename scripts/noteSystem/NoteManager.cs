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
                    node = GetNode<NoteChecker>("noteCheckers/arrowUp"),
                }
            },
            {
                ArrowType.Down,
                new ArrowData
                {
                    color = Colors.Aqua,
                    key = "arrowDown",
                    node = GetNode<NoteChecker>("noteCheckers/arrowDown"),
                }
            },
            {
                ArrowType.Left,
                new ArrowData
                {
                    color = Colors.HotPink,
                    key = "arrowLeft",
                    node = GetNode<NoteChecker>("noteCheckers/arrowLeft"),
                }
            },
            {
                ArrowType.Right,
                new ArrowData
                {
                    color = Colors.Red,
                    key = "arrowRight",
                    node = GetNode<NoteChecker>("noteCheckers/arrowRight"),
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
    }

    public override void _Process(double delta)
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

    public void CreateNote(ArrowType arrow)
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
