using System;
using Godot;

public partial class TitleScreen : Control
{
    [Export]
    public PointLight2D TextLight;

    public override void _Ready()
    {
        TweenLight();
    }

    private void TweenLight()
    {
        var tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Linear);
        tween.TweenProperty(TextLight, "energy", 7, 5f);
        tween.TweenProperty(TextLight, "energy", 2, 5f);
        tween.SetLoops(-1);
        tween.Play();
    }

    public override void _EnterTree()
    {
        GetNode<BgAudioPlayer>("/root/BgAudioPlayer").PlayLevelMusic();
    }
}
