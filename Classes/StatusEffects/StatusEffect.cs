using System;
using System.Linq;
using FunkEngine;
using Godot;

/// <summary>
/// Status Effect class.
/// Preferably set up as a static default status, then apply status using GetInstance, but custom statuses can be defined elsewhere.
/// Invoke StatusEnd to remove status.
/// </summary>
public partial class StatusEffect : TextureRect, IBattleEvent
{ //TODO: Status effects that are permanent, and status effects that don't take up a slot/are invisible
    public static readonly string LoadPath = "res://Classes/StatusEffects/StatusIcon.tscn";
    public PuppetTemplate Sufferer { get; private set; }
    public int Count { get; private set; }

    public string StatusName { get; private set; }

    [Export]
    private Label CountLabel { get; set; }

    internal delegate void StatusEndHandler(StatusEffect status);
    internal event StatusEndHandler StatusEnd;

    #region DefaultStatuses
    private static readonly Action<BattleEventArgs, StatusEffect> BlockEffect = (e, self) =>
    {
        if (e is BattleDirector.Harbinger.OnDamageInstanceArgs dmgArgs)
        {
            if (dmgArgs.Dmg.Target != self.Sufferer || dmgArgs.Dmg.Damage <= 0)
                return;
            dmgArgs.Dmg.ModifyDamage(0, 0);
            self.DecCount();
        }
    };

    /// <summary>
    /// On the owner receiving a damage instance, if valid (correct target and dmg > 0) sets damage to 0 and reduces count.
    /// </summary>
    public static readonly StatusEffect Block = new StatusEffect()
        .InitStatus(
            "Block",
            BlockEffect,
            BattleEffectTrigger.OnDamageInstance,
            GD.Load<Texture2D>("res://Classes/StatusEffects/Assets/Status_Block.png")
        )
        .SetTags(true);

    #endregion

    private BattleEffectTrigger _trigger;
    private Action<BattleEventArgs, StatusEffect> _effect;

    public BattleEffectTrigger GetTrigger()
    {
        return _trigger;
    }

    public void OnTrigger(BattleEventArgs e)
    {
        _effect(e, this);
    }

    public StatusEffect InitStatus(
        string name,
        Action<BattleEventArgs, StatusEffect> effect,
        BattleEffectTrigger trigger,
        Texture2D texture = null
    )
    {
        _effect = effect;
        _trigger = trigger;
        StatusName = name;
        Texture = texture;
        return this;
    }

    public StatusEffect GetInstance(int count = 1)
    {
        StatusEffect result = GD.Load<PackedScene>(LoadPath).Instantiate<StatusEffect>();
        result.SetCount(count);
        result.InitStatus(Name, _effect, _trigger, Texture);
        result.SetTags(_stackable, _refreshes);
        return result;
    }

    public void SetOwner(PuppetTemplate owner)
    {
        Sufferer = owner;
    }

    public void IncCount(int count = 1)
    {
        SetCount(Count + count);
    }

    public void DecCount(int count = 1)
    {
        SetCount(Count - count);
    }

    public void SetCount(int count)
    {
        Count = count;
        CountLabel.Text = Count.ToString();
        if (Count <= 0)
        {
            StatusEnd?.Invoke(this);
        }
    }

    /// <summary>
    /// Re-applying a status increases the count.
    /// </summary>
    private bool _stackable;

    /// <summary>
    /// Re-applying a status sets the count to the higher counte
    /// </summary>
    private bool _refreshes;

    public StatusEffect SetTags(bool stackable = false, bool refreshes = false)
    {
        _stackable = stackable;
        _refreshes = refreshes;
        return this;
    }

    //Called if a puppet is receiving a duplicate effect.
    public void StackEffect(StatusEffect incomingEffect)
    {
        if (incomingEffect.StatusName != StatusName)
            return;
        if (_stackable)
        {
            IncCount(incomingEffect.Count);
        }

        if (_refreshes && incomingEffect.Count >= Count)
        {
            SetCount(incomingEffect.Count);
        }
    }
}
