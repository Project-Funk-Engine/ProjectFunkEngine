using System;
using Godot;

/**
 * @class Note
 * @brief Data structure class for holding data and methods for a battle time note. WIP
 */
public partial class Note : Resource
{
    public int Beat;
    public NoteArrow.ArrowType Type;

    public Note(NoteArrow.ArrowType type = NoteArrow.ArrowType.Up, int beat = 0)
    {
        Beat = beat;
        Type = type;
    }
}
