using System;
using Godot;

/** Essentially a battle entity. Has HP and can be healed or damaged.
 * TODO: Look into interfaces
 */
public partial class Puppet_Template : Node2D
{
    private HealthBar _healthBar;
    public Sprite2D Sprite = new Sprite2D();

    private int _maxHealth = 100;
    private int _currentHealth = 100;

    //Stats would go here.

    public string UniqName = ""; //Eventually make subclasses/scenes/real stuff

    public override void _Ready()
    {
        _healthBar = GD.Load<PackedScene>("res://scenes/Puppets/HealthBar.tscn")
            .Instantiate<HealthBar>();
        AddChild(_healthBar);

        Sprite.Position = new Vector2(75, 86);
        AddChild(Sprite); //TODO: DECIDE Whether to replace with packedscenes/robust subclasses

        _healthBar.SetHealth(_maxHealth, _currentHealth);
    }

    public void Init(Texture2D texture, string name)
    {
        Sprite.Texture = texture;
        UniqName = name;
    }

    public void TakeDamage(int amount)
    {
        _healthBar.ChangeHP(-amount);
    }

    public void Heal(int amount)
    {
        _healthBar.ChangeHP(amount);
    }
}
