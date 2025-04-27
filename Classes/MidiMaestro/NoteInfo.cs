using System;
using Godot;

public partial class NoteInfo : Resource
{
    [Export]
    public float Beat;

    [Export]
    public float Length;

    public NoteInfo Create(float beat = 0, float len = 0)
    {
        Beat = beat;
        Length = len;
        return this;
    }
}
