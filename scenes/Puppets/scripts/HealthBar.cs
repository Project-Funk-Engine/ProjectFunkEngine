using System;
using Godot;

/**
 * <summary>Generic health bar, instantiate from scene.</summary>
 */
public partial class HealthBar : TextureProgressBar
{
    public override void _Ready()
    {
        Value = MaxValue;
    }

    //initializes health
    public void SetHealth(int max, int current)
    {
        MaxValue = max;
        Value = current;
    }

    //For effects changes max hp, and changes hp by a similar amount
    public void ChangeMax(int change)
    {
        MaxValue += change;
        Value += change;
    }

    //Changes hp value, for damage or heal, returns resulting hp.
    public int ChangeHP(int amount)
    {
        Value += amount;
        return (int)Value;
    }
}
