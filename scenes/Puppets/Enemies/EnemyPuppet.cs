using System;

public partial class EnemyPuppet : PuppetTemplate
{
    protected EnemyEffect[] BattleEvents = Array.Empty<EnemyEffect>();

    public virtual EnemyEffect[] GetBattleEvents()
    {
        return BattleEvents;
    }
}
