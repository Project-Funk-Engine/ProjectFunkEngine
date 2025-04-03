using System;
using Godot;

/**
 *<summary>Base class for battle entity. Manages sprite, health, and initial conditions.</summary>
 */
public partial class PuppetTemplate : Node2D
{
    public static readonly string LoadPath = "";
    public delegate void DefeatedHandler(PuppetTemplate self);
    public event DefeatedHandler Defeated;

    [Export]
    protected HealthBar HealthBar;

    [Export]
    public Sprite2D Sprite;

    [Export]
    public Vector2 StartPos; //158, 126

    [Export]
    public Vector2 InitScale = Vector2.One;

    [Export]
    public bool HideHealth;

    protected int MaxHealth = 100;
    protected int CurrentHealth = 100;

    //Stats would go here.

    protected string UniqName = ""; //Eventually make subclasses/scenes/real stuff

    public override void _Ready()
    {
        HealthBar.SetHealth(MaxHealth, CurrentHealth);
        Position = StartPos;
        Sprite.Scale = InitScale;

        HealthBar.Visible = !HideHealth;
    }

    public override void _Process(double delta)
    {
        ProcessShake(delta);
    }

    private void Init(Texture2D texture, string name)
    {
        Sprite.Texture = texture;
        UniqName = name;
    }

    #region DamageAnim
    //Juice - https://www.youtube.com/watch?v=LGt-jjVf-ZU
    private int _limiter;
    private const float BaseShake = 100f;
    private float _shakeFade = 10f;
    private float _shakeStrength;
    private float _baseAnimDuration = 0.1f;

    private void ProcessShake(double delta)
    {
        _limiter = (_limiter + 1) % 3;
        if (_limiter != 1)
            return;

        if (_shakeStrength > 0)
        {
            _shakeStrength = (float)Mathf.Lerp(_shakeStrength, 0, _shakeFade * delta);
        }
        Position =
            StartPos
            + new Vector2(
                (float)GD.RandRange(-_shakeStrength, _shakeStrength),
                (float)GD.RandRange(-_shakeStrength, _shakeStrength)
            );
    }

    protected virtual void DamageAnimate(int amount)
    { //TODO: Make animate in time with bpm
        float damageAnimDir = (GetViewportRect().Size / 2 - Position).Normalized().X;
        float scalar = (float)amount / MaxHealth;
        _shakeStrength = (scalar * BaseShake);

        Color flashColor = Colors.White * 99; //White = neutral modulate, very white is higher contrast white
        if (amount < 0) //If healing
        {
            flashColor = Colors.Green;
            damageAnimDir = 0;
            _shakeStrength = 0;
        }

        var tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Spring);
        tween.TweenProperty(this, "modulate", flashColor, _baseAnimDuration);
        tween.Chain().TweenProperty(this, "modulate", Colors.White, _baseAnimDuration);
        tween.Parallel();
        tween
            .TweenProperty(
                this,
                "position:x",
                -(damageAnimDir * 2) * (2 + scalar),
                _baseAnimDuration
            )
            .AsRelative();
        tween.Chain().TweenProperty(this, "position", StartPos, 2 * _baseAnimDuration);
    }
    #endregion

    public virtual void TakeDamage(int amount)
    {
        amount = Math.Max(0, amount); //Should not be able to heal from damage.
        if (CurrentHealth <= 0 || amount == 0)
            return; //Only check if hp would change
        CurrentHealth = HealthBar.ChangeHP(-amount);
        DamageAnimate(amount);
        if (CurrentHealth <= 0)
        {
            Defeated?.Invoke(this);
        }
        TextParticle newText = new TextParticle();
        newText.Modulate = Colors.Red;
        Sprite.AddChild(newText);
        newText.Text = $"-{amount}";
    }

    public virtual void Heal(int amount)
    {
        CurrentHealth = HealthBar.ChangeHP(amount);
        amount = Math.Max(0, amount);
        if (amount == 0)
            return;
        DamageAnimate(-amount);
        TextParticle newText = new TextParticle();
        newText.Modulate = Colors.Green;
        Sprite.AddChild(newText);
        newText.Text = $"+{amount}";
    }

    public int GetCurrentHealth()
    {
        return CurrentHealth;
    }
}
