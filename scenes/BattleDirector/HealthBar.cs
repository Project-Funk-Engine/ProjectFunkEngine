using System;
using Godot;

public partial class HealthBar : Control
{
    const int MaxHealth = 100;
    int _health = MaxHealth;

    [Export]
    public ProgressBar PlayerHealthBar;
    //we can change this to a Texture Progress bar once we have art assets for it

    
    public override void _Ready()
    {
        if (PlayerHealthBar != null)
        {
            SetHealth(MaxHealth, MaxHealth);
        }
    }

    public void SetHealth(int max, int current)
    {
        PlayerHealthBar.MaxValue = max;
        PlayerHealthBar.Value = current;
        _updateHealthBar();
    }

    private void _updateHealthBar()
    {
        PlayerHealthBar.Value = _health;
    }

    public void TakeDamage(int damage)
    {
        _health -= damage;
        if (_health <= 0)
        {
            GD.Print("You are dead");
        }
        _updateHealthBar();
    }
}
