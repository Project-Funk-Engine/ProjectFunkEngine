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

    Note(PuppetTemplate owner, Action<BattleDirector, int> noteEffect, int baseVal)
    {
        Owner = owner;
        NoteEffect = noteEffect;
        _baseVal = baseVal;
    }

    public string GetTrigger()
    {
        return "OnHit";
    }

    public void OnTrigger(BattleDirector BD)
    {
        NoteEffect(BD, _baseVal);
    }
}
