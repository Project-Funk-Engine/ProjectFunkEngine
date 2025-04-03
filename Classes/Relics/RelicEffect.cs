using System;
using FunkEngine;
using Godot;

/**
 * <summary>RelicEffect: BattleDirector effect as handled by a player passive relic.</summary>
 */
public partial class RelicEffect : IBattleEvent
{
    private BattleEffectTrigger Trigger { get; set; }
    private int _baseValue;
    public int Value;
    private Action<BattleDirector, RelicEffect, int> _onRelicEffect;
    private bool _effectPersists = false;

    public RelicEffect(
        BattleEffectTrigger trigger,
        int val,
        Action<BattleDirector, RelicEffect, int> onRelicEffect
    )
    {
        _baseValue = val;
        Value = _baseValue;
        Trigger = trigger;
        _onRelicEffect = onRelicEffect;
    }

    public void OnBattleEnd()
    {
        if (!_effectPersists)
            Value = _baseValue;
    }

    public void OnTrigger(BattleDirector battleDirector)
    {
        _onRelicEffect(battleDirector, this, Value);
    }

    public BattleEffectTrigger GetTrigger()
    {
        return Trigger;
    }
}
