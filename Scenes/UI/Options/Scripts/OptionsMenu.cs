using FunkEngine;
using Godot;

public partial class OptionsMenu : CanvasLayer, IFocusableMenu
{
    public static readonly string LoadPath = "res://Scenes/UI/Options/OptionsMenu.tscn";

    public IFocusableMenu Prev { get; set; }

    [Export]
    private Control _focused;
    private ProcessModeEnum _previousProcessMode;

    [Export]
    private HSlider _volumeSlider;

    [Export]
    private Button _closeButton;

    [Export]
    private Button _controlsButton;

    [Export]
    private CheckBox _highContrastToggle;

    [Export]
    private Button _howToPlayButton;

    [Export]
    private MarginContainer _titleScreenOptions;

    [Export]
    private CheckBox _noteSpriteToggle;

    [Export]
    private CheckBox _verticalScrollToggle;

    private const float MinVolumeVal = 0f;

    public override void _Ready()
    {
        _volumeSlider.MinValue = MinVolumeVal;
        _volumeSlider.Value = Mathf.DbToLinear(
            AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Master"))
        );

        _highContrastToggle.ButtonPressed = SaveSystem
            .GetConfigValue(SaveSystem.ConfigSettings.HighContrast)
            .AsBool();

        _volumeSlider.DragEnded += VolumeChanged;
        _volumeSlider.ValueChanged += ChangeVolume;

        _closeButton.Pressed += ReturnToPrev;
        _controlsButton.Pressed += OpenControls;
        _highContrastToggle.Toggled += HighContrastChanged;
        _howToPlayButton.Pressed += OpenHowToPlay;

        _titleScreenOptions.Visible =
            !StageProducer.IsInitialized
            && !SaveSystem.GetConfigValue(SaveSystem.ConfigSettings.FirstTime).AsBool();
        _noteSpriteToggle.ButtonPressed = InputHandler.UseArrows;
        _noteSpriteToggle.Toggled += ArrowSpritesToggled;
        _verticalScrollToggle.ButtonPressed = BattleDirector.VerticalScroll;
        _verticalScrollToggle.Toggled += VerticalScrollToggled;

        _controlsButton.Visible = StageProducer.IsJoy || StageProducer.IsKeyBoard;
    }

    public override void _Input(InputEvent @event)
    {
        if (ControlSettings.IsOutOfFocus(this))
        {
            GetViewport().SetInputAsHandled();
            return;
        }
        if (@event.IsActionPressed("ui_cancel"))
        {
            ReturnToPrev();
            GetViewport().SetInputAsHandled();
        }
    }

    public void ResumeFocus()
    {
        ProcessMode = _previousProcessMode;
        _focused.GrabFocus();
    }

    public void PauseFocus()
    {
        _focused = GetViewport().GuiGetFocusOwner();
        _previousProcessMode = ProcessMode;
        ProcessMode = ProcessModeEnum.Disabled;
    }

    public void OpenMenu(IFocusableMenu prev)
    {
        Prev = prev;
        Prev.PauseFocus();
        _focused.GrabFocus();
    }

    public void ReturnToPrev()
    {
        Prev.ResumeFocus();
        QueueFree();
    }

    private void OpenControls()
    {
        ControlSettings controlSettings = GD.Load<PackedScene>(ControlSettings.LoadPath)
            .Instantiate<ControlSettings>();
        AddChild(controlSettings);
        controlSettings.OpenMenu(this);
    }

    private void VolumeChanged(bool valueChanged)
    {
        if (!valueChanged)
            return;
        ChangeVolume((float)_volumeSlider.Value);
        SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.Volume, _volumeSlider.Value);
    }

    public static void ChangeVolume(double value)
    {
        AudioServer.SetBusVolumeDb(
            AudioServer.GetBusIndex("Master"),
            (float)Mathf.LinearToDb(value)
        );
    }

    private void ArrowSpritesToggled(bool value)
    {
        InputHandler.UseArrows = value;
        SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.TypeIsArrow, value);
    }

    private void VerticalScrollToggled(bool value)
    {
        BattleDirector.VerticalScroll = value;
        SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.VerticalScroll, value);
    }

    private void HighContrastChanged(bool toggled)
    {
        StageProducer.ContrastFilter.Visible = toggled;
        SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.HighContrast, toggled);
    }

    private void OpenHowToPlay()
    {
        HowToPlay howtoPlay = GD.Load<PackedScene>(HowToPlay.LoadPath).Instantiate<HowToPlay>();
        AddChild(howtoPlay);
        howtoPlay.OpenMenu(this);
    }

    private void OpenCredits()
    {
        CreditsMenu creditsMenu = GD.Load<PackedScene>(CreditsMenu.LoadPath)
            .Instantiate<CreditsMenu>();
        AddChild(creditsMenu);
        creditsMenu.OpenMenu(this);
    }
}
