using System.Threading.Tasks;
using FunkEngine;
using Godot;

public partial class TitleScreen : Control, IFocusableMenu
{
    public static readonly string LoadPath = "res://Scenes/UI/TitleScreen/TitleScreen.tscn";
    public static readonly string EffectsLoadPath =
        "res://Scenes/UI/TitleScreen/TitleScreenEffects.tscn";

    [Export]
    private Node _effectsPlaceholder;
    private Node _effectsRoot;
    public PointLight2D TextLight;

    [Export]
    public Button Options;

    [Export]
    private Button _customSelectionButton;

    private Control _focused;
    public IFocusableMenu Prev { get; set; }

    public override void _EnterTree()
    {
        BgAudioPlayer.LiveInstance.PlayLevelMusic();
        Options.Pressed += OpenOptions;
        _customSelectionButton.Pressed += OpenCustomSelection;
    }

    public override void _Ready()
    {
        if (StageProducer.LiveInstance.LastStage == Stages.Custom)
            OpenCustomSelection();
        // _customSelectionButton.Visible = (bool)
        //     SaveSystem.GetConfigValue(SaveSystem.ConfigSettings.HasWon);
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

    private void OpenCustomSelection()
    {
        CustomSelection customMenu = GD.Load<PackedScene>(CustomSelection.LoadPath)
            .Instantiate<CustomSelection>();
        AddChild(customMenu);
        customMenu.OpenMenu(this);
    }

    private bool taskStarted = false;

    private void InitEffects()
    {
        if (taskStarted || _effectsPlaceholder is not InstancePlaceholder placeholder)
            return;

        taskStarted = true;
        Task.Run(() => //Will need to monitor to make sure this is safe
        {
            Callable
                .From(() =>
                {
                    _effectsRoot = placeholder.CreateInstance(
                        true,
                        GD.Load<PackedScene>(EffectsLoadPath)
                    );
                    TextLight = _effectsRoot.GetNode<PointLight2D>("TextLight");
                    TweenLight();
                })
                .CallDeferred();
        });
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
