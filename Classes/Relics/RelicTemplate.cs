using System;
using FunkEngine;
using Godot;

public partial class RelicTemplate : Resource, IDisplayable
{
    public RelicEffect[] Effects;
    public string Name { get; set; }

    public Texture2D Texture { get; set; }
    public string Tooltip { get; set; }

    public RelicTemplate(
        string name = "",
        string tooltip = "",
        Texture2D texture = null,
        RelicEffect[] EffectTags = null
    )
    {
        Effects = EffectTags;
        Name = name;
        Tooltip = tooltip;
        Texture = texture;
    }

    public RelicTemplate Clone()
    {
        RelicTemplate newRelic = new RelicTemplate(Name, Tooltip, Texture, Effects);
        return newRelic;
    }
}
