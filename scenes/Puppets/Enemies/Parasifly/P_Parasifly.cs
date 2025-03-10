using System;
using Godot;

public partial class P_Parasifly : EnemyPuppet
{
    public override void _Ready()
    {
        base._Ready();
        var enemTween = CreateTween();
        enemTween.TweenProperty(Sprite, "position", Vector2.Down * 2, 2f).AsRelative();
        enemTween.TweenProperty(Sprite, "position", Vector2.Up * 2, 2f).AsRelative();
        enemTween.SetTrans(Tween.TransitionType.Linear);
        enemTween.SetEase(Tween.EaseType.In);
        enemTween.SetLoops();
        enemTween.Play();
    }
}
