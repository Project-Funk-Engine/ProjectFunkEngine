using System;
using FunkEngine;
using Godot;

public partial class RelicEffect : IBattleEvent
{
    private BattleEffectTrigger Trigger { get; set; }
    public int BaseValue;
    private Action<BattleDirector, int> OnRelicEffect;

    public RelicEffect(
        BattleEffectTrigger trigger,
        int val,
        Action<BattleDirector, int> onRelicEffect
    )
    {
        BaseValue = val;
        Trigger = trigger;
        OnRelicEffect = onRelicEffect;
    }

    public void OnTrigger(BattleDirector battleDirector)
    {
        OnRelicEffect(battleDirector, BaseValue);
    }

    public BattleEffectTrigger GetTrigger()
    {
        return Trigger;
    }
}
