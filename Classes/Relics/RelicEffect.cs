using System;
using FunkEngine;
using Godot;

public partial class RelicEffect : IBattleEvent
{
    private string Trigger { get; set; }
    public int BaseValue;
    private Action<BattleDirector, int> OnRelicEffect;

    public RelicEffect(string trigger, int val, Action<BattleDirector, int> onRelicEffect)
    {
        BaseValue = val;
        Trigger = trigger;
        OnRelicEffect = onRelicEffect;
    }

    public void OnTrigger(BattleDirector battleDirector)
    {
        OnRelicEffect(battleDirector, BaseValue);
    }

    public string GetTrigger()
    {
        return Trigger;
    }
}
