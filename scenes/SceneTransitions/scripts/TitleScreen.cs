using System;
using Godot;

public partial class TitleScreen : Control
{
    public override void _EnterTree()
    {
        GetNode<BgAudioPlayer>("/root/BgAudioPlayer").PlayLevelMusic();
    }
}
