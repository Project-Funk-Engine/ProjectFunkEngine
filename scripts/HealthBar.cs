using Godot;
using System;

public partial class HealthBar : Control
{
    const int MAX_HEALTH = 100; 
    int health = MAX_HEALTH;

    [Export] public ProgressBar PlayerHealthBar;
    //we can change this to a Texture Progress bar once we have art assets for it



    public override void _Ready()
    {
        if (PlayerHealthBar != null)
        {
            GD.Print("Player Health Bar");
            PlayerHealthBar.MaxValue = MAX_HEALTH;
        }
    }

    private void _updateHealthBar()
    {
        PlayerHealthBar.Value = health;
    }

    private void _takeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            GD.Print("We are dead");
        }
        _updateHealthBar();
    }

    public override void _Input(InputEvent @event)
    {
        if(@event.IsActionPressed("ui_accept"))
                _takeDamage(10);
          
    }
    
    
    
    
    
}
