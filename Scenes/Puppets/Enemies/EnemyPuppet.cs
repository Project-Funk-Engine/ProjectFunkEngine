using System;

public partial class EnemyPuppet : PuppetTemplate
{
    protected EnemyEffect[] BattleEvents = Array.Empty<EnemyEffect>();
    public int BaseMoney { get; protected set; } = 0;
    public (int NoteId, int Amount) InitialNote = (0, 0);

    public virtual EnemyEffect[] GetBattleEvents()
    {
        return BattleEvents;
    }
}
