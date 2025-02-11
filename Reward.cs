using System;
using System.Linq;
using Godot;

public static class Reward
{
    private static readonly Random _rng = new Random();

    public static void GiveRandomRelic(PlayerStats player)
    {
        RelicTemplate newRelic = GetRandomRelic(player.CurRelics);

        if (newRelic != null)
        {
            AddRelic(player, newRelic);
            GD.Print("Relic added: " + newRelic.Name);
        }
        else
        {
            GD.Print("No new relic to collect");
        }
    }

    public static RelicTemplate GetRandomRelic(RelicTemplate[] ownedRelics)
    {
        var availableRelics = Scribe
            .RelicDictionary.Where(r => !ownedRelics.Any(o => o.Name == r.Name))
            .ToArray();

        if (availableRelics.Length == 0)
        {
            return null; // No new relics available
        }

        int index = _rng.Next(availableRelics.Length);
        return availableRelics[index].Clone();
    }

    public static void AddRelic(PlayerStats player, RelicTemplate relic)
    {
        if (player.CurRelics.Any(r => r.Name == relic.Name))
        {
            GD.Print("Relic already in inventory: " + relic.Name);
            return;
        }
        player.CurRelics = player.CurRelics.Append(relic).ToArray();
        GD.Print("Adding relic: " + relic.Name);
    }
}
