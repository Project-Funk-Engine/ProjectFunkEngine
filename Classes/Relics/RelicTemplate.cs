using System;
using FunkEngine;
using Godot;

public partial class RelicTemplate : Resource
{
    public RelicEffect[] Effects;
    public string Name;

    //public Texture2D Texture
    //public string Tooltip
    public RelicTemplate(string Name = "", RelicEffect[] EffectTags = null)
    {
        Effects = EffectTags;
        this.Name = Name;
    }
}
