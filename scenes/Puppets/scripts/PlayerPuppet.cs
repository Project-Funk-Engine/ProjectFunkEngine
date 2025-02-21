using System;
using Godot;

public partial class PlayerPuppet : PuppetTemplate
{
    public PlayerStats Stats;

    public override void _Ready()
    {
        Stats = StageProducer.PlayerStats;

        _currentHealth = Stats.CurrentHealth;
        _maxHealth = Stats.MaxHealth;

        UniqName = "Player";
        base._Ready();
    }

    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount);
        Stats.CurrentHealth = _currentHealth;
    }

    public override void Heal(int amount)
    {
        base.Heal(amount);
        Stats.CurrentHealth = _currentHealth;
    }
}
