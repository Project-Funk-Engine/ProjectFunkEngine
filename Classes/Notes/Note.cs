using System;
using FunkEngine;
using Godot;

/**
 * @class Note
 * @brief Data structure class for holding data and methods for a battle time note. WIP
 */
public partial class Note : Resource
{
    public PuppetTemplate Owner;
    public string Name;
    private int _baseVal;
    private Action<BattleDirector, Note, Timing> NoteEffect; //TODO: Where/How to deal with timing.

    //public string Tooltip;

    public Note(
        string name,
        PuppetTemplate owner = null,
        int baseVal = 1,
        Action<BattleDirector, Note, Timing> noteEffect = null
    )
    {
        Name = name;
        Owner = owner;
        NoteEffect =
            noteEffect
            ?? (
                (BD, source, Timing) =>
                {
                    BD.GetTarget(this).TakeDamage(source._baseVal);
                }
            );
        _baseVal = baseVal;
    }

    public void OnHit(BattleDirector BD, Timing timing)
    {
        NoteEffect(BD, this, timing);
    }

    public Note Clone()
    {
        return (Note)MemberwiseClone();
    }
}
