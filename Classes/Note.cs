using System;
using FunkEngine;
using Godot;

/**
 * @class Note
 * @brief Data structure class for holding data and methods for a battle time note. WIP
 */
public partial class Note : Resource
{
    private string _effect;

    //public Puppet_Template Owner;

    public Note(string effect = "")
    {
        _effect = effect;
    }

    public string GetEffect()
    {
        return _effect;
    }
}
