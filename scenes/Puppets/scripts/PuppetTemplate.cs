using System;
using Godot;

/** Essentially a battle entity. Has HP and can be healed or damaged.
 * TODO: Look into interfaces
 */
public partial class PuppetTemplate : Node2D
{
    public delegate void DefeatedHandler(PuppetTemplate self);
    public event DefeatedHandler Defeated;

    [Export]
    protected HealthBar _healthBar;

    [Export]
    public Sprite2D Sprite;

    [Export]
    public Vector2 StartPos; //158, 126

    [Export]
    public Vector2 InitScale = Vector2.One;

    [Export]
    public bool hideHealth = false;

    protected int _maxHealth = 100;
    protected int _currentHealth = 100;

    //Stats would go here.

    public string UniqName = ""; //Eventually make subclasses/scenes/real stuff

    public override void _Ready()
    {
        _healthBar.SetHealth(_maxHealth, _currentHealth);
        Position = StartPos;
        Sprite.Scale = InitScale;

        if (hideHealth)
        {
            _healthBar.Hide();
        }
    }

    public void Init(Texture2D texture, string name)
    {
        Sprite.Texture = texture;
        UniqName = name;
    }

    public virtual void TakeDamage(int amount)
    {
        if (_currentHealth <= 0)
            return; //TEMP Only fire once.
        amount = Math.Max(0, amount); //Should not be able to heal from damage.
        _currentHealth = _healthBar.ChangeHP(-amount);
        if (_currentHealth <= 0)
        {
            Defeated?.Invoke(this);
        }

        if (amount == 0)
            return;
        TextParticle newText = new TextParticle();
        newText.Modulate = Colors.Red;
        Sprite.AddChild(newText);
        newText.Text = $"-{amount}";
    }

    public virtual void Heal(int amount)
    {
        _currentHealth = _healthBar.ChangeHP(amount);

        if (amount == 0)
            return;
        TextParticle newText = new TextParticle();
        newText.Modulate = Colors.Green;
        Sprite.AddChild(newText);
        newText.Text = $"+{amount}";
    }

    public int GetCurrentHealth()
    {
        return _currentHealth;
    }
}
