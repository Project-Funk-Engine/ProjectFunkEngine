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

    public Rarity Rarity { get; set; }

    public RelicTemplate(
        int id,
        string name = "",
        Rarity rarity = Rarity.Common,
        Texture2D texture = null,
        RelicEffect[] effectTags = null
    )
    {
        Id = id;
        Effects = effectTags;
        Name = name;
        Texture = texture;
        Rarity = rarity;
    }

    public RelicTemplate Clone()
    {
        RelicTemplate newRelic = new RelicTemplate(Id, Name, Rarity, Texture, Effects);
        return newRelic;
    }
}
