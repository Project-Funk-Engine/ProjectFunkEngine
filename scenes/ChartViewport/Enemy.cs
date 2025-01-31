using System;
using Godot;

public partial class Enemy : Node2D
{
    [Export]
    public HealthBar EnemyHealthBar;

    private int _enemyMaxHealth = 100;

    public override void _Ready()
    {
        if (EnemyHealthBar != null)
        {
            EnemyHealthBar.SetHealth(_enemyMaxHealth, _enemyMaxHealth);
        }
    }

    public void EnemyTakeDamage(int damage)
    {
        EnemyHealthBar.TakeDamage(damage);
        GD.Print("Enemy takes damage:" + damage);
    }
}
