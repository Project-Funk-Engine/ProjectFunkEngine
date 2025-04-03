using FunkEngine;
using Godot;

/**
 * <summary>RelicTemplate: Generic class representing a player passive relic.</summary>
 */
public partial class RelicTemplate : Resource, IDisplayable
{
    public readonly RelicEffect[] Effects;
    public int Id;
    public string Name { get; set; }

    public Texture2D Texture { get; set; }
    public string Tooltip { get; set; }

    public RelicTemplate(
        int id,
        string name = "",
        string tooltip = "",
        Texture2D texture = null,
        RelicEffect[] effectTags = null
    )
    {
        Id = id;
        Effects = effectTags;
        Name = name;
        Tooltip = tooltip;
        Texture = texture;
    }

    public RelicTemplate Clone()
    {
        RelicTemplate newRelic = new RelicTemplate(Id, Name, Tooltip, Texture, Effects);
        return newRelic;
    }
}
