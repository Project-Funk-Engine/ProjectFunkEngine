using System;
using FunkEngine;
using Godot;

public partial class EnemyPuppet : PuppetTemplate
{
    protected EnemyEffect[] _battleEvents = Array.Empty<EnemyEffect>();

    public virtual EnemyEffect[] GetBattleEvents()
    {
        return _battleEvents;
    }
}
