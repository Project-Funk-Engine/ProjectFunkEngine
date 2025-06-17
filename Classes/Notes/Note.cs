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
    public Targetting TargetType { get; private set; }
    private Action<BattleDirector, Note, Timing> NoteEffect;

    public const double TimingMax = 0.5d; //The max range for a note to be timed is its beat +/- this const
    public Texture2D Texture { get; set; }

    public string Description;

    public Note(
        int id,
        string name,
        Texture2D texture = null,
        int baseVal = 1,
        Action<BattleDirector, Note, Timing> noteEffect = null,
        float costModifier = 1.0f,
        Targetting targetType = Targetting.First,
        string description = null
    )
    {
        Id = id;
        Name = name;
        NoteEffect = noteEffect;
        _baseVal = baseVal;
        Texture = texture;
        CostModifier = costModifier;
        TargetType = targetType;
        Description = description;
    }

    public void OnHit(BattleDirector BD, Timing timing)
    {
        NoteEffect(BD, this, timing);
    }

    public Note SetOwner(PuppetTemplate owner)
    {
        Owner = owner;
        return this;
    }

    public Note Clone()
    {
        //Eventually could look into something more robust, but for now shallow copy is preferable.
        //We only would want val and name to be copied by value
        Note newNote = new Note(Id, Name, Texture, _baseVal, NoteEffect, CostModifier, TargetType);
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

    public void SetBaseVal(int val)
    {
        _baseVal = val;
    }
}
