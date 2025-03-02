using System;
using FunkEngine;
using Godot;

public partial class EnemyPuppet : PuppetTemplate
{
    //TODO: What do enemies need?
    protected EnemyEffect[] _battleEvents = Array.Empty<EnemyEffect>();

    public virtual EnemyEffect[] GetBattleEvents()
    {
        return _battleEvents;
    }
}
