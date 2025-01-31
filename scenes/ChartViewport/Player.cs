using System;
using Godot;

public partial class Player : Node2D
{
    [Export]
    public HealthBar PlayerHealthBar;

    [Export]
    public BattleDirector BattleDirectorInstance;
    private int _playerHealthMax = 100;
    private int _playerHealth = 100;

    public override void _Ready()
    {
        if (PlayerHealthBar != null)
        {
            PlayerHealthBar.SetHealth(_playerHealthMax, _playerHealth);
        }

        if (BattleDirectorInstance != null)
        {
            BattleDirectorInstance.Connect(
                (nameof(BattleDirectorInstance.PlayerDamage)),
                new Callable(this, nameof(PlayerTakeDamage))
            );
        }
    }

    private void PlayerTakeDamage(int damage)
    {
        GD.Print("Player taking damage: " + damage);
        PlayerHealthBar.TakeDamage(damage);
    }
}
