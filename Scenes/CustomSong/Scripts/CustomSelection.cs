using System;
using System.Collections.Generic;
using System.IO;
using FunkEngine;
using FunkEngine.Classes.MidiMaestro;
using Godot;

public partial class CustomSelection : CanvasLayer, IFocusableMenu
{
    public const string LoadPath = "res://Scenes/CustomSong/CustomSelection.tscn";
    public const string UserSongDir = "user://Exports/";

    [Export]
    private Button _returnButton;

    [Export]
    private VBoxContainer _songList;

    [Export]
    private Label _songDescription;

    private DirAccess _dirAccess = DirAccess.Open(UserSongDir);
    readonly List<SongTemplate> _customSongs = new List<SongTemplate>();

    public override void _EnterTree()
    {
        _returnButton.Pressed += ReturnToPrev;

        if (!DirAccess.DirExistsAbsolute(UserSongDir))
            DirAccess.Open("user://").MakeDirRecursive(UserSongDir);
        _dirAccess = DirAccess.Open(UserSongDir);
        if (_dirAccess == null)
        {
            GD.PushWarning("Could not open user song directory!");
            return;
        }
        foreach (string file in _dirAccess.GetFiles())
        {
            if (file.GetExtension() == "sontem")
            {
                SongTemplate result = SongTemplate.CreateFromPath(UserSongDir + file);
                _customSongs.Add(result);
            }
        }
    }

    SongTemplate _selectedSong;

    public override void _Ready()
    {
        foreach (SongTemplate song in _customSongs)
        {
            DisplayButton newButton = GD.Load<PackedScene>(DisplayButton.LoadPath)
                .Instantiate<DisplayButton>();
            _songList.AddChild(newButton);
            if (song.Chart == null)
            {
                newButton.Text = Tr(song.Name);
                newButton.Disabled = true;
                newButton.FocusEntered += () =>
                {
                    _songDescription.Text = song.EnemyScenePath[0];
                };
                continue;
            }
            newButton.Text = song.Name;
            newButton.FocusEntered += () =>
            {
                _songDescription.Text =
                    song.Name + "\n" + song.Chart.Bpm + "\n" + song.Chart.NumLoops;
                _selectedSong = song;
            };
            newButton.Pressed += StartCustomSelection;
        }
    }

    private void StartCustomSelection()
    {
        BgAudioPlayer.LiveInstance.StopMusic();
        StageProducer.LiveInstance.TransitionToCustom(_selectedSong);
    }

    #region IFocusableMenu
    public IFocusableMenu Prev { get; set; }
    private ProcessModeEnum _previousProcessMode;

    [Export]
    private Control _focused;

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
        StageProducer.LiveInstance.LastStage = Stages.Title;
        Prev.ResumeFocus();
        QueueFree();
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
    #endregion
}
