using System;
using FunkEngine;
using Godot;

public partial class RelicTemplate : Resource
{
    public RelicEffect[] Effects;
    public string Name;

    public Texture2D Texture;
    public string Tooltip;

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
