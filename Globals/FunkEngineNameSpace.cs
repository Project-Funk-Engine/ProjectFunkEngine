using Godot;

namespace FunkEngine;

public enum ArrowType
{
    Up = 0,
    Down = 1,
    Left = 2,
    Right = 3,
}

public struct ArrowData
{
    public Color Color;
    public string Key;
    public NoteChecker Node;
    public ArrowType Type;
}
