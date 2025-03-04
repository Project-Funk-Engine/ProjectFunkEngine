using System;
using Godot;

public partial class TitleScreen : Control
{
    [Export]
    public PointLight2D TextLight;

    [Export]
    public Button Options;

    public override void _Ready()
    {
        TweenLight();
        Options.Pressed += OpenOptions;
    }

    public override void _Process(double delta)
    {
        if (GetViewport().GuiGetFocusOwner() == null) //TODO: Better method for returning focus
        {
            Options.GrabFocus();
        }
    }

    private void OpenOptions()
    {
        OptionsMenu optionsMenu = GD.Load<PackedScene>("res://scenes/Options/OptionsMenu.tscn")
            .Instantiate<OptionsMenu>();
        AddChild(optionsMenu);
        optionsMenu.OpenMenu(this);
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
