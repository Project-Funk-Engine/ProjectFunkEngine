using System;
using FunkEngine;
using Godot;

/**
 * @class Note
 * @brief Data structure class for holding data and methods for a battle time note. WIP
 */
public partial class Note : Resource, IBattleEvent
{
    public PuppetTemplate Owner;
    private int _baseVal;
    private Action<BattleDirector, int> NoteEffect;

    //public string Tooltip;

    public Note(
        PuppetTemplate owner,
        int baseVal = 1,
        Action<BattleDirector, int> noteEffect = null
    )
    {
        Owner = owner;
        NoteEffect =
            noteEffect
            ?? (
                (BD, val) =>
                {
                    BD.GetTarget(this).TakeDamage(val);
                }
            );
        _baseVal = baseVal;
    }

    public BattleEffectTrigger GetTrigger()
    {
        return BattleEffectTrigger.SelfNoteHit;
    }

    public void OnTrigger(BattleDirector BD)
    {
        NoteEffect(BD, _baseVal);
    }
}
