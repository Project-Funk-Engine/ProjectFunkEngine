using System;
using Godot;

public partial class Enemy : Node2D
{
    [Export]
    public HealthBar EnemyHealthBar;

    [Export]
    public BattleDirector BattleDirectorInstance;

    private int _enemyMaxHealth = 100;

    public override void _Ready()
    {
        if (EnemyHealthBar != null)
        {
            EnemyHealthBar.SetHealth(_enemyMaxHealth, _enemyMaxHealth);
        }

        if (BattleDirectorInstance != null)
        {
            BattleDirectorInstance.Connect(
                (nameof(BattleDirectorInstance.EnemyDamage)),
                new Callable(this, nameof(EnemyTakeDamage))
            );
        }
    }

    private void EnemyTakeDamage(int damage)
    {
        EnemyHealthBar.TakeDamage(damage);
        GD.Print("Enemy takes damage:" + damage);
    }
}
