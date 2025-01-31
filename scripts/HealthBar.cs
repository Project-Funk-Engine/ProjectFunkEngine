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
            GD.Print("Player Health Bar");
            PlayerHealthBar.MaxValue = MaxHealth;
        }
        //Connect(nameof(BattleDirector.PlayerDamage), new Callable(this ,nameof(PlayerDamage)));
    }

    public void SetHealth(int max, int current)
    {
        PlayerHealthBar.MaxValue = max;
        PlayerHealthBar.Value = current;
    }

    private void _updateHealthBar()
    {
        PlayerHealthBar.Value = _health;
    }

    public void TakeDamage(int damage)
    {
        _health -= damage;
        GD.Print("Health: " + _health);
        if (_health <= 0)
        {
            GD.Print("You are dead");
        }
        _updateHealthBar();
    }

    /*public override void _Input(InputEvent @event)
    {
        if(@event.IsActionPressed("ui_accept"))
                _takeDamage(10);
          
    }*/
}
