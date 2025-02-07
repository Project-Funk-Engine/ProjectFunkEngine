using System;
using FunkEngine;
using Godot;

public class RelicDict
{
    public static RelicTemplate[] RelicPool = new[]
    {
        new RelicTemplate(
            "Good Vibes",
            new RelicEffect[]
            {
                new RelicEffect(
                    "NotePlaced",
                    5,
                    (director, val) =>
                    {
                        director.Player.Heal(val);
                    }
                ),
            }
        ),
    };
}
