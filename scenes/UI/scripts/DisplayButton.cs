using System;
using Godot;

public partial class DisplayButton : Button
{
    [Export]
    public Texture2D Texture;

    [Export]
    public string Description;

    [Export]
    public string DisplayName;

    public void Display(Texture2D texture, string description, string name)
    {
        Texture = texture;
        Description = description;
        DisplayName = name;
        Icon = Texture;
    }
}
