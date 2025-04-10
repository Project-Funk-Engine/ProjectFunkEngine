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

    public Rarity Rarity { get; set; }

    public RelicTemplate(
        int id,
        string name = "",
        string tooltip = "",
        Rarity rarity = Rarity.Common,
        Texture2D texture = null,
        RelicEffect[] effectTags = null
    )
    {
        Id = id;
        Effects = effectTags;
        Name = name;
        Tooltip = tooltip;
        Texture = texture;
        Rarity = rarity;
    }

    public RelicTemplate Clone()
    {
        RelicTemplate newRelic = new RelicTemplate(Id, Name, Tooltip, Rarity, Texture, Effects);
        return newRelic;
    }
}
