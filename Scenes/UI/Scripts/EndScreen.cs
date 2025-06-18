using FunkEngine;
using Godot;

public partial class EndScreen : CanvasLayer, IFocusableMenu
{
    public static readonly string LoadPath = "res://Scenes/UI/EndScreen.tscn";

    [Export]
    private Button[] _buttons;

    [Export]
    private MarginContainer _creditsCont;

    [Export]
    public Label TopLabel;

    public bool HasWon = false;

    public override void _Ready()
    {
        _buttons[0].Pressed += Restart;
        _buttons[1].Pressed += QuitToMainMenu;
        _buttons[2].Pressed += Quit;
        _buttons[3].Pressed += OpenCredits;
        _buttons[0].GrabFocus();
        _creditsCont.Visible = HasWon;
        BgAudioPlayer.LiveInstance.PlayLevelMusic();
    }

    private void Restart()
    {
        GetTree().Paused = false;
        StageProducer.IsInitialized = false;
        StageProducer.LiveInstance.TransitionStage(Stages.Map);
    }

    private void Quit()
    {
        StageProducer.LiveInstance.TransitionStage(Stages.Quit);
    }

    private void QuitToMainMenu()
    {
        GetTree().Paused = false;
        StageProducer.LiveInstance.TransitionStage(Stages.Title);
    }

    private void OpenCredits()
    {
        CreditsMenu credits = GD.Load<PackedScene>(CreditsMenu.LoadPath).Instantiate<CreditsMenu>();
        AddChild(credits);
        credits.OpenMenu(this);
    }

    public IFocusableMenu Prev { get; set; }

    public void ResumeFocus()
    {
        ProcessMode = ProcessModeEnum.Always;
        _buttons[3].GrabFocus(); //Should only get resumed from Credits
    }

    public void PauseFocus()
    {
        ProcessMode = ProcessModeEnum.Disabled;
    }

    public void OpenMenu(IFocusableMenu prev)
    {
        GD.PushWarning("EndScreen OpenMenu not implemented yet!");
        Prev = prev;
        Prev.PauseFocus();
    }

    public void ReturnToPrev()
    {
        GD.PushError("EndScreen should not return to previous menu!");
    }
}
