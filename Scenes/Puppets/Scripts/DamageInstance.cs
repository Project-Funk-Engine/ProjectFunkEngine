using System;
using Godot;

/// <summary>
/// A class to transmit damage with the source of damage and intended target.
/// Source should never be null, null target represents an indirect source of damage.
/// </summary>
public class DamageInstance
{
    public int Damage { get; private set; }
    public PuppetTemplate Target { get; private set; }
    public PuppetTemplate Source { get; private set; }

    public DamageInstance(int damage, PuppetTemplate source, PuppetTemplate target)
    {
        Damage = damage;
        Source = source;
        Target = target;
    }

    public DamageInstance ModifyDamage(int increase, int multiplier = 1)
    {
        Damage = (Damage + increase) * multiplier;
        return this;
    }
}
