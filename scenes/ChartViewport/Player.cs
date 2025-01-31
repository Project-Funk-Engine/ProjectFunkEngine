using System;
using Godot;

public partial class Player : Node2D
{
    [Export]
    public HealthBar PlayerHealthBar;

    private int _playerHealthMax = 100;
    private int _playerHealth = 100;

    public override void _Ready()
    {
        if (PlayerHealthBar != null)
        {
            PlayerHealthBar.SetHealth(_playerHealthMax, _playerHealth);
        }
    }

    public void PlayerTakeDamage(int damage)
    {
        GD.Print("Player taking damage: " + damage);
        PlayerHealthBar.TakeDamage(damage);
    }
}
