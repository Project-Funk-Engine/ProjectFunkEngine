using System;
using FunkEngine;

public class EnemyEffect : IBattleEvent
{
    private BattleEffectTrigger Trigger { get; set; }
    public EnemyPuppet Owner;
    private int _baseValue;
    public int Value;
    private Action<BattleEventArgs, EnemyEffect, int> _onEnemyEffect;

    public EnemyEffect(
        EnemyPuppet owner,
        BattleEffectTrigger trigger,
        int val,
        Action<BattleEventArgs, EnemyEffect, int> onEnemyEffect
    )
    {
        Owner = owner;
        _baseValue = val;
        Value = _baseValue;
        Trigger = trigger;
        _onEnemyEffect = onEnemyEffect;
    }

    public void OnTrigger(BattleEventArgs e)
    {
        _onEnemyEffect(e, this, Value);
    }

    public BattleEffectTrigger GetTrigger()
    {
        return Trigger;
    }
}
