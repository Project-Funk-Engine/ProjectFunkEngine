using System;
using System.Collections.Generic;
using System.Linq;
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
    public Vector2 InitScale = Vector2.One;

    [Export]
    public bool HideHealth;

    private Vector2 _startPos;

    protected int MaxHealth = 100;
    protected int CurrentHealth = 100;

    protected string UniqName = ""; //Eventually make subclasses/scenes/real stuff

    public override void _Ready()
    {
        HealthBar.SetHealth(MaxHealth, CurrentHealth);
        _startPos = Position;
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
            _startPos
            + new Vector2(
                (float)GD.RandRange(-_shakeStrength, _shakeStrength),
                (float)GD.RandRange(-_shakeStrength, _shakeStrength)
            );
    }

    protected virtual Tween DamageAnimate(int amount)
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
        tween.Chain().TweenProperty(this, "position", _startPos, 2 * _baseAnimDuration);
        return tween;
    }

    protected virtual void Kill()
    {
        Defeated?.Invoke(this);
    }
    #endregion

    public virtual void TakeDamage(DamageInstance dmg)
    {
        BattleDirector.Harbinger.Instance.InvokeOnDamageInstance(dmg);
        int amount = dmg.Damage;

        amount = Math.Max(0, amount); //Should not be able to heal from damage.
        if (CurrentHealth <= 0 || amount == 0)
            return; //Only check if hp would change
        CurrentHealth = HealthBar.ChangeHP(-amount);
        Tween deathTween = DamageAnimate(amount);
        if (CurrentHealth <= 0)
        {
            deathTween.Finished += Kill;
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

    #region Status Effects
    /// <summary>
    /// The visual indicators for status effects.
    /// </summary>
    [Export]
    protected GridContainer _statusContainer;
    const int MaxStatuses = 8;
    protected List<StatusEffect> StatusEffects = new List<StatusEffect>();

    /// <summary>
    /// Returns true if it could be successfully added as a new status effect. False if it is stacking, or can't add.
    /// ONLY call this from within BattleDirector's AddStatus
    /// </summary>
    /// <param name="effect">The status effect to add.</param>
    /// <returns></returns>
    public bool AddStatusEffect(StatusEffect effect)
    {
        int index = StatusEffects.FindIndex(sEff => sEff.StatusName == effect.StatusName);
        if (index != -1) //If status of same name -> stack -> return false
        {
            StatusEffects[index].StackEffect(effect);
            return false;
        }
        if (StatusEffects.Count >= MaxStatuses)
            return false; //Max status effects -> return false

        //Add status effect -> true
        _statusContainer.AddChild(effect);
        effect.SetOwner(this);
        StatusEffects.Add(effect);
        return true;
    }

    public void RemoveStatusEffect(StatusEffect effect)
    {
        _statusContainer.RemoveChild(effect);
        StatusEffects.Remove(effect);
        effect.QueueFree();
    }
    #endregion
}
