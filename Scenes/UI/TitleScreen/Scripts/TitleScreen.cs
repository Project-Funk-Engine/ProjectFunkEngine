using FunkEngine;
using Godot;

public partial class TitleScreen : Control, IFocusableMenu
{
    public static readonly string LoadPath = "res://Scenes/UI/TitleScreen/TitleScreen.tscn";
    private static readonly string EffectsLoadPath =
        "res://Scenes/UI/TitleScreen/TitleScreenEffects.tscn";

    [Export]
    private Node _effectsPlaceholder;
    private Node _effectsRoot;
    public PointLight2D TextLight;

    [Export]
    public Button Options;

    private Control _focused;
    public IFocusableMenu Prev { get; set; }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey eventKey && eventKey.Pressed && !eventKey.Echo)
        {
            if (eventKey.Keycode == Key.Key0)
            {
                SteamWhisperer.ResetAll();
                SaveSystem.ClearSave();
                SaveSystem.ClearConfig();
                StageProducer.LiveInstance.InitFromCfg();
            }
        }
    }

    public override void _EnterTree()
    {
        BgAudioPlayer.LiveInstance.PlayLevelMusic();
    }

    public override void _Ready()
    {
        Options.Pressed += OpenOptions;
    }

    public override void _Process(double delta)
    {
        if (TextLight == null)
            InitEffects();
    }

    public void ResumeFocus()
    {
        ProcessMode = ProcessModeEnum.Inherit;
        _focused.GrabFocus();
    }

    public void PauseFocus()
    {
        _focused = GetViewport().GuiGetFocusOwner();
        ProcessMode = ProcessModeEnum.Disabled;
    }

    public void OpenMenu(IFocusableMenu prev)
    {
        GD.PushWarning("Undefined behaviour, TitleScreen should not be opened!");
    }

    public void ReturnToPrev()
    {
        GD.PushWarning("Undefined behaviour, TitleScreen should not return to previous!");
    }

    private void OpenOptions()
    {
        OptionsMenu optionsMenu = GD.Load<PackedScene>(OptionsMenu.LoadPath)
            .Instantiate<OptionsMenu>();
        AddChild(optionsMenu);
        optionsMenu.OpenMenu(this);
    }

    private void InitEffects()
    {
        if (_effectsPlaceholder is not InstancePlaceholder placeholder)
            return;
        _effectsRoot = placeholder.CreateInstance(true, GD.Load<PackedScene>(EffectsLoadPath));
        TextLight = _effectsRoot.GetNode<PointLight2D>("TextLight");
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
}
