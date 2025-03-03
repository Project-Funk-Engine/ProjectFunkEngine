using System;
using Godot;

public partial class P_TheGWS : EnemyPuppet
{
    public override void _Ready()
    {
        base._Ready();
        var enemTween = CreateTween();
        enemTween.TweenProperty(Sprite, "position", Vector2.Down * 10, 3f).AsRelative();
        enemTween.TweenProperty(Sprite, "position", Vector2.Up * 10, 3f).AsRelative();
        enemTween.SetTrans(Tween.TransitionType.Quad);
        enemTween.SetEase(Tween.EaseType.InOut);
        enemTween.SetLoops();
        enemTween.Play();
    }
}
