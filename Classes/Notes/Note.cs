using System;
using FunkEngine;
using Godot;

/**
 * <summary>Note: Data structure class for holding data and methods for a battle time note.</summary>
 */
public partial class Note : Resource, IDisplayable
{
    public PuppetTemplate Owner;
    public int Id;
    public string Name { get; set; }
    private int _baseVal;
    public float CostModifier { get; private set; }
    private Action<BattleDirector, Note, Timing> NoteEffect;

    public const double TimingMax = 0.5d; //The max range for a note to be timed is its beat +/- this const

    public string Tooltip { get; set; }
    public Texture2D Texture { get; set; }

    public Note(
        int id,
        string name,
        string tooltip,
        Texture2D texture = null,
        PuppetTemplate owner = null,
        int baseVal = 1,
        Action<BattleDirector, Note, Timing> noteEffect = null,
        float costModifier = 1.0f
    )
    {
        Id = id;
        Name = name;
        Owner = owner;
        NoteEffect =
            noteEffect
            ?? (
                (BD, source, timing) =>
                {
                    BD.GetTarget(this).TakeDamage((int)timing * source._baseVal);
                }
            );
        _baseVal = baseVal;
        Texture = texture;
        Tooltip = tooltip;
        CostModifier = costModifier;
    }

    public void OnHit(BattleDirector BD, Timing timing)
    {
        NoteEffect(BD, this, timing);
    }

    public Note Clone()
    {
        //Eventually could look into something more robust, but for now shallow copy is preferable.
        //We only would want val and name to be copied by value
        Note newNote = new Note(
            Id,
            Name,
            Tooltip,
            Texture,
            Owner,
            _baseVal,
            NoteEffect,
            CostModifier
        );
        return newNote;
    }

    public bool IsPlayerNote()
    {
        return Name.Contains("Player");
    }

    public int GetBaseVal()
    {
        return _baseVal;
    }
}
