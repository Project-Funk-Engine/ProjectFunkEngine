using System;

public partial class EnemyPuppet : PuppetTemplate
{
    protected EnemyEffect[] BattleEvents = Array.Empty<EnemyEffect>();
    public int BaseMoney = 0;

    public virtual EnemyEffect[] GetBattleEvents()
    {
        return BattleEvents;
    }
}
